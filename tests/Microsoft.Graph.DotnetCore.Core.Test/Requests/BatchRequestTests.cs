// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Graph.Core.Requests;
    using Microsoft.Graph.DotnetCore.Core.Test.Mocks;
    using Xunit;

    public class BatchRequestTests: RequestTestBase
    {
        [Fact]
        public void BatchRequest()
        {
            // Arrange
            var requestUrl = "https://localhost";

            // Act
            var batchRequest = new BatchRequest(requestUrl, this.baseClient);

            // Assert
            Assert.Empty(batchRequest.QueryOptions);
        }

        [Fact]
        public void BatchRequestWithOptions()
        {
            // Arrange
            var requestUrl = "https://localhost";
            var queryOption = new QueryOption("name", "value");
            List<Option> optionsList = new List<Option> {queryOption};

            // Act
            var batchRequest = new BatchRequest(requestUrl,this.baseClient,optionsList);

            // Assert
            Assert.NotEmpty(batchRequest.QueryOptions);
            Assert.Equal(batchRequest.QueryOptions.First().Name, queryOption.Name);
            Assert.Equal(batchRequest.QueryOptions.First().Value, queryOption.Value);
        }


        [Fact]
        public async Task PostAsyncReturnsBatchResponseContent()
        {
            using (HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.OK))
            using (TestHttpMessageHandler testHttpMessageHandler = new TestHttpMessageHandler())
            {
                /* Arrange */
                // 1. create a mock response
                string requestUrl = "https://localhost/";
                string responseJSON = "{\"responses\":"
                                      + "[{"
                                      + "\"id\": \"1\","
                                      + "\"status\":200,"
                                      + "\"headers\":{\"Cache-Control\":\"no-cache\",\"OData-Version\":\"4.0\",\"Content-Type\":\"application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=false;charset=utf-8\"},"
                                      + "\"body\":{\"@odata.context\":\"https://graph.microsoft.com/v1.0/$metadata#users/$entity\",\"displayName\":\"MOD Administrator\",\"jobTitle\":null,\"id\":\"9f4fe8ea-7e6e-486e-b8f4-VkHdanfIomf\"}"
                                      + "},"
                                      + "{"
                                      + "\"id\": \"2\","
                                      + "\"status\":409,"
                                      + "\"headers\" : {\"Cache-Control\":\"no-cache\"},"
                                      + "\"body\":{\"error\": {\"code\": \"20117\",\"message\": \"An item with this name already exists in this location.\",\"innerError\":{\"request-id\": \"nothing1b13-45cd-new-92be873c5781\",\"date\": \"2019-03-22T23:17:50\"}}}"
                                      + "}]}";
                HttpContent content = new StringContent(responseJSON);
                responseMessage.Content = content;

                // 2. Map the response
                testHttpMessageHandler.AddResponseMapping(requestUrl, responseMessage);
                
                // 3. Create a batch request object to be tested
                MockCustomHttpProvider customHttpProvider = new MockCustomHttpProvider(testHttpMessageHandler);
                BaseClient client = new BaseClient(requestUrl, authenticationProvider.Object, customHttpProvider);
                BatchRequest batchRequest = new BatchRequest(requestUrl, client);

                // 4. Create batch request content to be sent out
                // 4.1 Create HttpRequestMessages for the content
                HttpRequestMessage httpRequestMessage1 = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me/");
                HttpRequestMessage httpRequestMessage2 = new HttpRequestMessage(HttpMethod.Post, "https://graph.microsoft.com/v1.0/me/onenote/notebooks");

                // 4.2 Create batch request steps with request ids.
                BatchRequestStep requestStep1 = new BatchRequestStep("1", httpRequestMessage1);
                BatchRequestStep requestStep2 = new BatchRequestStep("2", httpRequestMessage2, new List<string> { "1" });

                // 4.3 Add batch request steps to BatchRequestContent.
                BatchRequestContent batchRequestContent = new BatchRequestContent(requestStep1,requestStep2);

                /* Act */
                BatchResponseContent returnedResponse = await batchRequest.PostAsync(batchRequestContent);
                HttpResponseMessage firstResponse = await returnedResponse.GetResponseByIdAsync("1");
                HttpResponseMessage secondResponse = await returnedResponse.GetResponseByIdAsync("2");

                /* Assert */
                // validate the first response
                Assert.NotNull(firstResponse);
                Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
                Assert.True(firstResponse.Headers.CacheControl.NoCache);
                Assert.NotNull(firstResponse.Content);

                // validate the second response
                Assert.NotNull(secondResponse);
                Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
                Assert.True(secondResponse.Headers.CacheControl.NoCache);
                Assert.NotNull(secondResponse.Content);

            }
        }
    }
}