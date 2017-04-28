using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Graph.Test.Requests.Functional
{
    //[Ignore]
    [TestClass]
    public class EventTests : GraphTestBase
    {
        // https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/api/user_post_events
        // https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/api/singlevaluelegacyextendedproperty_post_singlevalueextendedproperties
        // https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/resources/extended-properties-overview
        [TestMethod]
        public async System.Threading.Tasks.Task EventCreateExtendedProperty()
        {
            try
            {
                var myEvent = new Microsoft.Graph.Event();
                myEvent.Subject = "Lunch with my friend";
                myEvent.Body = new ItemBody() { ContentType = BodyType.Text, Content = "Catch up on old times." };
                myEvent.Start = new DateTimeTimeZone() { DateTime = "2017-05-29T12:00:00", TimeZone = "Pacific Standard Time" };
                myEvent.End = new DateTimeTimeZone() { DateTime = "2017-05-29T13:00:00", TimeZone = "Pacific Standard Time" };
                myEvent.Location = new Location() { DisplayName = "In.gredients" };
                myEvent.SingleValueExtendedProperties = new EventSingleValueExtendedPropertiesCollectionPage();


                var myCustomIdentifier = "String {66f5a359-4659-4830-9070-00040ec6ac6e} Name courseId";

                var myCustomExtendedProperty = new SingleValueLegacyExtendedProperty()
                                                    {
                                                        Id = myCustomIdentifier,
                                                        Value = "1234567"
                                                    };

                myEvent.SingleValueExtendedProperties.Add(myCustomExtendedProperty);

                // Create the event with the extended property in the service. 
                var mySyncdEvent = await graphClient.Me.Calendar.Events.Request()
                                                                       .AddAsync(myEvent);

                // Get the event with the extended property.
                var mySyncdEventWithExtendedProperty = await graphClient.Me
                                                                        .Calendar
                                                                        .Events
                                                                        .Request()
                                                                        .Expand($"singleValueExtendedProperties($filter=id eq '{myCustomIdentifier}')")
                                                                        .GetAsync();

                Assert.IsNotNull(mySyncdEventWithExtendedProperty[0].SingleValueExtendedProperties);

                // Delete the event we just created.
                await graphClient.Me.Events[mySyncdEventWithExtendedProperty[0].Id].Request().DeleteAsync();
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
            }
        }


        [TestMethod]
        public async System.Threading.Tasks.Task EventGetCalendarView()
        {
            try
            {
                // http://graph.microsoft.io/en-us/docs/api-reference/v1.0/api/user_list_calendarview
                var queryOptions = new List<QueryOption>()
                {
                    new QueryOption("startDateTime", DateTime.Today.ToUniversalTime().ToString()),
                    new QueryOption("endDateTime", DateTime.Today.AddDays(1).ToUniversalTime().ToString())
                };

                var todaysEvents = await graphClient.Me.CalendarView.Request(queryOptions).GetAsync();
                Assert.IsNotNull(todaysEvents, "Expected a UserCalendarViewCollectionPage object.");
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
            }
        }
    }
}
