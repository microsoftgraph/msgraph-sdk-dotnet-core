// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Xunit;
    using Microsoft.Graph;
    using Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels;
    using System.Linq;
    using System;

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
                }", Encoding.UTF8, CoreConstants.MimeTypeNames.Application.Json)
            };
            hrm.Headers.Add("test", "value");

            // Act
            var user = await responseHandler.HandleResponse<TestUser>(hrm);

            //Assert
            Assert.Equal("123", user.Id);
            Assert.Equal("Joe", user.GivenName);
            Assert.Equal("Brown", user.Surname);
        }

        /// <summary>
        /// We assumed that JSON arrays only contained objects. We need to test that
        /// we account for array of primitives.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HandleEventDeltaResponseWithArrayOfPrimitives()
        {
            // Arrange
            var deltaResponseHandler = new DeltaResponseHandler();
        
            // Contains string, int, and boolean arrays.
            var testString = "{\"@odata.context\":\"https://graph.microsoft.com/v1.0/$metadata#Collection(user)\",\"@odata.nextLink\":\"https://graph.microsoft.com/v1.0/users/delta?$skiptoken=R0usmci39O\",\"value\":[{\"id\":\"AAMkADVxTAAA=\",\"arrayOfString\":[\"SMTP:alexd@contoso.com\",\"SMTP:meganb@contoso.com\"]},{\"id\":\"AAMkADVxUAAA=\",\"arrayOfBool\":[true,false]},{\"id\":\"AAMkADVxVAAA=\",\"arrayOfInt\":[2,5]}]}";
        
            var hrm = new HttpResponseMessage()
            {
                Content = new StringContent(testString,
                                Encoding.UTF8,
                                CoreConstants.MimeTypeNames.Application.Json)
            };
        
            // Act
            var deltaServiceLibResponse = await deltaResponseHandler.HandleResponse<TestEventDeltaCollectionResponse>(hrm);
        
            var deltaJObjectResponse = await deltaResponseHandler.HandleResponse<JsonElement>(hrm);
            string actualStringValue = deltaJObjectResponse.GetProperty("value").EnumerateArray().ElementAt(0)
                .GetProperty("arrayOfString").EnumerateArray().ElementAt(0).ToString(); //value[0].arrayOfString[0]
            bool actualBoolValue = Convert.ToBoolean(deltaJObjectResponse.GetProperty("value").EnumerateArray()
                .ElementAt(1).GetProperty("arrayOfBool").EnumerateArray().ElementAt(1).ToString()); //value[1].arrayOfBool[1]
            int actualIntValue = Convert.ToInt32((string)deltaJObjectResponse.GetProperty("value").EnumerateArray()
                .ElementAt(2).GetProperty("arrayOfInt").EnumerateArray().ElementAt(1).ToString());// value[2].arrayOfInt[1]

            string arrayOfString = deltaJObjectResponse.GetProperty("value").EnumerateArray().ElementAt(0)
                .GetProperty("changes").EnumerateArray().ElementAt(2).ToString();
            string arrayOfBool = deltaJObjectResponse.GetProperty("value").EnumerateArray().ElementAt(1)
                .GetProperty("changes").EnumerateArray().ElementAt(2).ToString();
            string arrayOfInt = deltaJObjectResponse.GetProperty("value").EnumerateArray().ElementAt(2)
                .GetProperty("changes").EnumerateArray().ElementAt(2).ToString();
        
            // Assert that the value is set.
            Assert.Equal("SMTP:alexd@contoso.com", actualStringValue);
            Assert.False(actualBoolValue);
            Assert.Equal(5, actualIntValue);
        
            // Assert that the change manifest is set.
            Assert.Equal("arrayOfString[1]", arrayOfString); // The third change is the second string array item.
            Assert.Equal("arrayOfBool[1]", arrayOfBool);
            Assert.Equal("arrayOfInt[1]", arrayOfInt);
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
                                            CoreConstants.MimeTypeNames.Application.Json)
            };
        
            // Act
            var deltaServiceLibResponse = await deltaResponseHandler.HandleResponse<TestEventDeltaCollectionResponse>(hrm);
            var deltaJObjectResponse = await deltaResponseHandler.HandleResponse<JsonElement>(hrm);
            string attendeeName = deltaJObjectResponse.GetProperty("value").EnumerateArray().ElementAt(0)
                .GetProperty("attendees").EnumerateArray().ElementAt(0).GetProperty("emailAddress").GetProperty("name")
                .ToString(); // value[0].attendees[0].emailAddress.name
            string attendeeNameInChangelist = deltaJObjectResponse.GetProperty("value").EnumerateArray().ElementAt(0)
                .GetProperty("changes").EnumerateArray().ElementAt(9).ToString();//eltaJObjectResponse["value"][0]["changes"][9]
            
            var collectionPage = deltaServiceLibResponse.Value;
            var collectionPageHasChanges = collectionPage[0].AdditionalData.TryGetValue("changes", out object obj);
            
            // IEventDeltaCollectionPage is what the service library provides.
            // Can't test against the service library model, since it has a reference to the signed
            // public version of this library. see issue #57 for more info.
            // https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/issues/57
            // Service library testing will need to happen in the service library repo once this is published on NuGet.
            
            // Assert
            Assert.NotEmpty(deltaServiceLibResponse.Value);
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
                                            CoreConstants.MimeTypeNames.Application.Json)
            };

            // Act
            var deltaJObjectResponse = await deltaResponseHandler.HandleResponse<JsonElement>(hrm);
            var itemsCount = deltaJObjectResponse.GetProperty("value").GetArrayLength();
            var odataContextFromJObject = deltaJObjectResponse.GetProperty("@odata.context").ToString();
            var odataDeltalinkFromJObject = deltaJObjectResponse.GetProperty("@odata.deltaLink").ToString();

            Assert.Equal(0 ,itemsCount); // We don't expect items in an empty collection page
            Assert.Equal(odataContext, odataContextFromJObject); // We expect that the odata.context isn't transformed.
            Assert.Equal(odataDeltalink, odataDeltalinkFromJObject); // We expect that the odata.deltalink isn't transformed.
        }

        [Fact]
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
                                            CoreConstants.MimeTypeNames.Application.Json)
            };

            // Assuming this is the developers model that they want to update based on delta query.
            TestEvent myModel = new TestEvent()
            {
                Subject = "Original subject",
                Body = new TestItemBody()
                {
                    Content = "Original body",
                    ContentType = TestBodyType.Text
                },
                AdditionalData = new Dictionary<string,object>()
            };

            // Act
            var deltaServiceLibResponse = await deltaResponseHandler.HandleResponse<TestEventDeltaCollectionResponse>(hrm);
            var eventsDeltaCollectionPage = deltaServiceLibResponse.Value;
            eventsDeltaCollectionPage[0].AdditionalData.TryGetValue("changes", out object changes);
            var changesElement = (JsonElement)changes;
            var changeList = JsonSerializer.Deserialize<List<string>>(changesElement.GetRawText());

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
                    myModel.Body = new TestItemBody();
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
                        var attendees = new List<TestAttendee>();
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
        [Fact]
        public async Task HandleEventDeltaResponseWithRemovedItem()
        {
            // Arrange
            var deltaResponseHandler = new DeltaResponseHandler();

            // Result string with one removed item.
            var testString = "{\"@odata.context\":\"https://graph.microsoft.com/v1.0/$metadata#Collection(event)\",\"@odata.nextLink\":\"https://graph.microsoft.com/v1.0/me/calendarView/delta?$skiptoken=R0usmci39OQxqJrxK4\",\"value\":[{\"@removed\":{\"reason\":\"removed\"},\"id\":\"AAMkADVxTAAA=\"}]}";

            var hrm = new HttpResponseMessage()
            {
                Content = new StringContent(testString,
                                            Encoding.UTF8,
                                            CoreConstants.MimeTypeNames.Application.Json)
            };

            // Act
            var deltaServiceLibResponse = await deltaResponseHandler.HandleResponse<TestEventDeltaCollectionResponse>(hrm);
            var eventsDeltaCollectionPage = deltaServiceLibResponse.Value;
            eventsDeltaCollectionPage[0].AdditionalData.TryGetValue("changes", out object changes);
            var changesElement = (JsonElement)changes;
            var changeList = JsonSerializer.Deserialize<List<string>>(changesElement.GetRawText());

            // Assert
            Assert.True(changeList.Exists(x => x.Equals("@removed.reason")));
        }

        [Fact]
        public async Task HandleEventDeltaResponseWithEmptyCollectionProperty()
        {
            // Arrange
            var deltaResponseHandler = new DeltaResponseHandler();

            // TestString represents a page of results with a nextLink. There are two changed events.
            // The events have key:value properties, key:object properties, and key:array properties.
            // To view and format this test string, replace all \" with ", and use a JSON formatter
            // to make it pretty.

            // In this scenario the attendees for the first event have been removed and the api has returned 
            // an empty collection to the property
            var testString = "{\"@odata.context\":\"https://graph.microsoft.com/v1.0/$metadata#Collection(event)\",\"@odata.nextLink\":\"https://graph.microsoft.com/v1.0/me/calendarView/delta?$skiptoken=R0usmci39OQxqJrxK4\",\"value\":[{\"@odata.type\":\"#microsoft.graph.event\",\"@odata.etag\":\"EZ9r3czxY0m2jz8c45czkwAAFXcvIw==\",\"subject\":\"Get food\",\"body\":{\"contentType\":\"html\",\"content\":\"\"},\"start\":{\"dateTime\":\"2016-12-10T19:30:00.0000000\",\"timeZone\":\"UTC\"},\"end\":{\"dateTime\":\"2016-12-10T21:30:00.0000000\",\"timeZone\":\"UTC\"},\"attendees\":[],\"organizer\":{\"emailAddress\":{\"name\":\"Samantha Booth\",\"address\":\"samanthab@contoso.onmicrosoft.com\"}},\"id\":\"AAMkADVxTAAA=\"},{\"@odata.type\":\"#microsoft.graph.event\",\"@odata.etag\":\"WEZ9r3czxY0m2jz8c45czkwAAFXcvJA==\",\"subject\":\"Prepare food\",\"body\":{\"contentType\":\"html\",\"content\":\"\"},\"start\":{\"dateTime\":\"2016-12-10T22:00:00.0000000\",\"timeZone\":\"UTC\"},\"end\":{\"dateTime\":\"2016-12-11T00:00:00.0000000\",\"timeZone\":\"UTC\"},\"attendees\":[],\"organizer\":{\"emailAddress\":{\"name\":\"Samantha Booth\",\"address\":\"samanthab@contoso.onmicrosoft.com\"}},\"id\":\"AAMkADVxUAAA=\"}]}";

            var hrm = new HttpResponseMessage()
            {
                Content = new StringContent(testString,
                                            Encoding.UTF8,
                                            CoreConstants.MimeTypeNames.Application.Json)
            };

            // Act
            var deltaServiceLibResponse = await deltaResponseHandler.HandleResponse<TestEventDeltaCollectionResponse>(hrm);

            // Assert
            Assert.True(deltaServiceLibResponse.Value[0].AdditionalData.TryGetValue("changes", out object changesElement)); // The first element has a list of changes
            
            // Deserialize the change list to a list of strings
            var firstItemChangeList = JsonSerializer.Deserialize<List<string>>(((JsonElement)changesElement).GetRawText());

            Assert.NotNull(firstItemChangeList);
            Assert.NotEmpty(firstItemChangeList);
            Assert.Contains("attendees", firstItemChangeList); // assert that the empty collection property is added to the list
        }
    }
}
