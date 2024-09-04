// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------



namespace Microsoft.Graph.DotnetCore.Core.Test.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Graph.Core.Models;
    using Microsoft.Graph.DotnetCore.Core.Test.TestModels;
    using Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels;
    using Microsoft.Kiota.Abstractions;
    using Microsoft.Kiota.Abstractions.Serialization;
    using Microsoft.Kiota.Serialization.Json;
    using Xunit;
    public class SerializerTests
    {
        public SerializerTests()
        {
            ApiClientBuilder.RegisterDefaultSerializer<JsonSerializationWriterFactory>();
            ApiClientBuilder.RegisterDefaultDeserializer<JsonParseNodeFactory>();
        }

        [Fact]
        public async Task DeserializeDerivedTypeAsync()
        {
            var id = "id";
            var name = "name";

            var stringToDeserialize = string.Format(
                "{{\"id\":\"{0}\", \"@odata.type\":\"#microsoft.graph.dotnetCore.core.test.testModels.derivedTypeClass\", \"name\":\"{1}\"}}",
                id,
                name);

            var derivedType = await KiotaJsonSerializer.DeserializeAsync<DerivedTypeClass>(stringToDeserialize);

            Assert.NotNull(derivedType);
            Assert.Equal(id, derivedType.Id);
            Assert.Equal(name, derivedType.Name);
        }

        [Fact]
        public async Task DeserializeDerivedTypeFromAbstractParentAsync()
        {
            var id = "id";
            var name = "name";

            var stringToDeserialize = string.Format(
                "{{\"id\":\"{0}\", \"@odata.type\":\"#microsoft.graph.dotnetCore.core.test.testModels.derivedTypeClass\", \"name\":\"{1}\"}}",
                id,
                name);

            //The type information from "@odata.type" should lead to correctly deserializing to the derived type
            var derivedType = await KiotaJsonSerializer.DeserializeAsync<AbstractEntityType>(stringToDeserialize) as DerivedTypeClass;

            Assert.NotNull(derivedType);
            Assert.Equal(id, derivedType.Id);
            Assert.Equal(name, derivedType.Name);
        }


        [Fact]
        public async Task DeserializeInvalidODataTypeAsync()
        {
            var id = "id";
            var givenName = "name";

            var stringToDeserialize = string.Format(
                "{{\"id\":\"{0}\", \"@odata.type\":\"invalid\", \"givenName\":\"{1}\"}}",
                id,
                givenName);

            var instance = await KiotaJsonSerializer.DeserializeAsync<DerivedTypeClass>(stringToDeserialize);

            Assert.NotNull(instance);
            Assert.Equal(id, instance.Id);
            Assert.NotNull(instance.AdditionalData);
            Assert.Equal(givenName, instance.AdditionalData["givenName"].ToString());
        }

        [Fact]
        public async Task DeserializerFollowsNamingPropertyAsync()
        {
            var id = "id";
            var givenName = "name";
            var link = "localhost.com"; // this property name does not match the object name

            var stringToDeserialize = string.Format(
                "{{\"id\":\"{0}\", \"givenName\":\"{1}\", \"link\":\"{2}\"}}",
                id,
                givenName,
                link);

            var instance = await KiotaJsonSerializer.DeserializeAsync<DerivedTypeClass>(stringToDeserialize);

            Assert.NotNull(instance);
            Assert.Equal(id, instance.Id);
            Assert.Equal(link, instance.WebUrl);
            Assert.NotNull(instance.AdditionalData);
            Assert.Equal(givenName, instance.AdditionalData["givenName"].ToString());
        }

        [Fact]
        public async Task DeserializeUnknownEnumValueAsync()
        {
            var enumValue = "newValue";
            var id = "id";

            var stringToDeserialize = string.Format(
                "{{\"enumType\":\"{0}\",\"id\":\"{1}\"}}",
                enumValue,
                id);

            var instance = await KiotaJsonSerializer.DeserializeAsync<DerivedTypeClass>(stringToDeserialize);

            Assert.NotNull(instance);
            Assert.Equal(id, instance.Id);
            Assert.Null(instance.EnumType);
            Assert.NotNull(instance.AdditionalData);
        }

        [Fact]
        public async Task DeserializeDateEnumerableValueAsync()
        {
            var now = DateTimeOffset.UtcNow;
            var tomorrow = now.AddDays(1);

            var stringToDeserialize = string.Format("{{\"dateCollection\":[\"{0}\",\"{1}\"]}}", now.ToString("yyyy-MM-dd"), tomorrow.ToString("yyyy-MM-dd"));

            var deserializedObject = await KiotaJsonSerializer.DeserializeAsync<DateTestClass>(stringToDeserialize);

            Assert.Equal(2, deserializedObject.DateCollection.Count());
            Assert.True(deserializedObject.DateCollection.Any(
                 date =>
                    date.Value.Year == now.Year &&
                    date.Value.Month == now.Month &&
                    date.Value.Day == now.Day),
                "Now date not found.");

            Assert.Contains(deserializedObject.DateCollection, date =>
                    date.Value.Year == tomorrow.Year &&
                    date.Value.Month == tomorrow.Month &&
                    date.Value.Day == tomorrow.Day);
        }


        [Fact]
        public async Task DeserializeMemorableDatesValueAsync()
        {
            var now = DateTimeOffset.UtcNow;
            var tomorrow = now.AddDays(1);
            var recurrence = new DateTestClass
            {
                DateCollection = new List<Date?> { new Date(now.Year, now.Month, now.Day), new Date(tomorrow.Year, tomorrow.Month, tomorrow.Day) },
            };

            var derivedTypeInstance = new DerivedTypeClass
            {
                Id = "Id",
                MemorableDates = new DateTestClass[] { recurrence, recurrence },
                Name = "Awesome Test"
            };

            // Serialize
            var serializedStream = KiotaJsonSerializer.SerializeAsStream(derivedTypeInstance);
            // De serialize
            var deserializedInstance = await KiotaJsonSerializer.DeserializeAsync<DerivedTypeClass>(serializedStream);

            Assert.Equal(derivedTypeInstance.Name, deserializedInstance.Name);
            Assert.Equal(derivedTypeInstance.Id, deserializedInstance.Id);
            Assert.Equal(derivedTypeInstance.MemorableDates.Count(), deserializedInstance.MemorableDates.Count());
        }

        [Fact]
        public async Task DeserializeDateValueAsync()
        {
            var now = DateTimeOffset.UtcNow;

            var stringToDeserialize = string.Format("{{\"nullableDate\":\"{0}\"}}", now.ToString("yyyy-MM-dd"));

            var dateClass = await KiotaJsonSerializer.DeserializeAsync<DateTestClass>(stringToDeserialize);

            Assert.Equal(now.Year, dateClass.NullableDate.Value.Year);
            Assert.Equal(now.Month, dateClass.NullableDate.Value.Month);
            Assert.Equal(now.Day, dateClass.NullableDate.Value.Day);
        }

        [Fact]
        // This test validates we do not experience an InvalidCastException in scenarios where the api could return a type in the odata.type string the is not assignable to the type defined in the metadata.
        // A good example is the ResourceData type which can have the odata.type specified as ChatMessage(which derives from entity) and can't be assigned to it. Extra properties will therefore be found in AdditionalData.
        // https://docs.microsoft.com/en-us/graph/api/resources/resourcedata?view=graph-rest-1.0#json-representation
        public async Task DeserializeUnrelatedTypesInOdataTypeAsync()
        {
            // Arrange
            var resourceDataString = "{\r\n" +
                                        "\"@odata.type\": \"#Microsoft.Graph.Message\",\r\n" + // this type can't be assigned/cast to TestResourceData
                                        "\"@odata.id\": \"Users/{user-id}/Messages/{message-id}\",\r\n" +
                                        "\"@odata.etag\": \"{etag}\",\r\n" +
                                        "\"id\": \"{id}\"\r\n" +
                                     "}";

            // Act
            var resourceData = await KiotaJsonSerializer.DeserializeAsync<TestResourceData>(resourceDataString);

            // Assert
            Assert.IsType<TestResourceData>(resourceData);
            Assert.NotNull(resourceData);
            Assert.Equal("#Microsoft.Graph.Message", resourceData.ODataType);
            Assert.Equal("{id}", resourceData.AdditionalData["id"].ToString());
            Assert.Equal("{etag}", resourceData.AdditionalData["@odata.etag"].ToString());
            Assert.Equal("Users/{user-id}/Messages/{message-id}", resourceData.AdditionalData["@odata.id"].ToString());
        }

        [Fact]
        public async Task NewAbstractDerivedClassInstanceAsync()
        {
            var entityId = "entityId";
            var additionalKey = "key";
            var additionalValue = "value";

            var stringToDeserialize = string.Format(
                "{{\"id\":\"{0}\", \"@odata.type\":\"newtype\", \"{1}\":\"{2}\"}}",
                entityId,
                additionalKey,
                additionalValue);

            var instance = await KiotaJsonSerializer.DeserializeAsync<AbstractEntityType>(stringToDeserialize);

            Assert.NotNull(instance);
            Assert.Equal(entityId, instance.Id);
            Assert.NotNull(instance.AdditionalData);
            Assert.Equal(additionalValue, instance.AdditionalData[additionalKey].ToString());
        }

        [Fact]
        public async Task SerializeAndDeserializeKnownEnumValueAsync()
        {
            var instance = new DerivedTypeClass
            {
                Id = "id",
                EnumType = EnumType.Value,
            };

            var expectedSerializedStream = string.Format(
                "{{\"id\":\"{1}\",\"enumType\":\"{0}\"}}",
                "value",
                instance.Id);


            // Serialize
            var serializeAsString = await KiotaJsonSerializer.SerializeAsStringAsync(instance);

            //Assert
            Assert.Equal(expectedSerializedStream, serializeAsString);

            // De serialize
            var newInstance = await KiotaJsonSerializer.DeserializeAsync<DerivedTypeClass>(serializeAsString);

            Assert.NotNull(newInstance);
            Assert.Equal(instance.Id, instance.Id);
            Assert.Equal(EnumType.Value, instance.EnumType);
            Assert.Null(instance.AdditionalData);
        }

        [Fact]
        public void SerializeEnumValueWithFlags()
        {
            EnumTypeWithFlags enumValueWithFlags = EnumTypeWithFlags.FirstValue | EnumTypeWithFlags.SecondValue;

            var expectedSerializedValue = "\"firstValue,secondValue\""; // All values should be camelCased

            using var jsonSerializerWriter = new JsonSerializationWriter();
            jsonSerializerWriter.WriteEnumValue<EnumTypeWithFlags>(string.Empty, enumValueWithFlags);
            var serializedStream = jsonSerializerWriter.GetSerializedContent();

            // Assert
            var streamReader = new StreamReader(serializedStream);
            Assert.Equal(expectedSerializedValue, streamReader.ReadToEnd());
        }

        [Fact]
        public void SerializeDateEnumerableValue()
        {
            var now = DateTimeOffset.UtcNow;
            var tomorrow = now.AddDays(1);

            var expectedSerializedString = string.Format("{{\"nullableDate\":null,\"dateCollection\":[\"{0}\",\"{1}\"]}}", now.ToString("yyyy-MM-dd"), tomorrow.ToString("yyyy-MM-dd"));

            var recurrence = new DateTestClass
            {
                DateCollection = new List<Date?> { new Date(now.Year, now.Month, now.Day), new Date(tomorrow.Year, tomorrow.Month, tomorrow.Day) },
            };

            using var jsonSerializerWriter = new JsonSerializationWriter();
            jsonSerializerWriter.OnStartObjectSerialization = (pasable, serialiationWriter) => { if ((pasable as DateTestClass).NullableDate == null) serialiationWriter.WriteNullValue("nullableDate"); };
            jsonSerializerWriter.WriteObjectValue(string.Empty, recurrence);
            var serializedStream = jsonSerializerWriter.GetSerializedContent();

            // Assert
            var streamReader = new StreamReader(serializedStream);
            Assert.Equal(expectedSerializedString, streamReader.ReadToEnd());
        }

        [Fact]
        public void SerializeDateNullValue()
        {
            var expectedSerializedString = "{\"nullableDate\":null}";

            var recurrence = new DateTestClass();

            using var jsonSerializerWriter = new JsonSerializationWriter();
            jsonSerializerWriter.OnStartObjectSerialization = (pasable, serialiationWriter) => { if ((pasable as DateTestClass).NullableDate == null) serialiationWriter.WriteNullValue("nullableDate"); };
            jsonSerializerWriter.WriteObjectValue(string.Empty, recurrence);
            var serializedStream = jsonSerializerWriter.GetSerializedContent();

            // Assert
            var streamReader = new StreamReader(serializedStream);
            Assert.Equal(expectedSerializedString, streamReader.ReadToEnd());
        }

        [Fact]
        public async Task SerializeDateValueAsync()
        {
            var now = DateTimeOffset.UtcNow;

            var expectedSerializedString = string.Format("{{\"nullableDate\":\"{0}\"}}", now.ToString("yyyy-MM-dd"));

            var date = new DateTestClass
            {
                NullableDate = new Date(now.Year, now.Month, now.Day),
            };

            var serializedString = await KiotaJsonSerializer.SerializeAsStringAsync(date);

            // Assert
            Assert.Equal(expectedSerializedString, serializedString);
        }

        [Fact]
        public async Task DerivedTypeConverterIgnoresPropertyWithJsonIgnoreAsync()
        {
            var now = DateTimeOffset.UtcNow;

            var expectedSerializedString = string.Format("{{\"nullableDate\":\"{0}\"}}", now.ToString("yyyy-MM-dd"));

            var date = new DateTestClass
            {
                NullableDate = new Date(now.Year, now.Month, now.Day),
                IgnoredNumber = 230 // we shouldn't see this value
            };

            // Assert
            var serializedString = await KiotaJsonSerializer.SerializeAsStringAsync(date);

            Assert.Equal(expectedSerializedString, serializedString);
            Assert.DoesNotContain("230", serializedString);
        }

        [Fact]
        public async Task SerializeObjectWithAdditionalDataWithDerivedTypeConverterAsync()
        {
            // This example class uses the derived type converter
            // Arrange
            TestItemBody testItemBody = new TestItemBody
            {
                Content = "Example Content",
                ContentType = TestBodyType.Text,
                AdditionalData = new Dictionary<string, object>
                {
                    { "length" , "100" },
                    { "extraProperty", null }
                }
            };
            var expectedSerializedString = "{" +
                                               "\"@odata.type\":\"microsoft.graph.itemBody\"," +
                                               "\"contentType\":\"text\"," +
                                               "\"content\":\"Example Content\"," +
                                               "\"length\":\"100\"," + // should be at the same level as other properties
                                               "\"extraProperty\":null" +
                                           "}";

            // Act
            var serializedJsonString = await KiotaJsonSerializer.SerializeAsStringAsync(testItemBody);
            // Assert
            Assert.Equal(expectedSerializedString, serializedJsonString);
        }

        [Fact]
        public async Task SerializeObjectWithEmptyAdditionalDataAsync()
        {
            // This example class uses the derived type converter with an empty/unset AdditionalData
            // Arrange
            TestItemBody testItemBody = new TestItemBody
            {
                Content = "Example Content",
                ContentType = TestBodyType.Text
            };
            var expectedSerializedString = "{" +
                                                "\"@odata.type\":\"microsoft.graph.itemBody\"," +
                                                "\"contentType\":\"text\"," +
                                                "\"content\":\"Example Content\"" +
                                           "}";

            // Act
            var serializedString = await KiotaJsonSerializer.SerializeAsStringAsync(testItemBody);

            //Assert
            Assert.Equal(expectedSerializedString, serializedString);
            Assert.DoesNotContain("@odata.nextLink", serializedString);
        }

        [Fact]
        public async Task SerializeObjectWithAdditionalDataWithoutDerivedTypeConverterAsync()
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
            // Assert
            var serializedJsonString = await KiotaJsonSerializer.SerializeAsStringAsync(testEmailAddress);
            Assert.Equal(expectedSerializedString, serializedJsonString);
        }

        [Theory]
        [InlineData("2016-11-20T18:23:45.9356913+00:00", "\"2016-11-20T18:23:45.9356913+00:00\"")]
        [InlineData("1992-10-26T08:30:15.1456919+07:00", "\"1992-10-26T08:30:15.1456919+07:00\"")]// make sure different offset is okay as well
        public void SerializeDateTimeOffsetValue(string dateTimeOffsetString, string expectedJsonValue)
        {
            // Arrange
            var dateTimeOffset = DateTimeOffset.Parse(dateTimeOffsetString);
            // Act
            using var jsonSerializerWriter = new JsonSerializationWriter();
            jsonSerializerWriter.WriteDateTimeOffsetValue(string.Empty, dateTimeOffset);

            // Assert
            // Expect the string to be ISO 8601-1:2019 format
            var serializedStream = jsonSerializerWriter.GetSerializedContent();
            using var reader = new StreamReader(serializedStream, Encoding.UTF8);
            var serializedJsonString = reader.ReadToEnd();
            Assert.Equal(expectedJsonValue, serializedJsonString);
        }

        [Fact]
        public async Task SerializeUploadSessionValuesAsync()
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
            // Assert
            var serializedJsonString = await KiotaJsonSerializer.SerializeAsStringAsync(uploadSession);
            // Expect the string to be ISO 8601-1:2019 format
            Assert.Equal(expectedString, serializedJsonString);
        }

        [Fact]
        public async Task DeserializeUploadSessionValuesAsync()
        {
            // Act 1
            const string camelCasedPayload = @"{""expirationDateTime"":""2016-11-20T18:23:45.9356913+00:00"",""nextExpectedRanges"":[""0 - 1000""],""uploadUrl"":""http://localhost""}";
            var uploadSession = await KiotaJsonSerializer.DeserializeAsync<UploadSession>(camelCasedPayload);
            Assert.NotNull(uploadSession);
            Assert.NotNull(uploadSession.ExpirationDateTime);
            Assert.NotNull(uploadSession.NextExpectedRanges);
            Assert.Single(uploadSession.NextExpectedRanges);

            // Act 1
            const string pascalCasedPayload = @"{""ExpirationDateTime"":""2016-11-20T18:23:45.9356913+00:00"",""NextExpectedRanges"":[""0 - 1000""],""uploadUrl"":""http://localhost""}";
            var uploadSession2 = await KiotaJsonSerializer.DeserializeAsync<UploadSession>(pascalCasedPayload);
            Assert.NotNull(uploadSession2);
            Assert.NotNull(uploadSession2.ExpirationDateTime);
            Assert.NotNull(uploadSession2.NextExpectedRanges);
            Assert.Single(uploadSession2.NextExpectedRanges);
        }

        [Fact]
        public void SerializeServiceExceptionValues()
        {
            // Arrange
            var serviceException = new ServiceException("Unknown Error", null, (int)System.Net.HttpStatusCode.InternalServerError);
            // Assert
            // Get the json string from the stream.
            var serializedStream = KiotaJsonSerializer.SerializeAsStream(serviceException);
            using var reader = new StreamReader(serializedStream, Encoding.UTF8);
            var serializedJsonString = reader.ReadToEnd();

            var expectedString = @"{""statusCode"":500,""message"":""Unknown Error""}";
            // Expect the string to be ISO 8601-1:2019 format
            Assert.Equal(expectedString, serializedJsonString);
        }
    }
}
