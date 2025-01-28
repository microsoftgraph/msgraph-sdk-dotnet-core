﻿// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

#pragma warning disable CS0618 // Type or member is obsolete Guidance is to use the BatchRequestContentCollection for making batch requests
namespace Microsoft.Graph.DotnetCore.Core.Test.Requests.Content
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.Graph.DotnetCore.Core.Test.Mocks;
    using Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels;
    using Microsoft.Kiota.Abstractions;
    using Microsoft.Kiota.Serialization.Json;
    using Microsoft.Kiota.Serialization.Text;
    using Xunit;
    using HttpMethod = System.Net.Http.HttpMethod;

    public class BatchRequestContentTests
    {
        private const string REQUEST_URL = "https://graph.microsoft.com/v1.0/me";
        private readonly IBaseClient client = new BaseClient(REQUEST_URL, new MockAuthenticationProvider().Object);

        private readonly Regex whitespacePattern = new Regex("\\s");

        public BatchRequestContentTests()
        {
            ApiClientBuilder.RegisterDefaultDeserializer<JsonParseNodeFactory>();
            ApiClientBuilder.RegisterDefaultDeserializer<TextParseNodeFactory>();
            ApiClientBuilder.RegisterDefaultSerializer<JsonSerializationWriterFactory>();
            ApiClientBuilder.RegisterDefaultSerializer<TextSerializationWriterFactory>();
        }

        [Fact]
        public void BatchRequestContent_DefaultInitialize()
        {
            BatchRequestContent batchRequestContent = new BatchRequestContent(client);

            Assert.NotNull(batchRequestContent.BatchRequestSteps);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(0));
        }

        [Fact]
        public void BatchRequestContent_InitializeWithSerializer()
        {
            List<BatchRequestStep> requestSteps = new List<BatchRequestStep>();
            for (int i = 0; i < 5; i++)
            {
                requestSteps.Add(new BatchRequestStep(i.ToString(), new HttpRequestMessage(HttpMethod.Get, REQUEST_URL)));
            }

            BatchRequestContent batchRequestContent = new BatchRequestContent(client, requestSteps.ToArray());

            Assert.NotNull(batchRequestContent.BatchRequestSteps);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(5));
        }

        [Fact]
        public void BatchRequestContent_InitializeWithBatchRequestSteps()
        {
            List<BatchRequestStep> requestSteps = new List<BatchRequestStep>();
            for (int i = 0; i < 5; i++)
            {
                requestSteps.Add(new BatchRequestStep(i.ToString(), new HttpRequestMessage(HttpMethod.Get, REQUEST_URL)));
            }

            BatchRequestContent batchRequestContent = new BatchRequestContent(client, requestSteps.ToArray());

            Assert.NotNull(batchRequestContent.BatchRequestSteps);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(5));
        }

        [Fact]
        public void BatchRequestContent_InitializeWithInvalidDependsOnIds()
        {
            BatchRequestStep batchRequestStep1 = new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
            BatchRequestStep batchRequestStep2 = new BatchRequestStep("2", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL), new List<string> { "3" });

            ArgumentException ex = Assert.Throws<ArgumentException>(() => new BatchRequestContent(client, batchRequestStep1, batchRequestStep2));

            Assert.Equal(ErrorConstants.Messages.InvalidDependsOnRequestId, ex.Message);
        }

        [Fact]
        public void BatchRequestContent_AddBatchRequestStepWithNewRequestStep()
        {
            // Arrange
            BatchRequestStep batchRequestStep = new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
            BatchRequestContent batchRequestContent = new BatchRequestContent(client);

            // Act
            Assert.False(batchRequestContent.BatchRequestSteps.Any());//Its empty
            bool isSuccess = batchRequestContent.AddBatchRequestStep(batchRequestStep);

            // Assert
            Assert.True(isSuccess);
            Assert.NotNull(batchRequestContent.BatchRequestSteps);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(1));
        }

        [Fact]
        public void BatchRequestContent_AddBatchRequestStepToBatchRequestContentWithMaxSteps()
        {
            // Arrange
            BatchRequestContent batchRequestContent = new BatchRequestContent(client);
            //Add MaxNumberOfRequests number of steps
            for (var i = 0; i < CoreConstants.BatchRequest.MaxNumberOfRequests; i++)
            {
                BatchRequestStep batchRequestStep = new BatchRequestStep(i.ToString(), new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
                bool isSuccess = batchRequestContent.AddBatchRequestStep(batchRequestStep);
                Assert.True(isSuccess);//Assert we can add steps up to the max
            }

            // Act
            BatchRequestStep extraBatchRequestStep = new BatchRequestStep("failing_id", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
            bool result = batchRequestContent.AddBatchRequestStep(extraBatchRequestStep);

            // Assert
            Assert.False(result);//Assert we did not add any more steps
            Assert.NotNull(batchRequestContent.BatchRequestSteps);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(CoreConstants.BatchRequest.MaxNumberOfRequests));
        }

        [Fact]
        public void BatchRequestContent_AddBatchRequestStepWithExistingRequestStep()
        {
            BatchRequestStep batchRequestStep = new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
            BatchRequestContent batchRequestContent = new BatchRequestContent(client, batchRequestStep);
            bool isSuccess = batchRequestContent.AddBatchRequestStep(batchRequestStep);

            Assert.False(isSuccess);
            Assert.NotNull(batchRequestContent.BatchRequestSteps);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(1));
        }

        [Fact]
        public void BatchRequestContent_AddBatchRequestStepWithNullRequestStep()
        {
            BatchRequestStep batchRequestStep = new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
            BatchRequestContent batchRequestContent = new BatchRequestContent(client, batchRequestStep);

            bool isSuccess = batchRequestContent.AddBatchRequestStep(batchRequestStep: null);

            Assert.False(isSuccess);
            Assert.NotNull(batchRequestContent.BatchRequestSteps);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(1));
        }

        [Fact]
        public void BatchRequestContent_RemoveBatchRequestStepWithIdForExistingId()
        {
            BatchRequestStep batchRequestStep1 = new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
            BatchRequestStep batchRequestStep2 = new BatchRequestStep("2", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL), new List<string> { "1" });

            BatchRequestContent batchRequestContent = new BatchRequestContent(client, batchRequestStep1, batchRequestStep2);

            bool isSuccess = batchRequestContent.RemoveBatchRequestStepWithId("1");

            Assert.True(isSuccess);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(1));
            Assert.True(batchRequestContent.BatchRequestSteps["2"].DependsOn.Count.Equals(0));
        }

        [Fact]
        public void BatchRequestContent_RemoveBatchRequestStepWithIdForNonExistingId()
        {
            BatchRequestStep batchRequestStep1 = new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
            BatchRequestStep batchRequestStep2 = new BatchRequestStep("2", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL), new List<string> { "1" });

            BatchRequestContent batchRequestContent = new BatchRequestContent(client, batchRequestStep1, batchRequestStep2);

            bool isSuccess = batchRequestContent.RemoveBatchRequestStepWithId("5");

            Assert.False(isSuccess);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(2));
            Assert.Same(batchRequestStep2.DependsOn[0], batchRequestContent.BatchRequestSteps["2"].DependsOn[0]);
        }

        [Fact]
        public async Task BatchRequestContent_NewBatchWithFailedRequestsAsync()
        {
            BatchRequestContentCollection batchRequestContent = new BatchRequestContentCollection(client);
            var requestIds = new List<string>();
            for (int i = 0; i < 50; i++)
            {
                var requestId = await batchRequestContent.AddBatchRequestStepAsync(new RequestInformation()
                {
                    HttpMethod = Method.DELETE,
                    UrlTemplate = REQUEST_URL
                });
                requestIds.Add(requestId);
            }

            batchRequestContent.GetBatchRequestsForExecution();// this is called when request is executed

            Dictionary<string, HttpStatusCode> responseStatusCodes = requestIds.ToDictionary(requestId => requestId, requestId => HttpStatusCode.OK);

            var retryBatch = batchRequestContent.NewBatchWithFailedRequests(responseStatusCodes);

            Assert.Empty(retryBatch.BatchRequestSteps);
        }

        [Fact]
        public async Task BatchRequestContent_NewBatchWithFailedRequests2Async()
        {
            BatchRequestContentCollection batchRequestContent = new BatchRequestContentCollection(client);
            var requestIds = new List<string>();
            for (int i = 0; i < 50; i++)
            {
                var requestId = await batchRequestContent.AddBatchRequestStepAsync(new RequestInformation()
                {
                    HttpMethod = Method.DELETE,
                    UrlTemplate = REQUEST_URL
                });
                requestIds.Add(requestId);
            }

            Dictionary<string, HttpStatusCode> responseStatusCodes = requestIds.ToDictionary(requestId => requestId, requestId => HttpStatusCode.OK);

            var retryBatch = batchRequestContent.NewBatchWithFailedRequests(responseStatusCodes);

            Assert.Empty(retryBatch.BatchRequestSteps);// All requests were succesfful
        }

        [Fact]
        public async Task BatchRequestContent_NewBatchWithFailedRequestsWithBodyAsync()
        {
            BatchRequestContentCollection batchRequestContent = new BatchRequestContentCollection(client);
            var requestIds = new List<string>();

            // Add the first request
            var postRequestInformation = new RequestInformation
            {
                HttpMethod = Method.POST,
                UrlTemplate = REQUEST_URL
            };
            postRequestInformation.SetContentFromParsable(client.RequestAdapter, "application/json", new TestDrive
            {
                Name = "testDrive"
            });
            var postRequestId = await batchRequestContent.AddBatchRequestStepAsync(postRequestInformation);
            requestIds.Add(postRequestId);

            // Add the second request with plain text
            var postRequestInformation2 = new RequestInformation
            {
                HttpMethod = Method.POST,
                UrlTemplate = REQUEST_URL
            };
            postRequestInformation2.SetContentFromScalar(client.RequestAdapter, "text/plain", "This is a test");
            var postRequestId2 = await batchRequestContent.AddBatchRequestStepAsync(postRequestInformation2);
            requestIds.Add(postRequestId2);

            // 1. Simulate the first time building the request and serializing it.
            var batchRequestContents = batchRequestContent.GetBatchRequestsForExecution();
            var stringContentFirst = await batchRequestContents.First().ReadAsStringAsync();

            // Assert the body is present
            Assert.Contains("\"body\":{\"@odata.type\":\"microsoft.graph.drive\",\"name\":\"testDrive\"}", stringContentFirst);
            Assert.Contains(Convert.ToBase64String(Encoding.UTF8.GetBytes("This is a test")), stringContentFirst);
            JsonDocument.Parse(stringContentFirst);// verify its valid json otherwise it will throw

            Dictionary<string, HttpStatusCode> responseStatusCodes = requestIds.ToDictionary(requestId => requestId, requestId => HttpStatusCode.BadGateway);
            var retryBatch = batchRequestContent.NewBatchWithFailedRequests(responseStatusCodes);

            // 2. Failed request is present
            Assert.NotEmpty(retryBatch.BatchRequestSteps);

            batchRequestContents = retryBatch.GetBatchRequestsForExecution();
            var retryStringContentFirst = await batchRequestContents.First().ReadAsStringAsync();

            // Assert the body is still present
            Assert.Contains("\"body\":{\"@odata.type\":\"microsoft.graph.drive\",\"name\":\"testDrive\"}", retryStringContentFirst);
            Assert.Contains(Convert.ToBase64String(Encoding.UTF8.GetBytes("This is a test")), retryStringContentFirst);
            JsonDocument.Parse(retryStringContentFirst);// verify its valid json otherwise it will throw
        }


        [Fact]
        public async System.Threading.Tasks.Task BatchRequestContent_GetBatchRequestContentFromStepAsync()
        {
            BatchRequestStep batchRequestStep1 = new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
            BatchRequestStep batchRequestStep2 = new BatchRequestStep("2", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL), new List<string> { "1" });

            BatchRequestContent batchRequestContent = new BatchRequestContent(client);
            batchRequestContent.AddBatchRequestStep(batchRequestStep1);
            batchRequestContent.AddBatchRequestStep(batchRequestStep2);

            batchRequestContent.RemoveBatchRequestStepWithId("1");

            string requestContent;
            // We get the contents of the stream as string for comparison.
            using (Stream requestStream = await batchRequestContent.GetBatchRequestContentAsync())
            using (StreamReader reader = new StreamReader(requestStream))
            {
                requestContent = await reader.ReadToEndAsync();
            }

            string expectedContent = "{\"requests\":[{\"id\":\"2\",\"url\":\"/me\",\"method\":\"GET\"}]}";

            Assert.NotNull(requestContent);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(1));
            Assert.Equal(expectedContent, requestContent);
        }

        [Fact]
        public async System.Threading.Tasks.Task BatchRequestContent_GetBatchRequestContentFromRequestInformationDoesNotAddAuthHeaderAsync()
        {
            BatchRequestContent batchRequestContent = new BatchRequestContent(client);
            RequestInformation requestInformation = new RequestInformation() { HttpMethod = Method.GET, UrlTemplate = REQUEST_URL };
            await batchRequestContent.AddBatchRequestStepAsync(requestInformation, "2");

            string requestContent;
            // We get the contents of the stream as string for comparison.
            using (Stream requestStream = await batchRequestContent.GetBatchRequestContentAsync())
            using (StreamReader reader = new StreamReader(requestStream))
            {
                requestContent = await reader.ReadToEndAsync();
            }

            string expectedContent = "{\"requests\":[{\"id\":\"2\",\"url\":\"/me\",\"method\":\"GET\"}]}"; //Auth Header Absent.

            Assert.NotNull(requestContent);
            Assert.IsType<BaseGraphRequestAdapter>(batchRequestContent.RequestAdapter);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(1));
            Assert.Equal(expectedContent, requestContent);
        }

        [Fact]
        public async System.Threading.Tasks.Task BatchRequestContent_GetBatchRequestContentSupportsNonJsonPayloadAsync()
        {
            using var fileStream = File.Open("ms-logo.png", FileMode.Open);
            BatchRequestStep batchRequestStep1 = new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
            HttpRequestMessage createImageMessage = new HttpRequestMessage(HttpMethod.Post, REQUEST_URL)
            {
                Content = new StreamContent(fileStream)
            };
            createImageMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

            BatchRequestStep batchRequestStep2 = new BatchRequestStep("2", createImageMessage, new List<string> { "1" });

            BatchRequestContent batchRequestContent = new BatchRequestContent(client);
            batchRequestContent.AddBatchRequestStep(batchRequestStep1);
            batchRequestContent.AddBatchRequestStep(batchRequestStep2);

            string requestContent;
            // we do this to get a version of the json that is indented
            using (Stream requestStream = await batchRequestContent.GetBatchRequestContentAsync())
            using (JsonDocument jsonDocument = await JsonDocument.ParseAsync(requestStream))
            {
                requestContent = JsonSerializer.Serialize(jsonDocument.RootElement, new JsonSerializerOptions() { WriteIndented = true });
            }

            string expectedJson = "{\r\n" +
                                  "  \"requests\": [\r\n" +
                                  "    {\r\n" +
                                  "      \"id\": \"1\",\r\n" +
                                  "      \"url\": \"/me\",\r\n" +
                                  "      \"method\": \"GET\"\r\n" +
                                  "    },\r\n" +
                                  "    {\r\n" +
                                  "      \"id\": \"2\",\r\n" +
                                  "      \"url\": \"/me\",\r\n" +
                                  "      \"method\": \"POST\",\r\n" +
                                  "      \"dependsOn\": [\r\n" +
                                  "        \"1\"\r\n" +
                                  "      ],\r\n" +
                                  "      \"headers\": {\r\n" +
                                  "        \"Content-Type\": \"image/png\"\r\n" +
                                  "      },\r\n" +
                                  "      \"body\": \"iVBORw0KGgoAAAANSUhEUgAAAFAAAABQCAYAAACOEfKtAAAACXBIWXMAAA7EAAAOxAGVKw4bAAABO0lEQVR42u3bMWoCQRSA4X8Xq0UE8QZ2scwhYusBchfBs3gA2\\u002BQKaSwsUnmFQALDkmLWYlTGwiqQvMD/Vys2j09msXgDZn9Zc3n4XM0BxoFmS5PdMQOsX2mBLshceYC0eSofRtUXY\\u002BArEOAD8H5\\u002BfgTeovywDcyAHqD1EP4sAQUUUEABTUABBRTQBBRQQAFNQAEFFNAEFFBAAU1AAQUU0AQUUEABTUABBRTwt8r/BbBe8U2UtdooHavnQ6DZMvDt2bMYXa85sP1oKdvwUTrwPO0Bhhc6YBHoCO\\u002BbZXlP1\\u002B/AjjhXCeD2msMi0GwJrzn4P1BAAU1AAQUUUEAJBBRQQAFNQAEFFNAEFFBAAU1AAQUU0AQUUEABTUABBRTQ7ldvqGbK9mWUctDZ0j3Ay\\u002BpqlOqrBPtgs/WePYvRCfKZLRIUo/e5AAAAAElFTkSuQmCC\"\r\n" +
                                  "    }\r\n" +
                                  "  ]\r\n" +
                                  "}";
            expectedJson = whitespacePattern.Replace(expectedJson, "");

            Assert.NotNull(requestContent);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(2));
            Assert.Equal(expectedJson, whitespacePattern.Replace(requestContent, ""));
        }

        [Fact]
        public async System.Threading.Tasks.Task BatchRequestContent_GetBatchRequestContentFromStepAsyncDoesNotModifyDateTimesAsync()
        {
            // System.Text.Json is strict on json content by default. So make sure that there are no
            // trailing comma's and special characters
            var payloadString = "{\r\n" +
                                "  \"subject\": \"Lets go for lunch\",\r\n" +
                                "  \"body\": {\r\n    \"contentType\": \"HTML\",\r\n" +
                                "    \"content\": \"Does mid month work for you?\"\r\n" +
                                "  },\r\n" +
                                "  \"start\": {\r\n" +
                                "      \"dateTime\": \"2019-03-15T12:00:00.0000\",\r\n" +
                                "      \"timeZone\": \"Pacific Standard Time\"\r\n" +
                                "  },\r\n" +
                                "  \"end\": {\r\n" +
                                "      \"dateTime\": \"2019-03-15T14:00:00.0000\",\r\n" +
                                "      \"timeZone\": \"Pacific Standard Time\"\r\n" +
                                "  },\r\n  \"location\":{\r\n" +
                                "      \"displayName\":\"Harrys Bar\"\r\n" +
                                "  },\r\n" +
                                "  \"attendees\": [\r\n" +
                                "    {\r\n" +
                                "      \"emailAddress\": {\r\n" +
                                "        \"address\":\"adelev@contoso.onmicrosoft.com\",\r\n" +
                                "        \"name\": \"Adele Vance\"\r\n" +
                                "      },\r\n" +
                                "      \"type\": \"required\"\r\n" +
                                "    }\r\n" +
                                "  ]\r\n" +
                                "}";

            BatchRequestStep batchRequestStep1 = new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
            HttpRequestMessage createEventMessage = new HttpRequestMessage(HttpMethod.Post, REQUEST_URL)
            {
                Content = new StringContent(payloadString, Encoding.UTF8, "application/json")
            };
            BatchRequestStep batchRequestStep2 = new BatchRequestStep("2", createEventMessage, new List<string> { "1" });

            BatchRequestContent batchRequestContent = new BatchRequestContent(client);
            batchRequestContent.AddBatchRequestStep(batchRequestStep1);
            batchRequestContent.AddBatchRequestStep(batchRequestStep2);

            string requestContent;
            // we do this to get a version of the json that is indented
            using (Stream requestStream = await batchRequestContent.GetBatchRequestContentAsync())
            using (JsonDocument jsonDocument = await JsonDocument.ParseAsync(requestStream))
            {
                requestContent = JsonSerializer.Serialize(jsonDocument.RootElement, new JsonSerializerOptions() { WriteIndented = true });
            }

            string expectedJson = "{\r\n" +
                                  "  \"requests\": [\r\n" +
                                  "    {\r\n" +
                                  "      \"id\": \"1\",\r\n" +
                                  "      \"url\": \"/me\",\r\n" +
                                  "      \"method\": \"GET\"\r\n" +
                                  "    },\r\n" +
                                  "    {\r\n" +
                                  "      \"id\": \"2\",\r\n" +
                                  "      \"url\": \"/me\",\r\n" +
                                  "      \"method\": \"POST\",\r\n" +
                                  "      \"dependsOn\": [\r\n" +
                                  "        \"1\"\r\n" +
                                  "      ],\r\n" +
                                  "      \"headers\": {\r\n" +
                                  "        \"Content-Type\": \"application/json; charset=utf-8\"\r\n" +
                                  "      },\r\n" +
                                  "      \"body\": {\r\n" +
                                  "        \"subject\": \"Lets go for lunch\",\r\n" +
                                  "        \"body\": {\r\n" +
                                  "          \"contentType\": \"HTML\",\r\n" +
                                  "          \"content\": \"Does mid month work for you?\"\r\n" +
                                  "        },\r\n" +
                                  "        \"start\": {\r\n" +
                                  "          \"dateTime\": \"2019-03-15T12:00:00.0000\",\r\n" +
                                  "          \"timeZone\": \"Pacific Standard Time\"\r\n" +
                                  "        },\r\n" +
                                  "        \"end\": {\r\n" +
                                  "          \"dateTime\": \"2019-03-15T14:00:00.0000\",\r\n" +
                                  "          \"timeZone\": \"Pacific Standard Time\"\r\n" +
                                  "        },\r\n" +
                                  "        \"location\": {\r\n" +
                                  "          \"displayName\": \"Harrys Bar\"\r\n" +
                                  "        },\r\n" +
                                  "        \"attendees\": [\r\n" +
                                  "          {\r\n" +
                                  "            \"emailAddress\": {\r\n" +
                                  "              \"address\": \"adelev@contoso.onmicrosoft.com\",\r\n" +
                                  "              \"name\": \"Adele Vance\"\r\n" +
                                  "            },\r\n" +
                                  "            \"type\": \"required\"\r\n" +
                                  "          }\r\n" +
                                  "        ]\r\n" +
                                  "      }\r\n" +
                                  "    }\r\n" +
                                  "  ]\r\n" +
                                  "}";
            expectedJson = whitespacePattern.Replace(expectedJson, "");

            Assert.NotNull(requestContent);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(2));
            Assert.Equal(expectedJson, whitespacePattern.Replace(requestContent, ""));
        }

        [Fact]
        public async Task BatchRequest_GetBathRequestContentAsyncRetainsInsertionOrderAsync()
        {
            BatchRequestStep batchRequestStep1 = new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
            BatchRequestStep batchRequestStep2 = new BatchRequestStep(Guid.NewGuid().ToString(), new HttpRequestMessage(HttpMethod.Get, REQUEST_URL), new List<string> { "1" });
            BatchRequestStep batchRequestStep3 = new BatchRequestStep(Guid.NewGuid().ToString(), new HttpRequestMessage(HttpMethod.Post, REQUEST_URL));
            BatchRequestStep batchRequestStep4 = new BatchRequestStep("5", new HttpRequestMessage(HttpMethod.Put, REQUEST_URL));

            BatchRequestContent batchRequestContent = new BatchRequestContent(client, batchRequestStep1, batchRequestStep2, batchRequestStep3, batchRequestStep4);

            // We get the contents of the stream as string for comparison.
            using (Stream requestStream = await batchRequestContent.GetBatchRequestContentAsync())
            {
                // Parse requestStream to JSON object and check the "requests" array
                using (JsonDocument jsonDocument = await JsonDocument.ParseAsync(requestStream))
                {
                    var requests = jsonDocument.RootElement.GetProperty("requests");
                    Assert.True(requests.GetArrayLength().Equals(4));
                    Assert.Equal(batchRequestStep1.RequestId, requests[0].GetProperty("id").GetString());
                    Assert.Equal(batchRequestStep2.RequestId, requests[1].GetProperty("id").GetString());
                    Assert.Equal(batchRequestStep3.RequestId, requests[2].GetProperty("id").GetString());
                    Assert.Equal(batchRequestStep4.RequestId, requests[3].GetProperty("id").GetString());
                }
            }
        }

        [Fact]
        public void BatchRequestContent_AddBatchRequestStepWithHttpRequestMessage()
        {
            // Arrange
            BatchRequestContent batchRequestContent = new BatchRequestContent(client);
            Assert.False(batchRequestContent.BatchRequestSteps.Any());//Its empty

            // Act
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, REQUEST_URL);
            string batchRequestStepId = batchRequestContent.AddBatchRequestStep(httpRequestMessage);

            // Assert we added successfully and contents are as expected
            Assert.NotNull(batchRequestStepId);
            Assert.NotNull(batchRequestContent.BatchRequestSteps);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(1));
            Assert.Equal(batchRequestContent.BatchRequestSteps.First().Value.Request.RequestUri.AbsoluteUri, httpRequestMessage.RequestUri.AbsoluteUri);
            Assert.Equal(batchRequestContent.BatchRequestSteps.First().Value.Request.Method.Method, httpRequestMessage.Method.Method);
        }

        [Fact]
        public void BatchRequestContent_AddBatchRequestStepWithHttpRequestMessageToBatchRequestContentWithMaxSteps()
        {
            // Arrange
            BatchRequestContent batchRequestContent = new BatchRequestContent(client);
            // Add MaxNumberOfRequests number of steps
            for (var i = 0; i < CoreConstants.BatchRequest.MaxNumberOfRequests; i++)
            {
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, REQUEST_URL);
                string batchRequestStepId = batchRequestContent.AddBatchRequestStep(httpRequestMessage);
                Assert.NotNull(batchRequestStepId);
                Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(i + 1));//Assert we can add steps up to the max
            }

            // Act
            HttpRequestMessage extraHttpRequestMessage = new HttpRequestMessage(HttpMethod.Get, REQUEST_URL);

            // Assert
            var exception = Assert.Throws<ArgumentException>(() => batchRequestContent.AddBatchRequestStep(extraHttpRequestMessage));//Assert we throw exception on excess add
            Assert.Equal(string.Format(ErrorConstants.Messages.MaximumValueExceeded, "Number of batch request steps", CoreConstants.BatchRequest.MaxNumberOfRequests), exception.Message);
            Assert.NotNull(batchRequestContent.BatchRequestSteps);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(CoreConstants.BatchRequest.MaxNumberOfRequests));
        }

        [Fact]
        public async Task BatchRequestContent_AddBatchRequestStepWithBaseRequestAsync()
        {
            // Arrange
            RequestInformation requestInformation = new RequestInformation() { HttpMethod = Method.GET, UrlTemplate = REQUEST_URL };
            BatchRequestContent batchRequestContent = new BatchRequestContent(client);
            Assert.False(batchRequestContent.BatchRequestSteps.Any());//Its empty

            // Act
            string batchRequestStepId = await batchRequestContent.AddBatchRequestStepAsync(requestInformation);

            // Assert we added successfully and contents are as expected
            Assert.NotNull(batchRequestStepId);
            Assert.NotNull(batchRequestContent.BatchRequestSteps);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(1));
            Assert.Equal(batchRequestContent.BatchRequestSteps.First().Value.Request.RequestUri.OriginalString, requestInformation.URI.OriginalString);
            Assert.Equal(batchRequestContent.BatchRequestSteps.First().Value.Request.Method.Method, requestInformation.HttpMethod.ToString());
        }

        [Fact]
        public async Task BatchRequestContent_AddBatchRequestStepWithBaseRequestWithHeaderOptionsAsync()
        {
            // Create a BatchRequestContent from a BaseRequest object
            BatchRequestContent batchRequestContent = new BatchRequestContent(client);

            // Create a BatchRequestContent from a HttpRequestMessage object
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, REQUEST_URL)
            {
                Content = new StringContent("{}")
            };
            requestMessage.Headers.Add("ConsistencyLevel", "eventual");
            requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(CoreConstants.MimeTypeNames.Application.Json);
            string batchRequestStepId = batchRequestContent.AddBatchRequestStep(requestMessage);

            // Assert we added successfully and contents are as expected
            Assert.NotNull(batchRequestContent.BatchRequestSteps);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(1));
            Assert.True(batchRequestContent.BatchRequestSteps[batchRequestStepId].Request.Headers.Any());
            Assert.True(batchRequestContent.BatchRequestSteps[batchRequestStepId].Request.Content.Headers.Any());

            // we do this to get a version of the json payload that is indented
            await using var requestStream = await batchRequestContent.GetBatchRequestContentAsync();
            using var jsonDocument = await JsonDocument.ParseAsync(requestStream);
            string requestContentString = JsonSerializer.Serialize(jsonDocument.RootElement, new JsonSerializerOptions() { WriteIndented = true });

            // Ensure the headers section is added
            string expectedJsonSection = "      \"url\": \"/me\",\r\n" +
                                         "      \"method\": \"POST\",\r\n" +
                                         "      \"headers\": {\r\n" +
                                         "        \"ConsistencyLevel\": \"eventual\",\r\n" + // Ensure the requestMessage headers are present
                                         "        \"Content-Type\": \"application/json\"\r\n" + // Ensure the content headers are present
                                         "      }";
            expectedJsonSection = whitespacePattern.Replace(expectedJsonSection, "");
            Assert.Contains(expectedJsonSection, whitespacePattern.Replace(requestContentString, ""));
        }

        [Fact]
        public async Task BatchRequestContent_AddBatchRequestStepWithBaseRequestToBatchRequestContentWithMaxStepsAsync()
        {
            // Arrange
            BatchRequestContent batchRequestContent = new BatchRequestContent(client);
            // Add MaxNumberOfRequests number of steps
            for (var i = 0; i < CoreConstants.BatchRequest.MaxNumberOfRequests; i++)
            {
                RequestInformation requestInformation = new RequestInformation() { HttpMethod = Method.GET, UrlTemplate = REQUEST_URL };
                string batchRequestStepId = await batchRequestContent.AddBatchRequestStepAsync(requestInformation);
                Assert.NotNull(batchRequestStepId);
                Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(i + 1));//Assert we can add steps up to the max
            }

            // Act
            RequestInformation extraRequestInformation = new RequestInformation() { HttpMethod = Method.GET, UrlTemplate = REQUEST_URL };
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => batchRequestContent.AddBatchRequestStepAsync(extraRequestInformation));

            // Assert
            Assert.Equal(string.Format(ErrorConstants.Messages.MaximumValueExceeded, "Number of batch request steps", CoreConstants.BatchRequest.MaxNumberOfRequests), exception.Message);
            Assert.NotNull(batchRequestContent.BatchRequestSteps);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(CoreConstants.BatchRequest.MaxNumberOfRequests));
        }

        [Theory]
        [InlineData("https://graph.microsoft.com/v1.0/me", "/me")]
        [InlineData("https://graph.microsoft.com/beta/me", "/me")]
        [InlineData("https://graph.microsoft.com/v1.0/users/abcbeta123@wonderemail.com/events", "/users/abcbeta123@wonderemail.com/events")]
        [InlineData("https://graph.microsoft.com/beta/users/abcbeta123@wonderemail.com/events", "/users/abcbeta123@wonderemail.com/events")]
        [InlineData("https://graph.microsoft.com/v1.0/users?$filter=identities/any(id:id/issuer%20eq%20'$74707853-18b3-411f-ad57-2ef65f6fdeb0'%20and%20id/issuerAssignedId%20eq%20'**bobbetancourt@fakeemail.com**')", "/users?$filter=identities/any(id:id/issuer eq '$74707853-18b3-411f-ad57-2ef65f6fdeb0' and id/issuerAssignedId eq '**bobbetancourt@fakeemail.com**')")]
        [InlineData("https://graph.microsoft.com/beta/users?$filter=identities/any(id:id/issuer%20eq%20'$74707853-18b3-411f-ad57-2ef65f6fdeb0'%20and%20id/issuerAssignedId%20eq%20'**bobbetancourt@fakeemail.com**')&$top=1", "/users?$filter=identities/any(id:id/issuer eq '$74707853-18b3-411f-ad57-2ef65f6fdeb0' and id/issuerAssignedId eq '**bobbetancourt@fakeemail.com**')&$top=1")]
        public async Task BatchRequestContent_AddBatchRequestStepWithBaseRequestProperlySetsVersionAsync(string requestUrl, string expectedUrl)
        {
            // Arrange
            BatchRequestStep batchRequestStep = new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, requestUrl));
            BatchRequestContent batchRequestContent = new BatchRequestContent(client);
            Assert.False(batchRequestContent.BatchRequestSteps.Any());//Its empty

            // Act
            batchRequestContent.AddBatchRequestStep(batchRequestStep);
            var requestContentStream = await batchRequestContent.GetBatchRequestContentAsync();
            string requestContent;
            using (StreamReader reader = new StreamReader(requestContentStream))
            {
                requestContent = await reader.ReadToEndAsync();
            }

            var expectedContent = "{\"requests\":[{\"id\":\"1\",\"url\":\"" + expectedUrl + "\",\"method\":\"GET\"}]}";

            // Assert we added successfully and contents are as expected and URI is not encoded
            Assert.Equal(expectedContent, System.Text.RegularExpressions.Regex.Unescape(requestContent));
        }

        [Theory]
        [InlineData("https://graph.microsoft.com/v1.0/drives/b%21ynG/items/74707853-18b3-411f-ad57-2ef65f6fdeb0:/test.txt:/textfilecontentbytes", "/drives/b!ynG/items/74707853-18b3-411f-ad57-2ef65f6fdeb0:/test.txt:/textfilecontentbytes")]
        public async Task BatchRequestContent_AddBatchRequestPutStepWithBaseRequestProperlyEncodedURIAsync(string requestUrl, string expectedUrl)
        {
            // Arrange
            BatchRequestStep batchRequestStep = new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Put, requestUrl));
            BatchRequestContent batchRequestContent = new BatchRequestContent(client);
            Assert.False(batchRequestContent.BatchRequestSteps.Any());//Its empty

            // Act
            batchRequestContent.AddBatchRequestStep(batchRequestStep);
            var requestContentStream = await batchRequestContent.GetBatchRequestContentAsync();
            string requestContent;
            using (StreamReader reader = new StreamReader(requestContentStream))
            {
                requestContent = await reader.ReadToEndAsync();
            }

            var expectedContent = "{\"requests\":[{\"id\":\"1\",\"url\":\"" + expectedUrl + "\",\"method\":\"PUT\"}]}";

            // Assert we added successfully and contents are as expected and URI is not encoded
            Assert.Equal(expectedContent, System.Text.RegularExpressions.Regex.Unescape(requestContent));
        }
    }
}
