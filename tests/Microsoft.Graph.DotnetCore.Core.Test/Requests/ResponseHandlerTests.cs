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
        public async Task HandleEventDeltaResponse()
        {
            // Arrange
            var deltaResponseHandler = new DeltaResponseHandler();

            // TestString represents a page of results with a nextLink. There are two changed events.
            // The events have key:value properties, key:object properties, and key:array properties.
            // To view and format this test string, replace all \" with ", and use a JSON formatter
            // to make it pretty.
            var testString = "{\"@odata.context\":\"https://graph.microsoft.com/v1.0/$metadata#Collection(event)\",\"@odata.nextLink\":\"https://graph.microsoft.com/v1.0/me/calendarView/delta?$skiptoken=R0usmci39OQxqJrxK4\",\"value\":[{\"@odata.type\":\"#microsoft.graph.event\",\"@odata.etag\":\"EZ9r3czxY0m2jz8c45czkwAAFXcvIw==\",\"subject\":\"Get food\",\"body\":{\"contentType\":\"html\",\"content\":\"\"},\"start\":{\"dateTime\":\"2016-12-10T19:30:00.0000000\",\"timeZone\":\"UTC\"},\"end\":{\"dateTime\":\"2016-12-10T21:30:00.0000000\",\"timeZone\":\"UTC\"},\"attendees\":[{\"emailAddress\":{\"name\":\"George\",\"address\":\"george@contoso.onmicrosoft.com\"}},{\"emailAddress\":{\"name\":\"Jane\",\"address\":\"jane@contoso.onmicrosoft.com\"}}],\"organizer\":{\"emailAddress\":{\"name\":\"Samantha Booth\",\"address\":\"samanthab@contoso.onmicrosoft.com\"}},\"id\":\"AAMkADVxTAAA=\"},{\"@odata.type\":\"#microsoft.graph.event\",\"@odata.etag\":\"WEZ9r3czxY0m2jz8c45czkwAAFXcvJA==\",\"subject\":\"Prepare food\",\"body\":{\"contentType\":\"html\",\"content\":\"\"},\"start\":{\"dateTime\":\"2016-12-10T22:00:00.0000000\",\"timeZone\":\"UTC\"},\"end\":{\"dateTime\":\"2016-12-11T00:00:00.0000000\",\"timeZone\":\"UTC\"},\"attendees\":[],\"organizer\":{\"emailAddress\":{\"name\":\"Samantha Booth\",\"address\":\"samanthab@contoso.onmicrosoft.com\"}},\"id\":\"AAMkADVxUAAA=\"}]}";

            var hrm = new HttpResponseMessage()
            {
                Content = new StringContent(testString, 
                                            Encoding.UTF8, 
                                            "application/json")
            };

            // Act
            var deltaServiceLibResponse = await deltaResponseHandler.HandleResponse<EventDeltaCollectionResponse>(hrm);
            var deltaJObjectResponse = await deltaResponseHandler.HandleResponse<JObject>(hrm);
            string attendeeName = (string)deltaJObjectResponse.SelectToken("value[0].attendees[0].emailAddress.name");
            //string attendeeName = (string)deltaJObjectResponse.SelectToken("value[0].changes.emailAddress.name");

            // IEventDeltaCollectionPage is what the service library provides.
            // Can't test against the service library model, since it has a reference to the signed
            // public version of this library. see issue #57 for more info.
            // https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/issues/57
            // Service library testing will need to happen in the service library repo.

            // Assert
            Assert.True(deltaServiceLibResponse.Value is IEventDeltaCollectionPage); // We create a valid ICollectionPage.
            Assert.Equal("George", attendeeName); // We maintain the expected response body when we change it.
            // TODO: Assert that we capture emailAddress.Name in the change list.
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
