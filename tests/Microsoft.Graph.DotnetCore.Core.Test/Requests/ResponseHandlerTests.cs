// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests
{
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;
    using Microsoft.Graph;
    
    public class ResponseHandlerTests
    {
        [Fact]
        public async Task HandleUserResponse()
        {
            // Arrange
            var responseHandler = new ResponseHandler(new Serializer());
            var hrm = new HttpResponseMessage()
            {
                Content = new StringContent(@"{
                    ""id"": ""123"",
                    ""givenName"": ""Joe"",
                    ""surName"": ""Brown"",
                    ""@odata.type"":""test""
                }", Encoding.UTF8, "application/json")
            };
            hrm.Headers.Add("test", "value");

            // Act
            var user = await responseHandler.HandleResponse<User>(hrm);

            //Assert
            Assert.Equal("123", user.Id);
            Assert.Equal("Joe", user.GivenName);
            Assert.Equal("Brown", user.Surname);
            Assert.Equal("OK", user.AdditionalData["statusCode"]);
            var headers = (JObject)(user.AdditionalData["responseHeaders"]);
            Assert.Equal("value", (string)headers["test"][0]);
        }

        [Fact]
        public async Task HandleEventDeltaResponseWithoutAttendees()
        {
            // Arrange
            var deltaResponseHandler = new DeltaResponseHandler();

            // No attendee
            var testString = "{\"@odata.context\":\"https://graph.microsoft.com/v1.0/$metadata#Collection(event)\",\"@odata.nextLink\":\"https://graph.microsoft.com/v1.0/me/calendarView/delta?$skiptoken=R0usmci39OQxqJrxK4\",\"value\":[{\"@odata.type\":\"#microsoft.graph.event\",\"@odata.etag\":\"EZ9r3czxY0m2jz8c45czkwAAFXcvIw==\",\"subject\":\"Get food\",\"body\":{\"contentType\":\"html\",\"content\":\"\"},\"start\":{\"dateTime\":\"2016-12-10T19:30:00.0000000\",\"timeZone\":\"UTC\"},\"end\":{\"dateTime\":\"2016-12-10T21:30:00.0000000\",\"timeZone\":\"UTC\"},\"organizer\":{\"emailAddress\":{\"name\":\"Samantha Booth\",\"address\":\"samanthab@contoso.onmicrosoft.com\"},\"customProp\":\"customValue\"},\"id\":\"AAMkADVxTAAA=\"},{\"@odata.type\":\"#microsoft.graph.event\",\"@odata.etag\":\"WEZ9r3czxY0m2jz8c45czkwAAFXcvJA==\",\"subject\":\"Prepare food\",\"body\":{\"contentType\":\"html\",\"content\":\"\"},\"start\":{\"dateTime\":\"2016-12-10T22:00:00.0000000\",\"timeZone\":\"UTC\"},\"end\":{\"dateTime\":\"2016-12-11T00:00:00.0000000\",\"timeZone\":\"UTC\"},\"attendees\":[],\"organizer\":{\"emailAddress\":{\"name\":\"Samantha Booth\",\"address\":\"samanthab@contoso.onmicrosoft.com\"}},\"id\":\"AAMkADVxUAAA=\"}]}";

            var hrm = new HttpResponseMessage()
            {
                Content = new StringContent(testString, 
                                            Encoding.UTF8, 
                                            "application/json")
            };

            // Act
            var deltaResponse = await deltaResponseHandler.HandleResponse<EventDeltaCollectionResponse>(hrm);
            bool hasChanges = deltaResponse.AdditionalData.TryGetValue("changes", out object changes);

            // Can't test against the actual model, see issue #57 for more info.
            var serializer = new Serializer();
            List<string> changeList = serializer.DeserializeObject<List<string>>(changes.ToString());
            
            // Assert
            Assert.True(hasChanges);
            Assert.Equal("value[0]['@odata.type']", changeList[0]);
            Assert.Equal("value[0]['@odata.etag']", changeList[1]);
            Assert.Equal("value[0].subject", changeList[2]);
            Assert.Equal("value[0].body", changeList[3]);
            Assert.Equal("value[0].body.contentType", changeList[4]);
            Assert.Equal("value[0].body.content", changeList[5]);
            Assert.Equal("value[0].start", changeList[6]);
            Assert.Equal("value[0].start.dateTime", changeList[7]);
            Assert.Equal("value[0].start.timeZone", changeList[8]);
            Assert.Equal("value[0].end", changeList[9]);
            Assert.Equal("value[0].end.dateTime", changeList[10]);
            Assert.Equal("value[0].end.timeZone", changeList[11]);
            Assert.Equal("value[0].organizer", changeList[12]);
            Assert.Equal("value[0].organizer.emailAddress", changeList[13]);
            Assert.Equal("value[0].organizer.emailAddress.name", changeList[14]);
            Assert.Equal("value[0].organizer.emailAddress.address", changeList[15]);
            Assert.Equal("value[0].organizer.customProp", changeList[16]);
            Assert.Equal("value[0].id", changeList[17]);
            Assert.Equal("value[1]['@odata.type']", changeList[18]);
        }

        [Fact]
        public async Task HandleEventDeltaResponseWithEmptyCollectionPage()
        {
            // Arrange
            var deltaResponseHandler = new DeltaResponseHandler();

            // Empty result
            var testString = "{\"@odata.context\":\"https://graph.microsoft.com/v1.0/$metadata#Collection(event)\",\"@odata.nextLink\":\"https://graph.microsoft.com/v1.0/me/calendarView/delta?$skiptoken=R0usmci39OQxqJrxK4\",\"value\":[]}";

            var hrm = new HttpResponseMessage()
            {
                Content = new StringContent(testString,
                                            Encoding.UTF8,
                                            "application/json")
            };

            // Act
            var deltaResponse = await deltaResponseHandler.HandleResponse<EventDeltaCollectionResponse>(hrm);
            bool hasChanges = deltaResponse.AdditionalData.TryGetValue("changes", out object changes);

            var serializer = new Serializer();
            List<string> changeList = serializer.DeserializeObject<List<string>>(changes.ToString());

            // Assert
            Assert.True(hasChanges);
            Assert.Empty(changeList);
        }
    }
}
