// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.Graph;
    using Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels;
    using Microsoft.Kiota.Abstractions.Serialization;
    using Microsoft.Kiota.Serialization.Json;
    using Xunit;

    public class ResponseHandlerTests
    {
        public ResponseHandlerTests()
        {
            // register the default serialization instance as the generator would.
            ParseNodeFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories.TryAdd(CoreConstants.MimeTypeNames.Application.Json, new JsonParseNodeFactory());
            SerializationWriterFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories.TryAdd(CoreConstants.MimeTypeNames.Application.Json, new JsonSerializationWriterFactory());
        }

        [Fact]
        public async Task HandleUserResponseAsync()
        {
            // Arrange
            var responseHandler = new ResponseHandler<TestUser>();
            var hrm = new HttpResponseMessage()
            {
                Content = new StringContent(@"{
                    ""id"": ""123"",
                    ""givenName"": ""Joe"",
                    ""surname"": ""Brown"",
                    ""@odata.type"":""test""
                }", Encoding.UTF8, CoreConstants.MimeTypeNames.Application.Json)
            };
            hrm.Headers.Add("test", "value");

            // Act
            var user = await responseHandler.HandleResponseAsync<HttpResponseMessage, TestUser>(hrm, null);

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
        public async Task HandleEventDeltaResponseWithArrayOfPrimitivesAsync()
        {
            // Arrange
            var deltaResponseHandler = new DeltaResponseHandler<TestEventDeltaCollectionResponse>();

            // Contains string, int, and boolean arrays.
            var testString = "{\"@odata.context\":\"https://graph.microsoft.com/v1.0/$metadata#Collection(user)\",\"@odata.nextLink\":\"https://graph.microsoft.com/v1.0/users/delta?$skiptoken=R0usmci39O\",\"value\":[{\"id\":\"AAMkADVxTAAA=\",\"arrayOfString\":[\"SMTP:alexd@contoso.com\",\"SMTP:meganb@contoso.com\"]},{\"id\":\"AAMkADVxUAAA=\",\"arrayOfBool\":[true,false]},{\"id\":\"AAMkADVxVAAA=\",\"arrayOfInt\":[2,5]}]}";

            var hrm = new HttpResponseMessage()
            {
                Content = new StringContent(testString,
                                Encoding.UTF8,
                                CoreConstants.MimeTypeNames.Application.Json)
            };

            // Act
            var deltaServiceLibResponse = await deltaResponseHandler.HandleResponseAsync<HttpResponseMessage, TestEventDeltaCollectionResponse>(hrm, null);

            var collectionPage = deltaServiceLibResponse.Value;
            var stringCollection = collectionPage[0].AdditionalData["arrayOfString"] as UntypedArray;
            UntypedString actualStringValue = stringCollection.GetValue().ElementAt(0) as UntypedString; //value[0].arrayOfString[0]
            var boolCollection = collectionPage[1].AdditionalData["arrayOfBool"] as UntypedArray;
            UntypedBoolean actualBoolValue = boolCollection.GetValue().ElementAt(1) as UntypedBoolean; //value[1].arrayOfBool[1]
            var intCollection = collectionPage[2].AdditionalData["arrayOfInt"] as UntypedArray;
            UntypedInteger actualIntValue = intCollection.GetValue().ElementAt(1) as UntypedInteger;// value[2].arrayOfInt[1]

            UntypedString arrayOfString = ((UntypedArray)collectionPage[0].AdditionalData["changes"]).GetValue().ElementAt(2) as UntypedString;
            UntypedString arrayOfBool = ((UntypedArray)collectionPage[1].AdditionalData["changes"]).GetValue().ElementAt(2) as UntypedString;
            UntypedString arrayOfInt = ((UntypedArray)collectionPage[2].AdditionalData["changes"]).GetValue().ElementAt(2) as UntypedString;

            // Assert that the value is set.
            Assert.Equal("SMTP:alexd@contoso.com", actualStringValue.GetValue());
            Assert.False(actualBoolValue.GetValue());
            Assert.Equal(5, actualIntValue.GetValue());

            // Assert that the change manifest is set.
            Assert.Equal("arrayOfString[1]", arrayOfString.GetValue()); // The third change is the second string array item.
            Assert.Equal("arrayOfBool[1]", arrayOfBool.GetValue());
            Assert.Equal("arrayOfInt[1]", arrayOfInt.GetValue());
        }

        [Fact]
        public async Task HandleEventDeltaResponseAsync()
        {
            // Arrange
            var deltaResponseHandler = new DeltaResponseHandler<TestEventDeltaCollectionResponse>();

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
            var deltaServiceLibResponse = await deltaResponseHandler.HandleResponseAsync<HttpResponseMessage, TestEventDeltaCollectionResponse>(hrm, null);
            var collectionPage = deltaServiceLibResponse.Value;
            string attendeeName = collectionPage.First().Attendees.First().EmailAddress.Name;

            var collectionPageHasChanges = collectionPage[0].AdditionalData.TryGetValue("changes", out var obj);

            // IEventDeltaCollectionPage is what the service library provides.
            // Can't test against the service library model, since it has a reference to the signed
            // public version of this library. see issue #57 for more info.
            // https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/issues/57
            // Service library testing will need to happen in the service library repo once this is published on NuGet.

            // Assert
            Assert.NotEmpty(deltaServiceLibResponse.Value);
            Assert.Equal("George", attendeeName); // We maintain the expected response body when we change it.
            var objectCollection = obj as UntypedArray;
            var expectedItem = objectCollection.GetValue().ElementAt(9) as UntypedString;
            Assert.Equal("attendees[0].emailAddress.name", expectedItem.GetValue()); // We expect that this property is in change list.
            Assert.True(collectionPageHasChanges); // We expect that the CollectionPage is populated with the changes.
        }

        /// <summary>
        /// Occurs in the response when we call with the deltalink and there are no items to sync.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HandleEventDeltaResponseWithEmptyCollectionPageAsync()
        {
            // Arrange
            var deltaResponseHandler = new DeltaResponseHandler<TestEventDeltaCollectionResponse>();
            var odataContext = @"https://graph.microsoft.com/v1.0/$metadata#Collection(event)";
            var odataDeltaLink = @"https://graph.microsoft.com/v1.0/me/mailfolders/inbox/messages/delta?$deltatoken=LztZwWjo5IivWBhyxw5rACKxf7mPm0oW6JZZ7fvKxYPS_67JnEYmfQQMPccy6FRun0DWJF5775dvuXxlZnMYhBubC1v4SBVT9ZjO8f7acZI.uCdGKSBS4YxPEbn_Q5zxLSq91WhpGoz9ZKeNZHMWgSA";


            // Empty result
            var testString = "{\"@odata.context\":\"" + odataContext + "\",\"@odata.deltaLink\":\"" + odataDeltaLink + "\",\"value\":[]}";

            var hrm = new HttpResponseMessage()
            {
                Content = new StringContent(testString,
                                            Encoding.UTF8,
                                            CoreConstants.MimeTypeNames.Application.Json)
            };

            // Act
            var deltaServiceLibResponse = await deltaResponseHandler.HandleResponseAsync<HttpResponseMessage, TestEventDeltaCollectionResponse>(hrm, null);
            var collectionPage = deltaServiceLibResponse.Value;
            var itemsCount = collectionPage.Count;
            var odataContextFromJObject = deltaServiceLibResponse.AdditionalData["@odata.context"] as string;
            var odataDeltaLinkFromJObject = deltaServiceLibResponse.AdditionalData["@odata.deltaLink"] as string;

            Assert.Equal(0, itemsCount); // We don't expect items in an empty collection page
            Assert.Equal(odataContext, odataContextFromJObject); // We expect that the odata.context isn't transformed.
            Assert.Equal(odataDeltaLink, odataDeltaLinkFromJObject); // We expect that the odata.deltalink isn't transformed.
        }

        [Fact]
        public async Task HandleEventDeltaResponseWithNullValuesAsync()
        {
            // Arrange
            var deltaResponseHandler = new DeltaResponseHandler<TestEventDeltaCollectionResponse>();

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
                AdditionalData = new Dictionary<string, object>()
            };

            // Act
            var deltaServiceLibResponse = await deltaResponseHandler.HandleResponseAsync<HttpResponseMessage, TestEventDeltaCollectionResponse>(hrm, null);
            var eventsDeltaCollectionPage = deltaServiceLibResponse.Value;
            eventsDeltaCollectionPage[0].AdditionalData.TryGetValue("changes", out object changes);
            var changeList = JsonSerializer.Deserialize<List<string>>(await KiotaJsonSerializer.SerializeAsStringAsync(changes as UntypedArray));

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
        public async Task HandleEventDeltaResponseWithRemovedItemAsync()
        {
            // Arrange
            var deltaResponseHandler = new DeltaResponseHandler<TestEventDeltaCollectionResponse>();

            // Result string with one removed item.
            var testString = "{\"@odata.context\":\"https://graph.microsoft.com/v1.0/$metadata#Collection(event)\",\"@odata.nextLink\":\"https://graph.microsoft.com/v1.0/me/calendarView/delta?$skiptoken=R0usmci39OQxqJrxK4\",\"value\":[{\"@removed\":{\"reason\":\"removed\"},\"id\":\"AAMkADVxTAAA=\"}]}";

            var hrm = new HttpResponseMessage()
            {
                Content = new StringContent(testString,
                                            Encoding.UTF8,
                                            CoreConstants.MimeTypeNames.Application.Json)
            };

            // Act
            var deltaServiceLibResponse = await deltaResponseHandler.HandleResponseAsync<HttpResponseMessage, TestEventDeltaCollectionResponse>(hrm, null);
            var eventsDeltaCollectionPage = deltaServiceLibResponse.Value;
            eventsDeltaCollectionPage[0].AdditionalData.TryGetValue("changes", out object changes);
            var changeList = JsonSerializer.Deserialize<List<string>>(await KiotaJsonSerializer.SerializeAsStringAsync(changes as UntypedArray));

            // Assert
            Assert.True(changeList.Exists(x => x.Equals("@removed.reason")));
        }

        [Fact]
        public async Task HandleEventDeltaResponseWithEmptyCollectionPropertyAsync()
        {
            // Arrange
            var deltaResponseHandler = new DeltaResponseHandler<TestEventDeltaCollectionResponse>();

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
            var deltaServiceLibResponse = await deltaResponseHandler.HandleResponseAsync<HttpResponseMessage, TestEventDeltaCollectionResponse>(hrm, null);

            // Assert
            Assert.True(deltaServiceLibResponse.Value[0].AdditionalData.TryGetValue("changes", out object changesElement)); // The first element has a list of changes

            // Deserialize the change list to a list of strings
            var firstItemChangeList = JsonSerializer.Deserialize<List<string>>(await KiotaJsonSerializer.SerializeAsStringAsync(changesElement as UntypedNode));

            Assert.NotNull(firstItemChangeList);
            Assert.NotEmpty(firstItemChangeList);
            Assert.Contains("attendees", firstItemChangeList); // assert that the empty collection property is added to the list
        }
    }
}
