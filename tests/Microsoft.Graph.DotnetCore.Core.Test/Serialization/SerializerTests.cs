// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Serialization
{
    using Microsoft.Graph.Core.Models;
    using Microsoft.Graph.DotnetCore.Core.Test.TestModels;
    using Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
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
        public void DeserializeDerivedTypeFromAbstractParent()
        {
            var id = "id";
            var name = "name";

            var stringToDeserialize = string.Format(
                "{{\"id\":\"{0}\", \"@odata.type\":\"#microsoft.graph.dotnetCore.core.test.testModels.derivedTypeClass\", \"name\":\"{1}\"}}",
                id,
                name);

            //The type information from "@odata.type" should lead to correctly deserializing to the derived type
            var derivedType = this.serializer.DeserializeObject<AbstractEntityType>(stringToDeserialize) as DerivedTypeClass;

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
        public void DerivedTypeConverterFollowsNamingProperty()
        {
            var id = "id";
            var givenName = "name";
            var link = "localhost.com"; // this property name does not match the object name

            var stringToDeserialize = string.Format(
                "{{\"id\":\"{0}\", \"givenName\":\"{1}\", \"link\":\"{2}\"}}",
                id,
                givenName,
                link);

            var instance = this.serializer.DeserializeObject<DerivedTypeClass>(stringToDeserialize);

            Assert.NotNull(instance);
            Assert.Equal(id, instance.Id);
            Assert.Equal(link, instance.WebUrl);
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
        public void DeserializeEmptyStringOrStream()
        {
            var stringToDeserialize = string.Empty;

            // Asset empty stream deserializes to null
            using (var serializedStream = new MemoryStream(Encoding.UTF8.GetBytes(stringToDeserialize)))
            {
                var instance = this.serializer.DeserializeObject<DerivedTypeClass>(serializedStream);
                Assert.Null(instance);
            }

            // Asset empty string deserializes to null
            var stringInstance = this.serializer.DeserializeObject<DerivedTypeClass>(stringToDeserialize);
            Assert.Null(stringInstance);
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
        public void SerializeEnumValueWithFlags()
        {
            EnumTypeWithFlags enumValueWithFlags = EnumTypeWithFlags.FirstValue | EnumTypeWithFlags.SecondValue;

            var expectedSerializedValue = "\"firstValue, secondValue\""; // All values should be camelCased

            var serializedValue = this.serializer.SerializeObject(enumValueWithFlags);

            Assert.Equal(expectedSerializedValue, serializedValue);
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
        public void DerivedTypeConverterIgnoresPropertyWithJsonIgnore()
        {
            var now = DateTimeOffset.UtcNow;

            var expectedSerializedString = string.Format("{{\"nullableDate\":\"{0}\"}}", now.ToString("yyyy-MM-dd"));

            var date = new DateTestClass
            {
                NullableDate = new Date(now.Year, now.Month, now.Day),
                IgnoredNumber = 230 // we shouldn't see this value
            };

            var serializedString = this.serializer.SerializeObject(date);

            Assert.Equal(expectedSerializedString, serializedString);
            Assert.DoesNotContain("230", serializedString);
        }

        [Fact]
        public void SerializeObjectWithAdditionalDataWithDerivedTypeConverter()
        {
            // This example class uses the derived type converter
            // Arrange
            TestItemBody testItemBody = new TestItemBody
            {
                Content = "Example Content",
                ContentType = TestBodyType.Text,
                AdditionalData = new Dictionary<string, object>
                {
                    { "length" , "100" }
                }
            };
            var expectedSerializedString = "{" +
                                               "\"contentType\":\"text\"," +
                                               "\"content\":\"Example Content\"," +
                                               "\"length\":\"100\"," + // should be at the same level as other properties
                                               "\"@odata.type\":\"microsoft.graph.itemBody\"" +
                                           "}";

            // Act
            var serializedString = this.serializer.SerializeObject(testItemBody);

            //Assert
            Assert.Equal(expectedSerializedString, serializedString);
        }

        [Fact]
        public void SerializeObjectWithEmptyAdditionalDataWithDerivedTypeConverter()
        {
            // This example class uses the derived type converter with an empty/unset AdditionalData
            // Arrange
            TestItemBody testItemBody = new TestItemBody
            {
                Content = "Example Content",
                ContentType = TestBodyType.Text
            };
            var expectedSerializedString = "{" +
                                           "\"contentType\":\"text\"," +
                                           "\"content\":\"Example Content\"," +
                                           "\"@odata.type\":\"microsoft.graph.itemBody\"" +
                                           "}";

            // Act
            var serializedString = this.serializer.SerializeObject(testItemBody);

            //Assert
            Assert.Equal(expectedSerializedString, serializedString);
        }

        [Fact]
        public void SerializeObjectWithAdditionalDataWithoutDerivedTypeConverter()
        {
            // This example class does NOT use the derived type converter to act as a control
            // Arrange
            TestEmailAddress testEmailAddress = new TestEmailAddress
            {
                Name = "Peter Pan",
                Address = "peterpan@neverland.com",
                AdditionalData = new Dictionary<string, object>
                {
                    { "alias" , "peterpan" }
                }
            };
            var expectedSerializedString = "{" +
                                               "\"name\":\"Peter Pan\"," +
                                               "\"address\":\"peterpan@neverland.com\"," +
                                               "\"@odata.type\":\"microsoft.graph.emailAddress\"," +
                                               "\"alias\":\"peterpan\"" + // should be at the same level as other properties
                                           "}";

            // Act
            var serializedString = this.serializer.SerializeObject(testEmailAddress);

            // Assert
            Assert.Equal(expectedSerializedString, serializedString);
        }

        [Theory]
        [InlineData("2016-11-20T18:23:45.9356913+00:00", "\"2016-11-20T18:23:45.9356913+00:00\"")]
        [InlineData("1992-10-26T08:30:15.1456919+07:00", "\"1992-10-26T08:30:15.1456919+07:00\"")]// make sure different offset is okay as well
        public void SerializeDateTimeOffsetValue(string dateTimeOffsetString, string expectedJsonValue)
        {
            // Arrange
            var dateTimeOffset = DateTimeOffset.Parse(dateTimeOffsetString);
            // Act
            var serializedString = this.serializer.SerializeObject(dateTimeOffset);

            // Assert
            // Expect the string to be ISO 8601-1:2019 format
            Assert.Equal(expectedJsonValue, serializedString);
        }

        [Fact]
        public void SerializeUploadSessionValues()
        {
            // Arrange
            var uploadSession = new UploadSession()
            {
                ExpirationDateTime = DateTimeOffset.Parse("2016-11-20T18:23:45.9356913+00:00"),
                UploadUrl = "http://localhost",
                NextExpectedRanges = new List<string> { "0 - 1000" }
            };
            var expectedString = @"{""expirationDateTime"":""2016-11-20T18:23:45.9356913+00:00"",""nextExpectedRanges"":[""0 - 1000""],""uploadUrl"":""http://localhost""}";
            // Act
            var serializedString = this.serializer.SerializeObject(uploadSession);
            // Assert
            // Expect the string to be ISO 8601-1:2019 format
            Assert.Equal(expectedString, serializedString);
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
        
        [Theory]
        [InlineData("string", "{\"@odata.context\": \"https://graph.microsoft.com/beta/$metadata#String\", \"value\": \"expectedvalue\"}")]
        [InlineData("bool", "{\"@odata.context\": \"https://graph.microsoft.com/beta/$metadata#String\", \"value\": true}")]
        [InlineData("int32", "{\"@odata.context\":\"https://graph.microsoft.com/v1.0/$metadata#Edm.Boolean\",\"value\":1}")]
        public void MethodResponseSimpleReturnTypeDeserialization(string @case, string payload)
        {
            switch (@case)
            {
                case "string": 
                    var actualStringValue = this.serializer.DeserializeObject<ODataMethodStringResponse>(payload).Value;
                    Assert.Equal("expectedvalue", actualStringValue);
                    break;
                case "bool":
                    var actualBoolValue = this.serializer.DeserializeObject<ODataMethodBooleanResponse>(payload).Value;
                    Assert.True(actualBoolValue);
                    break;
                case "int32":
                    var actualInt32Value = this.serializer.DeserializeObject<ODataMethodIntResponse>(payload).Value;
                    Assert.Equal(1, actualInt32Value);
                    break;
                default:
                    Assert.True(false);
                    break;
            }
        }

        [Theory]
        [InlineData("string")]
        [InlineData("bool")]
        [InlineData("int32")]
        public void MethodResponseUnexpectedReturnObject(string @case)
        {
            var payload = "{\"@odata.context\": \"https://graph.microsoft.com/beta/$metadata#String\", \"value\": { \"objProp\": \"objPropValue\" }}";

            switch (@case)
            {
                case "string":
                    JsonException exceptionString = Assert.Throws<JsonException>(() => this.serializer
                                                                                                       .DeserializeObject<ODataMethodStringResponse>(payload));
                    Assert.Equal("$.value", exceptionString.Path); // the value property doesn't exist as a string
                    break;
                case "bool":
                    JsonException exceptionBool = Assert.Throws<JsonException>(() => this.serializer
                                                                                                     .DeserializeObject<ODataMethodBooleanResponse>(payload));
                    Assert.Equal("$.value", exceptionBool.Path); // the value property doesn't exist as a bool
                    break;
                case "int32":
                    JsonException exceptionInt = Assert.Throws<JsonException>(() => this.serializer
                                                                                                    .DeserializeObject<ODataMethodIntResponse>(payload));
                    Assert.Equal("$.value", exceptionInt.Path); // the value property doesn't exist as a int
                    break;
                default:
                    Assert.True(false);
                    break;
            }
        }

        /// <summary>
        /// Test what happens when the service API returns an unexpected response body.
        /// https://github.com/microsoftgraph/msgraph-sdk-serviceissues/issues/9
        /// </summary>
        [Theory]
        [InlineData("string")]
        [InlineData("bool")]
        [InlineData("int32")]
        public void MethodResponseMissingValueDeserialization(string @case)
        {
            var badPayload = "{\"@odata.context\": \"https://graph.microsoft.com/v1.0/$metadata#Edm.Null\", \"@odata.null\": true}";

            switch (@case)
            {
                case "string":
                    var stringResult = this.serializer.DeserializeObject<ODataMethodStringResponse>(badPayload).Value;
                    Assert.Null(stringResult);
                    break;
                case "bool":
                    var boolResult = this.serializer.DeserializeObject<ODataMethodBooleanResponse>(badPayload).Value;
                    Assert.Null(boolResult);
                    break;
                case "int32":
                    var intResult = this.serializer.DeserializeObject<ODataMethodIntResponse>(badPayload).Value;
                    Assert.Null(intResult);
                    break;
                default:
                    Assert.True(false);
                    break;
            }
        }
    }
}
