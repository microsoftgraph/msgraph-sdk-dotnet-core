// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Serialization
{
    using Microsoft.Graph.DotnetCore.Core.Test.TestModels;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Xunit;
    public class SerializerTests
    {
        private Serializer serializer;

        public SerializerTests()
        {
            this.serializer = new Serializer();
        }

        [Fact]
        public void AbstractClassDeserializationFailure()
        {
            var stringToDeserialize = "{\"jsonKey\":\"jsonValue\"}";
            ServiceException exception = Assert.Throws<ServiceException>(() => this.serializer.DeserializeObject<AbstractClass>(stringToDeserialize));
            Assert.True(exception.IsMatch(ErrorConstants.Codes.GeneralException));
            Assert.Equal(
                string.Format(ErrorConstants.Messages.UnableToCreateInstanceOfTypeFormatString, typeof(AbstractClass).FullName),
                exception.Error.Message);
        }

        [Fact]
        public void DeserializeDerivedType()
        {
            var id = "id";
            var name = "name";

            var stringToDeserialize = string.Format(
                "{{\"id\":\"{0}\", \"@odata.type\":\"#microsoft.graph.dotnetCore.core.test.testModels.derivedTypeClass\", \"name\":\"{1}\"}}",
                id,
                name);

            var derivedType = this.serializer.DeserializeObject<DerivedTypeClass>(stringToDeserialize);

            Assert.NotNull(derivedType);
            Assert.Equal(id, derivedType.Id);
            Assert.Equal(name, derivedType.Name);
        }

        [Fact]
        public void DeserializeInvalidODataType()
        {
            var id = "id";
            var givenName = "name";

            var stringToDeserialize = string.Format(
                "{{\"id\":\"{0}\", \"@odata.type\":\"invalid\", \"givenName\":\"{1}\"}}",
                id,
                givenName);

            var instance = this.serializer.DeserializeObject<DerivedTypeClass>(stringToDeserialize);

            Assert.NotNull(instance);
            Assert.Equal(id, instance.Id);
            Assert.NotNull(instance.AdditionalData);
            Assert.Equal(givenName, instance.AdditionalData["givenName"].ToString());
        }

        [Fact]
        public void DeserializeStream()
        {
            var id = "id";

            var stringToDeserialize = string.Format("{{\"id\":\"{0}\"}}", id);

            using (var serializedStream = new MemoryStream(Encoding.UTF8.GetBytes(stringToDeserialize)))
            {
                var instance = this.serializer.DeserializeObject<DerivedTypeClass>(serializedStream);

                Assert.NotNull(instance);
                Assert.Equal(id, instance.Id);
                Assert.Null(instance.AdditionalData);
            }
        }

        [Fact]
        public void DeserializeUnknownEnumValue()
        {
            var enumValue = "newValue";
            var id = "id";

            var stringToDeserialize = string.Format(
                "{{\"enumType\":\"{0}\",\"id\":\"{1}\"}}",
                enumValue,
                id);

            var instance = this.serializer.DeserializeObject<DerivedTypeClass>(stringToDeserialize);

            Assert.NotNull(instance);
            Assert.Equal(id, instance.Id);
            Assert.Null(instance.EnumType);
            Assert.NotNull(instance.AdditionalData);
            Assert.Equal(enumValue, instance.AdditionalData["enumType"].ToString());
        }

        [Fact]
        public void DerivedTypeWithoutDefaultConstructor()
        {
            var stringToDeserialize = "{\"jsonKey\":\"jsonValue\"}";
            ServiceException exception = Assert.Throws<ServiceException>(() => this.serializer.DeserializeObject<NoDefaultConstructor>(stringToDeserialize));

            Assert.True(exception.IsMatch(ErrorConstants.Codes.GeneralException));
            Assert.Equal(
                string.Format(
                    ErrorConstants.Messages.UnableToCreateInstanceOfTypeFormatString,
                    typeof(NoDefaultConstructor).AssemblyQualifiedName),
                exception.Error.Message);
        }

        [Fact]
        public void DeserializeDateEnumerableValue()
        {
            var now = DateTimeOffset.UtcNow;
            var tomorrow = now.AddDays(1);

            var stringToDeserialize = string.Format("{{\"dateCollection\":[\"{0}\",\"{1}\"]}}", now.ToString("yyyy-MM-dd"), tomorrow.ToString("yyyy-MM-dd"));

            var deserializedObject = this.serializer.DeserializeObject<DateTestClass>(stringToDeserialize);

            Assert.Equal(2, deserializedObject.DateCollection.Count());
            Assert.True(deserializedObject.DateCollection.Any(
                 date =>
                    date.Year == now.Year &&
                    date.Month == now.Month &&
                    date.Day == now.Day),
                "Now date not found.");

            Assert.Contains(deserializedObject.DateCollection, date =>
                    date.Year == tomorrow.Year &&
                    date.Month == tomorrow.Month &&
                    date.Day == tomorrow.Day);
        }

        [Fact]
        public void DeserializeMemorableDatesValue()
        {
            var now = DateTimeOffset.UtcNow;
            var tomorrow = now.AddDays(1);
            var recurrence = new DateTestClass
            {
                DateCollection = new List<Date> { new Date(now.Year, now.Month, now.Day), new Date(tomorrow.Year, tomorrow.Month, tomorrow.Day) },
            };

            var derivedTypeInstance = new DerivedTypeClass
            {
                Id = "Id",
                MemorableDates = new DateTestClass[] {recurrence, recurrence},
                Name = "Awesome Test"
            };

            string json = this.serializer.SerializeObject(derivedTypeInstance);

            var deserializedInstance = this.serializer.DeserializeObject<DerivedTypeClass>(json);

            Assert.Equal(derivedTypeInstance.Name ,deserializedInstance.Name);
            Assert.Equal(derivedTypeInstance.Id ,deserializedInstance.Id);
            Assert.Equal(derivedTypeInstance.MemorableDates.Count() ,deserializedInstance.MemorableDates.Count());
        }

        [Fact]
        public void DeserializeDateValue()
        {
            var now = DateTimeOffset.UtcNow;

            var stringToDeserialize = string.Format("{{\"nullableDate\":\"{0}\"}}", now.ToString("yyyy-MM-dd"));

            var dateClass = this.serializer.DeserializeObject<DateTestClass>(stringToDeserialize);

            Assert.Equal(now.Year, dateClass.NullableDate.Year);
            Assert.Equal(now.Month, dateClass.NullableDate.Month);
            Assert.Equal(now.Day, dateClass.NullableDate.Day);
        }

        [Fact]
        public void DeserializeInterface()
        {
            var collectionPage = new CollectionPageInstance
            {
                new DerivedTypeClass { Id = "id" },
                new DerivedTypeClass { Id = "id1" },
                new DerivedTypeClass { Id = "id2" },
                new DerivedTypeClass { Id = "id3" },

            };

            var serializedString = this.serializer.SerializeObject(collectionPage);

            var deserializedPage = this.serializer.DeserializeObject<ICollectionPageInstance>(serializedString);
            Assert.IsType<CollectionPageInstance>(deserializedPage);
            Assert.Equal(4, deserializedPage.Count);
            Assert.Equal("id", deserializedPage[0].Id);
            Assert.Equal("id1", deserializedPage[1].Id);
            Assert.Equal("id2", deserializedPage[2].Id);
            Assert.Equal("id3", deserializedPage[3].Id);
        }
        
        [Fact]
        public void NewAbstractDerivedClassInstance()
        {
            var entityId = "entityId";
            var additionalKey = "key";
            var additionalValue = "value";

            var stringToDeserialize = string.Format(
                "{{\"id\":\"{0}\", \"@odata.type\":\"newtype\", \"{1}\":\"{2}\"}}",
                entityId,
                additionalKey,
                additionalValue);

            var instance = this.serializer.DeserializeObject<AbstractEntityType>(stringToDeserialize);

            Assert.NotNull(instance);
            Assert.Equal(entityId, instance.Id);
            Assert.NotNull(instance.AdditionalData);
            Assert.Equal(additionalValue, instance.AdditionalData[additionalKey].ToString());
        }

        [Fact]
        public void SerializeAndDeserializeKnownEnumValue()
        {
            var instance = new DerivedTypeClass
            {
                Id = "id",
                EnumType = EnumType.Value,
            };

            var expectedSerializedStream = string.Format(
                "{{\"enumType\":\"{0}\",\"id\":\"{1}\"}}",
                "value",
                instance.Id);

            var serializedValue = this.serializer.SerializeObject(instance);

            Assert.Equal(expectedSerializedStream, serializedValue);

            var newInstance = this.serializer.DeserializeObject<DerivedTypeClass>(serializedValue);

            Assert.NotNull(newInstance);
            Assert.Equal(instance.Id, instance.Id);
            Assert.Equal(EnumType.Value, instance.EnumType);
            Assert.Null(instance.AdditionalData);
        }

        [Fact]
        public void SerializeDateEnumerableValue()
        {
            var now = DateTimeOffset.UtcNow;
            var tomorrow = now.AddDays(1);

            var expectedSerializedString = string.Format("{{\"nullableDate\":null,\"dateCollection\":[\"{0}\",\"{1}\"]}}", now.ToString("yyyy-MM-dd"), tomorrow.ToString("yyyy-MM-dd"));

            var recurrence = new DateTestClass
            {
                DateCollection = new List<Date> { new Date(now.Year, now.Month, now.Day), new Date(tomorrow.Year, tomorrow.Month, tomorrow.Day) },
            };

            var serializedString = this.serializer.SerializeObject(recurrence);

            Assert.Equal(expectedSerializedString, serializedString);
        }

        [Fact]
        public void SerializeDateNullValue()
        {
            var now = DateTimeOffset.UtcNow;

            var expectedSerializedString = "{\"nullableDate\":null}";

            var recurrence = new DateTestClass();

            var serializedString = this.serializer.SerializeObject(recurrence);

            Assert.Equal(expectedSerializedString, serializedString);
        }

        [Fact]
        public void SerializeDateValue()
        {
            var now = DateTimeOffset.UtcNow;

            var expectedSerializedString = string.Format("{{\"nullableDate\":\"{0}\"}}", now.ToString("yyyy-MM-dd"));

            var date = new DateTestClass
            {
                NullableDate = new Date(now.Year, now.Month, now.Day),
            };

            var serializedString = this.serializer.SerializeObject(date);

            Assert.Equal(expectedSerializedString, serializedString);
        }

        [Fact]
        public void VerifyTypeMappingCache()
        {
            // Clear the cache so it won't have mappings from previous tests.
            DerivedTypeConverter<object>.TypeMappingCache.Clear();

            var id = "id";
            var derivedTypeClassTypeString = "microsoft.graph.dotnetCore.core.test.testModels.derivedTypeClass";
            var dateTestClassTypeString = "microsoft.graph.dotnetCore.core.test.testModels.dateTestClass";

            var deserializeExistingTypeString = string.Format(
                "{{\"id\":\"{0}\", \"@odata.type\":\"#{1}\"}}",
                id,
                derivedTypeClassTypeString);

            var derivedType = this.serializer.DeserializeObject<AbstractEntityType>(deserializeExistingTypeString) as DerivedTypeClass;
            var derivedType2 = this.serializer.DeserializeObject<DerivedTypeClass>(deserializeExistingTypeString) as DerivedTypeClass;

            var deserializeUnknownTypeString = string.Format(
                "{{\"id\":\"{0}\", \"@odata.type\":\"#unknown\"}}",
                id);

            var upcastType = this.serializer.DeserializeObject<DerivedTypeClass>(deserializeUnknownTypeString) as DerivedTypeClass;

            var dateTestTypeString = string.Format(
                "{{\"@odata.type\":\"#{1}\"}}",
                id,
                dateTestClassTypeString);

            var dateTestType = this.serializer.DeserializeObject<DateTestClass>(dateTestTypeString) as DateTestClass;

            Assert.NotNull(derivedType);
            Assert.NotNull(derivedType2);
            Assert.NotNull(upcastType);
            Assert.NotNull(dateTestType);

            Assert.Equal(2, DerivedTypeConverter<DerivedTypeClass>.TypeMappingCache.Count);

            Assert.Equal(
                typeof(DerivedTypeClass),
                DerivedTypeConverter<DerivedTypeClass>.TypeMappingCache[derivedTypeClassTypeString]);

            Assert.Equal(
                typeof(DerivedTypeClass),
                DerivedTypeConverter<DerivedTypeClass>.TypeMappingCache["unknown"]);

            Assert.Equal(
                typeof(DateTestClass),
                DerivedTypeConverter<DateTestClass>.TypeMappingCache[dateTestClassTypeString]);
        }
    }
}
