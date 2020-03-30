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
    using System.Linq;

    public class ResponseHandlerTests
    {
        [Fact(Skip = "Service Library needs to support System.Text.Json Attributes")]
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

        [Fact(Skip = "To Do: Refactor Delta Response Handler")]
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
            string attendeeNameInChangelist = (deltaJObjectResponse["value"][0]["changes"] as JArray)[9].ToString();
            var collectionPage = deltaServiceLibResponse.Value as CollectionPage<Event>;
            var collectionPageHasChanges = collectionPage[0].AdditionalData.TryGetValue("changes", out object obj);

            // IEventDeltaCollectionPage is what the service library provides.
            // Can't test against the service library model, since it has a reference to the signed
            // public version of this library. see issue #57 for more info.
            // https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/issues/57
            // Service library testing will need to happen in the service library repo once this is published on NuGet.

            // Assert
            Assert.True(deltaServiceLibResponse.Value is IEventDeltaCollectionPage); // We create a valid ICollectionPage.
            Assert.Equal("George", attendeeName); // We maintain the expected response body when we change it.
            Assert.Equal("attendees[0].emailAddress.name", attendeeNameInChangelist); // We expect that this property is in changelist.
            Assert.True(collectionPageHasChanges); // We expect that the CollectionPage is populated with the changes.
        }

        /// <summary>
        /// Occurs in the response when we call with the deltalink and there are no items to sync.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HandleEventDeltaResponseWithEmptyCollectionPage()
        {
            // Arrange
            var deltaResponseHandler = new DeltaResponseHandler();
            var odataContext = @"https://graph.microsoft.com/v1.0/$metadata#Collection(event)";
            var odataDeltalink = @"https://graph.microsoft.com/v1.0/me/mailfolders/inbox/messages/delta?$deltatoken=LztZwWjo5IivWBhyxw5rACKxf7mPm0oW6JZZ7fvKxYPS_67JnEYmfQQMPccy6FRun0DWJF5775dvuXxlZnMYhBubC1v4SBVT9ZjO8f7acZI.uCdGKSBS4YxPEbn_Q5zxLSq91WhpGoz9ZKeNZHMWgSA";


            // Empty result
            var testString = "{\"@odata.context\":\"" + odataContext + "\",\"@odata.deltaLink\":\"" + odataDeltalink + "\",\"value\":[]}";

            var hrm = new HttpResponseMessage()
            {
                Content = new StringContent(testString,
                                            Encoding.UTF8,
                                            "application/json")
            };

            // Act
            var deltaJObjectResponse = await deltaResponseHandler.HandleResponse<JObject>(hrm);
            var hasItems = deltaJObjectResponse["value"].HasValues;
            var odataContextFromJObject = deltaJObjectResponse["@odata.context"].ToString();
            var odataDeltalinkFromJObject = deltaJObjectResponse["@odata.deltaLink"].ToString();

            Assert.False(hasItems); // We don't expect items in an empty collection page
            Assert.Equal(odataContext, odataContextFromJObject); // We expect that the odata.context isn't transformed.
            Assert.Equal(odataDeltalink, odataDeltalinkFromJObject); // We expect that the odata.deltalink isn't transformed.
        }

        [Fact(Skip = "To Do: Refactor Delta Response Handler")]
        public async Task HandleEventDeltaResponseWithNullValues()
        {
            // Arrange
            var deltaResponseHandler = new DeltaResponseHandler();

            // TestString represents a page of results with a nextLink. There are two changed events.
            // The events have key:value properties, key:object properties, and key:array properties.
            // To view and format this test string, replace all \" with ", and use a JSON formatter
            // to make it pretty.
            // value[0].subject == null
            var testString = "{\"@odata.context\":\"https://graph.microsoft.com/v1.0/$metadata#Collection(event)\",\"@odata.nextLink\":\"https://graph.microsoft.com/v1.0/me/calendarView/delta?$skiptoken=R0usmci39OQxqJrxK4\",\"value\":[{\"@odata.type\":\"#microsoft.graph.event\",\"@odata.etag\":\"EZ9r3czxY0m2jz8c45czkwAAFXcvIw==\",\"subject\":null,\"body\":{\"contentType\":\"html\",\"content\":\"<p>Updated content</p>\"},\"start\":{\"dateTime\":\"2016-12-10T19:30:00.0000000\",\"timeZone\":\"UTC\"},\"end\":{\"dateTime\":\"2016-12-10T21:30:00.0000000\",\"timeZone\":\"UTC\"},\"attendees\":[{\"emailAddress\":{\"name\":\"George\",\"address\":\"george@contoso.onmicrosoft.com\"}},{\"emailAddress\":{\"name\":\"Jane\",\"address\":\"jane@contoso.onmicrosoft.com\"}}],\"organizer\":{\"emailAddress\":{\"name\":\"Samantha Booth\",\"address\":\"samanthab@contoso.onmicrosoft.com\"}},\"id\":\"AAMkADVxTAAA=\"},{\"@odata.type\":\"#microsoft.graph.event\",\"@odata.etag\":\"WEZ9r3czxY0m2jz8c45czkwAAFXcvJA==\",\"subject\":\"Prepare food\",\"body\":{\"contentType\":\"html\",\"content\":\"\"},\"start\":{\"dateTime\":\"2016-12-10T22:00:00.0000000\",\"timeZone\":\"UTC\"},\"end\":{\"dateTime\":\"2016-12-11T00:00:00.0000000\",\"timeZone\":\"UTC\"},\"attendees\":[],\"organizer\":{\"emailAddress\":{\"name\":\"Samantha Booth\",\"address\":\"samanthab@contoso.onmicrosoft.com\"}},\"id\":\"AAMkADVxUAAA=\"}]}";

            var hrm = new HttpResponseMessage()
            {
                Content = new StringContent(testString,
                                            Encoding.UTF8,
                                            "application/json")
            };

            // Assuming this is the developers model that they want to update based on delta query.
            Event myModel = new Event()
            {
                Subject = "Original subject",
                Body = new ItemBody()
                {
                    Content = "Original body",
                    ContentType = BodyType.Text
                },
                AdditionalData = new Dictionary<string,object>()
            };

            // Act
            var deltaServiceLibResponse = await deltaResponseHandler.HandleResponse<EventDeltaCollectionResponse>(hrm);
            var eventsDeltaCollectionPage = deltaServiceLibResponse.Value as CollectionPage<Event>;
            eventsDeltaCollectionPage[0].AdditionalData.TryGetValue("changes", out object changes);
            var changeList = (changes as JArray).ToObject<List<string>>();

            // Updating a non-schematized property on a model such as instance annotations, open types,
            // and schema extensions. We can assume that a customer's  model would not use a dictionary.
            if (changeList.Exists(x => x.Equals("@odata.etag")))
            {
                eventsDeltaCollectionPage[0].AdditionalData.TryGetValue("@odata.etag", out object odataEtag);
                myModel.AdditionalData["@odata.etag"] = odataEtag.ToString();
            }

            // Core scenario - update schematized property regardless of it is set to null.
            // This property has been set to null in the response. We can be confident that
            // whatever the value set is correct, regardless whether it is null.
            if (changeList.Exists(x => x.Equals("subject")))
            {
                myModel.Subject = eventsDeltaCollectionPage[0].Subject;
            }

            // Update the value on a complex type property's value. Developer can't just replace the body
            // as that could result in overwriting other unchanged property. Essentially, they need to inspect
            // every leaf node in the selected property set.
            if (changeList.Exists(x => x.Equals("body.content"))) // 
            {
                if (myModel.Body == null)
                {
                    myModel.Body = new ItemBody();
                }

                myModel.Body.Content = eventsDeltaCollectionPage[0].Body.Content;
            }

            // Update complex type property's value when the value is a collection of objects.
            // We don't know whether this is an update or add without querying the client model.
            // We will need to check each object in the model.
            var attendeesChangelist = changeList.FindAll(x => x.Contains("attendees"));
            if (attendeesChangelist.Count > 0)
            {
                // This is where if we provided the delta response as a JSON object, 
                // we could let the developer use JMESPath to query the changes.
                if (changeList.Exists(x => x.Equals("attendees[0].emailAddress.name")))
                {
                    if (myModel.Attendees == null) // Attendees are being added for the first time.
                    {
                        var attendees = new List<Attendee>();
                        attendees.AddRange(eventsDeltaCollectionPage[0].Attendees);
                        myModel.Attendees = attendees;
                    }
                    else // Attendees list is being updated.
                    {
                        // We need to inspect each object, and determine which objects and properties 
                        // need to be initialized and/or updated.
                    }
                }
            }

            Assert.NotNull(changeList);
            Assert.Null(myModel.Subject);
            Assert.Equal("<p>Updated content</p>", myModel.Body.Content);
            Assert.NotNull(eventsDeltaCollectionPage[0].AdditionalData["@odata.etag"]);
            Assert.Collection(myModel.Attendees,
                attendee1 =>
                {
                    Assert.Equal("George", attendee1.EmailAddress.Name);
                    Assert.Equal("george@contoso.onmicrosoft.com", attendee1.EmailAddress.Address);
                },
                attendee2 => 
                {
                    Assert.Equal("Jane", attendee2.EmailAddress.Name);
                    Assert.Equal("jane@contoso.onmicrosoft.com", attendee2.EmailAddress.Address);
                }
            );
        }

        /// <summary>
        /// Occurs in the response when we call with the deltalink and there are no items to sync.
        /// </summary>
        /// <returns></returns>
        [Fact(Skip = "To Do: Refactor Delta Response Handler")]
        public async Task HandleEventDeltaResponseWithRemovedItem()
        {
            // Arrange
            var deltaResponseHandler = new DeltaResponseHandler();
            var odataContext = @"https://graph.microsoft.com/v1.0/$metadata#Collection(event)";
            var odataDeltalink = @"https://graph.microsoft.com/v1.0/me/mailfolders/inbox/messages/delta?$deltatoken=LztZwWjo5IivWBhyxw5rACKxf7mPm0oW6JZZ7fvKxYPS_67JnEYmfQQMPccy6FRun0DWJF5775dvuXxlZnMYhBubC1v4SBVT9ZjO8f7acZI.uCdGKSBS4YxPEbn_Q5zxLSq91WhpGoz9ZKeNZHMWgSA";


            // Result string with one removed item.
            var testString = "{\"@odata.context\":\"https://graph.microsoft.com/v1.0/$metadata#Collection(event)\",\"@odata.nextLink\":\"https://graph.microsoft.com/v1.0/me/calendarView/delta?$skiptoken=R0usmci39OQxqJrxK4\",\"value\":[{\"@removed\":{\"reason\":\"removed\"},\"id\":\"AAMkADVxTAAA=\"}]}";

            var hrm = new HttpResponseMessage()
            {
                Content = new StringContent(testString,
                                            Encoding.UTF8,
                                            "application/json")
            };

            // Act
            var deltaServiceLibResponse = await deltaResponseHandler.HandleResponse<EventDeltaCollectionResponse>(hrm);
            var eventsDeltaCollectionPage = deltaServiceLibResponse.Value as CollectionPage<Event>;
            eventsDeltaCollectionPage[0].AdditionalData.TryGetValue("changes", out object changes);
            var changeList = (changes as JArray).ToObject<List<string>>();

            // Assert
            Assert.True(changeList.Exists(x => x.Equals("@removed.reason")));
        }
    }
}
