using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Graph.Test.Requests.Functional
{
    [Ignore]
    [TestClass]
    public class ErrorTests : GraphTestBase
    {
        [Ignore] // Setup Fiddler autoresponder 
        [TestMethod]
        public async Task ErrorThrottlingError()
        {
            try
            {
                // All requests should have a client-request-id set so that the client can correlate a 
                // request with a response. 
                var headerOptions = new List<HeaderOption>()
                {
                    new HeaderOption("client-request-id", "dddddddd-dddd-dddd-dddd-dddddddddddd")
                };

                // To get a throttling error, I mocked up a 429 response in a text file and turned on the Fiddler
                // autoresponder to return the text file as the response envelope. The autoresponder for this 
                // scenario responds to EXACT:https://graph.microsoft.com/v1.0/groups/036bd54c-c6e5-43eb-b8b5-03e019e75bd1
                var group = await graphClient.Groups["036bd54c-c6e5-43eb-b8b5-03e019e75bd1"].Request(headerOptions).GetAsync();
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                if ((int)e.StatusCode == 429) // Too Many Requests
                {
                    // We have the client-request-id for correlating the response to the request that failed.
                    IEnumerable<string> clientrequestidvalues;
                    Assert.IsTrue(e.ResponseHeaders.TryGetValues("client-request-id", out clientrequestidvalues), "client-request-id not found");

                    // We have the Retry-After that the client can use to wait and resubmit the rejected request.
                    IEnumerable<string> retryaftervalues;
                    Assert.IsTrue(e.ResponseHeaders.TryGetValues("Retry-After", out retryaftervalues), "Retry-After not found");
                }
                else
                {
                    Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
                }
            }
        }
    }
}
