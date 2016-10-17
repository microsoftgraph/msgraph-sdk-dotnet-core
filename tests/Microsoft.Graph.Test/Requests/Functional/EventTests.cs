using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Graph.Test.Requests.Functional
{
    [Ignore]
    [TestClass]
    public class EventTests : GraphTestBase
    {
        [TestMethod]
        public async Task EventGetCalendarView()
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
