// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Serialization
{
    using Microsoft.Graph.Core.Models;
    using Microsoft.Graph.DotnetCore.Core.Test.TestModels;
    using Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels;
    using Microsoft.Kiota.Abstractions;
    using Microsoft.Kiota.Serialization.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Xunit;
    public class SerializerTests
    {
        private readonly JsonParseNodeFactory parseNodeFactory;

        public SerializerTests()
        {
            this.parseNodeFactory = new JsonParseNodeFactory();
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

            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(stringToDeserialize));
            var parseNode = this.parseNodeFactory.GetRootParseNode(CoreConstants.MimeTypeNames.Application.Json,memoryStream);
            var derivedType = parseNode.GetObjectValue<DerivedTypeClass>(DerivedTypeClass.CreateFromDiscriminatorValue);

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
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(stringToDeserialize));
            var parseNode = this.parseNodeFactory.GetRootParseNode(CoreConstants.MimeTypeNames.Application.Json, memoryStream);
            var derivedType = parseNode.GetObjectValue<AbstractEntityType>(AbstractEntityType.CreateFromDiscriminatorValue) as DerivedTypeClass;

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

            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(stringToDeserialize));
            var parseNode = this.parseNodeFactory.GetRootParseNode(CoreConstants.MimeTypeNames.Application.Json, memoryStream);
            var instance = parseNode.GetObjectValue<DerivedTypeClass>(DerivedTypeClass.CreateFromDiscriminatorValue);

            Assert.NotNull(instance);
            Assert.Equal(id, instance.Id);
            Assert.NotNull(instance.AdditionalData);
            Assert.Equal(givenName, instance.AdditionalData["givenName"].ToString());
        }

        [Fact]
        public void DeserializerFollowsNamingProperty()
        {
            var id = "id";
            var givenName = "name";
            var link = "localhost.com"; // this property name does not match the object name

            var stringToDeserialize = string.Format(
                "{{\"id\":\"{0}\", \"givenName\":\"{1}\", \"link\":\"{2}\"}}",
                id,
                givenName,
                link);

            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(stringToDeserialize));
            var parseNode = this.parseNodeFactory.GetRootParseNode(CoreConstants.MimeTypeNames.Application.Json, memoryStream);
            var instance = parseNode.GetObjectValue<DerivedTypeClass>(DerivedTypeClass.CreateFromDiscriminatorValue);

            Assert.NotNull(instance);
            Assert.Equal(id, instance.Id);
            Assert.Equal(link, instance.WebUrl);
            Assert.NotNull(instance.AdditionalData);
            Assert.Equal(givenName, instance.AdditionalData["givenName"].ToString());
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

            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(stringToDeserialize));
            var parseNode = this.parseNodeFactory.GetRootParseNode(CoreConstants.MimeTypeNames.Application.Json, memoryStream);
            var instance = parseNode.GetObjectValue<DerivedTypeClass>(DerivedTypeClass.CreateFromDiscriminatorValue);

            Assert.NotNull(instance);
            Assert.Equal(id, instance.Id);
            Assert.Null(instance.EnumType);
            Assert.NotNull(instance.AdditionalData);
        }

        [Fact]
        public void DeserializeDateEnumerableValue()
        {
            var now = DateTimeOffset.UtcNow;
            var tomorrow = now.AddDays(1);

            var stringToDeserialize = string.Format("{{\"dateCollection\":[\"{0}\",\"{1}\"]}}", now.ToString("yyyy-MM-dd"), tomorrow.ToString("yyyy-MM-dd"));

            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(stringToDeserialize));
            var parseNode = this.parseNodeFactory.GetRootParseNode(CoreConstants.MimeTypeNames.Application.Json, memoryStream);
            var deserializedObject = parseNode.GetObjectValue<DateTestClass>(DateTestClass.CreateFromDiscriminatorValue);

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
        public void DeserializeMemorableDatesValue()
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
                MemorableDates = new DateTestClass[] {recurrence, recurrence},
                Name = "Awesome Test"
            };

            // Serialize
            using var jsonSerializerWriter = new JsonSerializationWriter();
            jsonSerializerWriter.WriteObjectValue(string.Empty, derivedTypeInstance);
            var serializedStream = jsonSerializerWriter.GetSerializedContent();
            // De serialize
            var parseNode = this.parseNodeFactory.GetRootParseNode(CoreConstants.MimeTypeNames.Application.Json, serializedStream);
            var deserializedInstance = parseNode.GetObjectValue<DerivedTypeClass>(DerivedTypeClass.CreateFromDiscriminatorValue);

            Assert.Equal(derivedTypeInstance.Name ,deserializedInstance.Name);
            Assert.Equal(derivedTypeInstance.Id ,deserializedInstance.Id);
            Assert.Equal(derivedTypeInstance.MemorableDates.Count() ,deserializedInstance.MemorableDates.Count());
        }

        [Fact]
        public void DeserializeDateValue()
        {
            var now = DateTimeOffset.UtcNow;

            var stringToDeserialize = string.Format("{{\"nullableDate\":\"{0}\"}}", now.ToString("yyyy-MM-dd"));

            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(stringToDeserialize));
            var parseNode = this.parseNodeFactory.GetRootParseNode(CoreConstants.MimeTypeNames.Application.Json, memoryStream);
            var dateClass = parseNode.GetObjectValue<DateTestClass>(DateTestClass.CreateFromDiscriminatorValue);

            Assert.Equal(now.Year, dateClass.NullableDate.Value.Year);
            Assert.Equal(now.Month, dateClass.NullableDate.Value.Month);
            Assert.Equal(now.Day, dateClass.NullableDate.Value.Day);
        }

        [Fact]
        // This test validates we do not experience an InvalidCastException in scenarios where the api could return a type in the odata.type string the is not assignable to the type defined in the metadata.
        // A good example is the ResourceData type which can have the odata.type specified as ChatMessage(which derives from entity) and can't be assigned to it. Extra properties will therefore be found in AdditionalData.
        // https://docs.microsoft.com/en-us/graph/api/resources/resourcedata?view=graph-rest-1.0#json-representation
        public void DeserializeUnrelatedTypesInOdataType()
        {
            // Arrange
            var resourceDataString = "{\r\n" +
                                        "\"@odata.type\": \"#Microsoft.Graph.Message\",\r\n" + // this type can't be assigned/cast to TestResourceData
                                        "\"@odata.id\": \"Users/{user-id}/Messages/{message-id}\",\r\n" +
                                        "\"@odata.etag\": \"{etag}\",\r\n" +
                                        "\"id\": \"{id}\"\r\n" +
                                     "}";

            // Act
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(resourceDataString));
            var parseNode = this.parseNodeFactory.GetRootParseNode(CoreConstants.MimeTypeNames.Application.Json, memoryStream);
            var resourceData = parseNode.GetObjectValue<TestResourceData>(TestResourceData.CreateFromDiscriminatorValue);

            // Assert
            Assert.IsType<TestResourceData>(resourceData);
            Assert.NotNull(resourceData);
            Assert.Equal("#Microsoft.Graph.Message", resourceData.ODataType);
            Assert.Equal("{id}", resourceData.AdditionalData["id"].ToString());
            Assert.Equal("{etag}", resourceData.AdditionalData["@odata.etag"].ToString());
            Assert.Equal("Users/{user-id}/Messages/{message-id}", resourceData.AdditionalData["@odata.id"].ToString());
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

            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(stringToDeserialize));
            var parseNode = this.parseNodeFactory.GetRootParseNode(CoreConstants.MimeTypeNames.Application.Json, memoryStream);
            var instance = parseNode.GetObjectValue<AbstractEntityType>(AbstractEntityType.CreateFromDiscriminatorValue);

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
                "{{\"id\":\"{1}\",\"enumType\":\"{0}\"}}",
                "value",
                instance.Id);


            // Serialize
            using var jsonSerializerWriter = new JsonSerializationWriter();
            jsonSerializerWriter.WriteObjectValue(string.Empty, instance);
            var serializedStream = jsonSerializerWriter.GetSerializedContent();

            //Assert
            var streamReader = new StreamReader(serializedStream);
            Assert.Equal(expectedSerializedStream, streamReader.ReadToEnd());

            // De serialize
            serializedStream.Position = 0; //reset the stream to be read again
            var parseNode = this.parseNodeFactory.GetRootParseNode(CoreConstants.MimeTypeNames.Application.Json, serializedStream);
            var newInstance = parseNode.GetObjectValue<DerivedTypeClass>(DerivedTypeClass.CreateFromDiscriminatorValue);

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
        public void SerializeDateValue()
        {
            var now = DateTimeOffset.UtcNow;

            var expectedSerializedString = string.Format("{{\"nullableDate\":\"{0}\"}}", now.ToString("yyyy-MM-dd"));

            var date = new DateTestClass
            {
                NullableDate = new Date(now.Year, now.Month, now.Day),
            };

            using var jsonSerializerWriter = new JsonSerializationWriter();
            jsonSerializerWriter.WriteObjectValue(string.Empty, date);
            var serializedStream = jsonSerializerWriter.GetSerializedContent();

            // Assert
            var streamReader = new StreamReader(serializedStream);
            Assert.Equal(expectedSerializedString, streamReader.ReadToEnd());
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

            using var jsonSerializerWriter = new JsonSerializationWriter();
            jsonSerializerWriter.WriteObjectValue(string.Empty, date);
            var serializedStream = jsonSerializerWriter.GetSerializedContent();

            // Assert
            using var streamReader = new StreamReader(serializedStream);
            var serializedString = streamReader.ReadToEnd();

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
            using var jsonSerializerWriter = new JsonSerializationWriter();
            jsonSerializerWriter.WriteObjectValue(string.Empty, testItemBody);

            // Assert
            var serializedStream = jsonSerializerWriter.GetSerializedContent();
            using var reader = new StreamReader(serializedStream, Encoding.UTF8);
            var serializedJsonString = reader.ReadToEnd();
            Assert.Equal(expectedSerializedString, serializedJsonString);
        }

        [Fact]
        public void SerializeObjectWithEmptyAdditionalData()
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
            using var jsonSerializerWriter = new JsonSerializationWriter();
            jsonSerializerWriter.WriteObjectValue(string.Empty, testItemBody);

            //Assert
            var serializedStream = jsonSerializerWriter.GetSerializedContent();
            using var reader = new StreamReader(serializedStream, Encoding.UTF8);
            var serializedString = reader.ReadToEnd();
            Assert.Equal(expectedSerializedString, serializedString);
            Assert.DoesNotContain("@odata.nextLink", serializedString);
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
            using var jsonSerializerWriter = new JsonSerializationWriter();
            jsonSerializerWriter.WriteObjectValue(string.Empty, testEmailAddress);

            // Assert
            var serializedStream = jsonSerializerWriter.GetSerializedContent();
            using var reader = new StreamReader(serializedStream, Encoding.UTF8);
            var serializedJsonString = reader.ReadToEnd();
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
        public void SerializeUploadSessionValues()
        {
            // Arrange
            var uploadSession = new UploadSession()
            {
                ExpirationDateTime = DateTimeOffset.Parse("2016-11-20T18:23:45.9356913+00:00"),
                UploadUrl = "http://localhost",
                NextExpectedRanges = new List<string> { "0 - 1000" }
            };
            using var jsonSerializerWriter = new JsonSerializationWriter();
            var expectedString = @"{""expirationDateTime"":""2016-11-20T18:23:45.9356913+00:00"",""nextExpectedRanges"":[""0 - 1000""],""uploadUrl"":""http://localhost""}";
            // Act
            jsonSerializerWriter.WriteObjectValue(string.Empty, uploadSession);
            // Assert
            // Get the json string from the stream.
            var serializedStream = jsonSerializerWriter.GetSerializedContent();
            using var reader = new StreamReader(serializedStream, Encoding.UTF8);
            var serializedJsonString = reader.ReadToEnd();
            // Expect the string to be ISO 8601-1:2019 format
            Assert.Equal(expectedString, serializedJsonString);
        }
    }
}
