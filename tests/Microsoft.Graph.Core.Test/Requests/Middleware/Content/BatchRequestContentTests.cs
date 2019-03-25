// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests.Content
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;

    [TestClass]
    public class BatchRequestContentTests
    {
        private const string REQUEST_URL = "https://graph.microsoft.com/v1.0/me";
        [TestMethod]
        public void BatchRequestContent_DefaultInitialize()
        {
            BatchRequestContent batchRequestContent = new BatchRequestContent();

            Assert.IsNotNull(batchRequestContent.BatchRequestSteps, "BatchRequestSteps not initialized.");
            Assert.IsTrue(batchRequestContent.BatchRequestSteps.Count.Equals(0), "Unexpected batchRequestSteps set.");
        }

        [TestMethod]
        public void BatchRequestContent_InitializeWithBatchRequestSteps()
        {
            List<BatchRequestStep> requestSteps = new List<BatchRequestStep>();
            for (int i = 0; i < 5; i++)
            {
                requestSteps.Add(new BatchRequestStep(i.ToString(), new HttpRequestMessage(HttpMethod.Get, REQUEST_URL)));
            }

            BatchRequestContent batchRequestContent = new BatchRequestContent(requestSteps);

            Assert.IsNotNull(batchRequestContent.BatchRequestSteps, "BatchRequestSteps not initialized.");
            Assert.IsTrue(batchRequestContent.BatchRequestSteps.Count.Equals(5), "Unexpected batchRequestSteps set.");
        }

        [TestMethod]
        public void BatchRequestContent_AddBatchRequestStepWithNewRequestStep()
        {
            BatchRequestStep batchRequestStep = new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
            BatchRequestContent batchRequestContent = new BatchRequestContent();
            bool isSuccess = batchRequestContent.AddBatchRequestStep(batchRequestStep);

            Assert.IsTrue(isSuccess, "Failed to add batch request step.");
            Assert.IsNotNull(batchRequestContent.BatchRequestSteps, "BatchRequestSteps not initialized.");
            Assert.IsTrue(batchRequestContent.BatchRequestSteps.Count.Equals(1), "Unexpected batchRequestSteps set.");
        }

        [TestMethod]
        public void BatchRequestContent_AddBatchRequestStepWithExistingRequestStep()
        {
            BatchRequestStep batchRequestStep = new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
            BatchRequestContent batchRequestContent = new BatchRequestContent(new List<BatchRequestStep> { batchRequestStep });
            bool isSuccess = batchRequestContent.AddBatchRequestStep(batchRequestStep);

            Assert.IsFalse(isSuccess, "Failed to add batch request step.");
            Assert.IsNotNull(batchRequestContent.BatchRequestSteps, "BatchRequestSteps not initialized.");
            Assert.IsTrue(batchRequestContent.BatchRequestSteps.Count.Equals(1), "Unexpected batchRequestSteps set.");
        }

        [TestMethod]
        public void BatchRequestContent_AddBatchRequestStepWithNullRequestStep()
        {
            BatchRequestStep batchRequestStep = new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
            BatchRequestContent batchRequestContent = new BatchRequestContent(new List<BatchRequestStep> { batchRequestStep });

            bool isSuccess = batchRequestContent.AddBatchRequestStep(null);

            Assert.IsFalse(isSuccess, "Failed to add batch request step.");
            Assert.IsNotNull(batchRequestContent.BatchRequestSteps, "BatchRequestSteps not initialized.");
            Assert.IsTrue(batchRequestContent.BatchRequestSteps.Count.Equals(1), "Unexpected batchRequestSteps set.");
        }

        [TestMethod]
        public void BatchRequestContent_RemoveBatchRequestStepWithIdForExistingId()
        {
            BatchRequestStep batchRequestStep1 = new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
            BatchRequestStep batchRequestStep2 = new BatchRequestStep("2", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL), new List<string> { "1" });

            BatchRequestContent batchRequestContent = new BatchRequestContent(new List<BatchRequestStep> { batchRequestStep1, batchRequestStep2 });

            bool isSuccess = batchRequestContent.RemoveBatchRequestStepWithId("1");

            Assert.IsTrue(isSuccess, "Failed to remove batch request step.");
            Assert.IsTrue(batchRequestContent.BatchRequestSteps.Count.Equals(1), "Unexpected batchRequestSteps set.");
            Assert.IsTrue(batchRequestContent.BatchRequestSteps["2"].DependsOn.Count.Equals(0), "Unexpected batchRequestSteps set.");
        }

        [TestMethod]
        public void BatchRequestContent_RemoveBatchRequestStepWithIdForNonExistingId()
        {
            BatchRequestStep batchRequestStep1 = new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
            BatchRequestStep batchRequestStep2 = new BatchRequestStep("2", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL), new List<string> { "1" });

            BatchRequestContent batchRequestContent = new BatchRequestContent(new List<BatchRequestStep> { batchRequestStep1, batchRequestStep2 });

            bool isSuccess = batchRequestContent.RemoveBatchRequestStepWithId("5");

            Assert.IsFalse(isSuccess, "Failed to remove batch request step.");
            Assert.IsTrue(batchRequestContent.BatchRequestSteps.Count.Equals(2), "Unexpected batchRequestSteps set.");
            Assert.AreSame(batchRequestStep2.DependsOn.First(), batchRequestContent.BatchRequestSteps["2"].DependsOn.First(), "Unexpected depends on set.");
        }

        [TestMethod]
        public async System.Threading.Tasks.Task BatchRequestContent_GetBatchRequestContentFromStepAsync()
        {
            BatchRequestStep batchRequestStep1 = new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
            BatchRequestStep batchRequestStep2 = new BatchRequestStep("2", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL), new List<string> { "1" });

            BatchRequestContent batchRequestContent = new BatchRequestContent();
            batchRequestContent.AddBatchRequestStep(batchRequestStep1);
            batchRequestContent.AddBatchRequestStep(batchRequestStep2);

            batchRequestContent.RemoveBatchRequestStepWithId("1");

            string expectedJson = "{\"requests\":[{\"id\":\"2\",\"url\":\"/me\",\"method\":\"GET\"}]}";
            JObject expectedContent = JsonConvert.DeserializeObject<JObject>(expectedJson);

            JObject requestContent = await batchRequestContent.GetBatchRequestContentAsync();

            Assert.IsNotNull(requestContent, "Unexpected response content returned.");
            Assert.IsTrue(batchRequestContent.BatchRequestSteps.Count.Equals(1), "Unexpected batchRequestSteps set.");
            Assert.AreEqual(expectedContent.ToString(), requestContent.ToString(), "Unexpected response content returned.");
        }
    }
}
