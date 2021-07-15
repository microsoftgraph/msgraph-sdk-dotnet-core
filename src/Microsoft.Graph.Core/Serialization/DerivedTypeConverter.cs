// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// Handles resolving interfaces to the correct derived class during serialization/deserialization.
    /// </summary>
    public class DerivedTypeConverter<T> : JsonConverter<T> where T : class
    {
        internal static readonly ConcurrentDictionary<string, Type> TypeMappingCache = new ConcurrentDictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Checks if the given object can be converted. In this instance, all object can be converted.
        /// </summary>
        /// <param name="objectType">The type of the object to convert.</param>
        /// <returns>True</returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableFrom(typeof(T));
        }

        /// <summary>
        /// Deserializes the object to the correct type.
        /// </summary>
        /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> for deserialization.</param>
        /// <returns></returns>
        public override T Read(ref Utf8JsonReader reader, Type objectType, JsonSerializerOptions options)
        {
            JsonDocument jsonDocument = JsonDocument.ParseValue(ref reader);
            JsonElement type;
            try
            {
                // try to get the @odata.type property if we can
                if (!jsonDocument.RootElement.TryGetProperty(CoreConstants.Serialization.ODataType, out type))
                {
                    type = default;
                }
            }
            catch (InvalidOperationException)
            {
                type = default;
            }

            object instance;
            if (type.ValueKind != JsonValueKind.Undefined)
            {
                var typeString = type.ToString();
                typeString = typeString.TrimStart('#');
                typeString = StringHelper.ConvertTypeToTitleCase(typeString);
                var typeAssembly = objectType.GetTypeInfo().Assembly;
                // Use the type assembly as part of the key since users might use v1 and beta at the same causing conflicts
                var typeMappingCacheKey = $"{typeAssembly.FullName} : {typeString}";

                if (DerivedTypeConverter<T>.TypeMappingCache.TryGetValue(typeMappingCacheKey, out var instanceType))
                {
                    instance = this.Create(instanceType);
                }
                else
                {
                    instance = this.Create(typeString, typeAssembly);
                }

                // If @odata.type is set but we aren't able to create an instance of it use the method-provided object type instead.
                // This means unknown types will be deserialized as a parent type.
                // Also if the @odata.type is set but the type is not assignable to the method provided type e.g they are not related by inheritance
                // also use the parent type object. 
                if (instance == null || !objectType.IsAssignableFrom(instance.GetType()))
                {
                    instance = this.Create(objectType.AssemblyQualifiedName, /* typeAssembly */ null);
                }

                if (instance != null && instanceType == null)
                {
                    // Cache the type mapping resolution if we haven't pulled it from the cache already.
                    DerivedTypeConverter<T>.TypeMappingCache.TryAdd(typeMappingCacheKey, instance.GetType());
                }
            }
            else
            {
                instance = this.Create(objectType.AssemblyQualifiedName, /* typeAssembly */ null);
            }

            if (instance == null)
            {
                throw new ServiceException(
                    new Error
                    {
                        Code = ErrorConstants.Codes.GeneralException,
                        Message = string.Format(
                            ErrorConstants.Messages.UnableToCreateInstanceOfTypeFormatString,
                            objectType.AssemblyQualifiedName),
                    });
            }

            PopulateObject(instance, jsonDocument.RootElement, options);
            return (T)instance;
        }

        /// <summary>
        /// Populate an existing object with properties rather than create a new object. This custom implementation will be obsolete once
        /// System.Text.Json add support for this.
        /// Note : As this is a converter for derived type the expected inputs are either object or collection not value types.
        /// </summary>
        /// <param name="target">The target object</param>
        /// <param name="json">The json element undergoing deserialization</param>
        /// <param name="options">The options to use for deserialization.</param>
        private void PopulateObject(object target, JsonElement json, JsonSerializerOptions options)
        {
            // We use the target type information since it maybe be derived. We do not want to leave out extra properties in the child class and put them in the additional data unnecessarily
            Type objectType = target.GetType();
            switch (json.ValueKind)
            {
                case JsonValueKind.Object:
                {
                    // iterate through the object properties
                    foreach (var property in json.EnumerateObject())
                    {
                        // look up the property in the object definition using the mapping provided in the model attribute
                        var propertyInfo = objectType.GetProperties().FirstOrDefault((mappedProperty) =>
                        {
                            var attribute = mappedProperty.GetCustomAttribute<JsonPropertyNameAttribute>();
                            return attribute?.Name == property.Name;
                        });
                        if (propertyInfo == null)
                        {
                            //Add the property to AdditionalData as it doesn't exist as a member of the object
                            AddToAdditionalDataBag(target, objectType, property);
                            continue;
                        }

                        try
                        {
                            // Deserialize the property in and update the current object.
                            var parsedValue = JsonSerializer.Deserialize(property.Value.GetRawText(), propertyInfo.PropertyType, options);
                            propertyInfo.SetValue(target, parsedValue);
                        }
                        catch (JsonException)
                        {
                            //Add the property to AdditionalData as it can't be deserialized as a member. Eg. non existing enum member type
                            AddToAdditionalDataBag(target, objectType, property);
                        }
                    }

                    break;
                }
                case JsonValueKind.Array:
                {
                    //Its most likely a collectionPage instance so get its CurrentPage property
                    var collectionPropertyInfo = objectType.GetProperty("CurrentPage", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                    if (collectionPropertyInfo != null)
                    {
                        // Get the generic type info for deserialization
                        Type genericType = collectionPropertyInfo.PropertyType.GenericTypeArguments.FirstOrDefault();
                        int index = 0;
                        foreach (var property in json.EnumerateArray())
                        {
                            // Get the object instance
                            var instance = JsonSerializer.Deserialize(property.GetRawText(), genericType, options);

                            // Invoke the insert function to add it to the collection as it an IList
                            MethodInfo methodInfo = collectionPropertyInfo.PropertyType.GetMethods().FirstOrDefault(method => method.Name.Equals("Insert"));
                            object[] parameters = new object[] { index, instance };
                            if (methodInfo != null)
                            {
                                methodInfo.Invoke(target, parameters);//insert the object to the page List
                                index++;
                            }
                        }
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// Adds unknown elements to a property that has the JsonExtensionData attribute. This is not
        /// done for us automagically since we hare using a custom converter
        /// </summary>
        /// <param name="target">The target object</param>
        /// <param name="objectType">The object type</param>
        /// <param name="property">The json property</param>
        private void AddToAdditionalDataBag(object target, Type objectType, JsonProperty property)
        {
            // Get the property with the JsonExtensionData attribute and add the property to the collection
            var additionalDataInfo = objectType.GetProperties().FirstOrDefault(propertyInfo => ((MemberInfo) propertyInfo).GetCustomAttribute<JsonExtensionDataAttribute>() != null);
            if (additionalDataInfo != null)
            {
                var additionalData = additionalDataInfo.GetValue(target) as IDictionary<string, object> ?? new Dictionary<string, object>();
                additionalData.Add(property.Name, property.Value);
                additionalDataInfo.SetValue(target, additionalData);
            }
        }

        /// <summary>
        /// Write out json from existing object
        /// </summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write with</param>
        /// <param name="value">The object to write</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> to write out with</param>
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var propertyInfo in value.GetType().GetProperties())
            {
                var ignoreConverterAttribute = propertyInfo.GetCustomAttribute<System.Text.Json.Serialization.JsonIgnoreAttribute>();
                if(ignoreConverterAttribute != null)
                {
                    continue;// Don't serialize a property we are asked to ignore
                }

                string propertyName;
                // Try to get the property name off the JsonAttribute otherwise camel case the property name
                var jsonProperty = propertyInfo.GetCustomAttribute<System.Text.Json.Serialization.JsonPropertyNameAttribute>();
                if (jsonProperty != null && !string.IsNullOrWhiteSpace(jsonProperty.Name))
                {
                    propertyName = jsonProperty.Name;
                }
                else
                {
                    propertyName = StringHelper.ConvertTypeToLowerCamelCase(propertyInfo.Name);
                }

                // Check so that we are not serializing the JsonExtensionDataAttribute(i.e AdditionalData) at a nested level
                var jsonExtensionData = propertyInfo.GetCustomAttribute<System.Text.Json.Serialization.JsonExtensionDataAttribute>();
                if (jsonExtensionData != null)
                {
                    var additionalData = propertyInfo.GetValue(value) as IDictionary<string, object> ?? new Dictionary<string, object>();
                    foreach (var item in additionalData)
                    {
                        writer.WritePropertyName(item.Key);
                        JsonSerializer.Serialize(writer, item.Value, item.Value.GetType(), options);
                    }
                }
                else
                {
                    // Check to see if the property has a special converter specified
                    var jsonConverter = propertyInfo.GetCustomAttribute<System.Text.Json.Serialization.JsonConverterAttribute>();
                    if (propertyInfo.GetValue(value) == null && jsonConverter == null)
                    {
                        continue; //Don't do anything if we don't have a special converter or the value is null
                    }

                    writer.WritePropertyName(propertyName);
                    JsonSerializer.Serialize(writer, propertyInfo.GetValue(value), propertyInfo.PropertyType, options);
                }
            }
            writer.WriteEndObject();
        }

        private object Create(string typeString, Assembly typeAssembly)
        {
            Type type = null;

            if (typeAssembly != null)
            {
                type = typeAssembly.GetType(typeString);
            }
            else
            {
                type = Type.GetType(typeString);
            }

            return this.Create(type);
        }

        private object Create(Type type)
        {
            if (type == null)
            {
                return null;
            }

            try
            {
                // Find the default constructor. Abstract entity classes use non-public constructors.
                var constructorInfo = type.GetTypeInfo().DeclaredConstructors.FirstOrDefault(
                    constructor => !constructor.GetParameters().Any() && !constructor.IsStatic);

                if (constructorInfo == null)
                {
                    return null;
                }

                return constructorInfo.Invoke( new object[] { } );
            }
            catch (Exception exception)
            {
                throw new ServiceException(
                    new Error
                    {
                        Code = ErrorConstants.Codes.GeneralException,
                        Message = string.Format(ErrorConstants.Messages.UnableToCreateInstanceOfTypeFormatString, type.FullName),
                    },
                    exception);
            }
        }
    }
}
