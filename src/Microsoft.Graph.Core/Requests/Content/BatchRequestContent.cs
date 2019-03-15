// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    public class BatchRequestContent
    {
        private const int MAX_NUMBER_OF_REQUESTS = 20;
        public IDictionary<string, BatchRequestStep> BatchRequestSteps { get; private set; }

        public BatchRequestContent()
        {
            BatchRequestSteps = new Dictionary<string, BatchRequestStep>();
        }

        public BatchRequestContent(IList<BatchRequestStep> batchRequestSteps)
        {
            // TODO: Handle null case
            if(batchRequestSteps.Count() > MAX_NUMBER_OF_REQUESTS)
                throw new ArgumentException("Number of batch request steps cannot exceed " + MAX_NUMBER_OF_REQUESTS);

            BatchRequestSteps = new Dictionary<string, BatchRequestStep>();
            foreach (BatchRequestStep requestStep in batchRequestSteps)
                AddBatchRequestStep(requestStep);
        }

        public bool AddBatchRequestStep(BatchRequestStep batchRequestStep)
        {
            if (BatchRequestSteps.ContainsKey(batchRequestStep.RequestId))
                return false;
            BatchRequestSteps.Add(batchRequestStep.RequestId, batchRequestStep);
            return true;
        }

        public bool RemoveBatchRequestStepWithId(string requestId)
        {
            bool isRemoved = false;
            if (BatchRequestSteps.ContainsKey(requestId)) {
                BatchRequestSteps.Remove(requestId);
                isRemoved = true;
                foreach (KeyValuePair<string, BatchRequestStep> batchRequestStep in BatchRequestSteps)
                {
                    if (batchRequestStep.Value != null && batchRequestStep.Value.DependsOn != null)
                        while (batchRequestStep.Value.DependsOn.Remove(requestId)) ;
                }
            }
            return isRemoved;
        }

        public async Task<JObject> GetBatchRequestContentAsync(BatchRequestStep batchRequestStep)
        {
            JObject jRequestContent = new JObject();
            jRequestContent.Add("id", batchRequestStep.RequestId);
            jRequestContent.Add("url", GetRelativeUrl(batchRequestStep.Request.RequestUri));
            jRequestContent.Add("method", batchRequestStep.Request.Method.Method);

            if(batchRequestStep.Request.Headers != null && batchRequestStep.Request.Headers.Count() > 0)
                jRequestContent.Add("headers", GetRequestHeader(batchRequestStep.Request.Headers));

            if (batchRequestStep.DependsOn != null && batchRequestStep.DependsOn.Count() > 0)
                jRequestContent.Add("dependsOn", new JArray(batchRequestStep.DependsOn));

            if(batchRequestStep.Request != null && batchRequestStep.Request.Content != null)
            {
                try
                {
                    jRequestContent.Add("body", await GetRequestBodyAsync(batchRequestStep.Request));
                }
                catch (Exception ex)
                {
                    // TODO: Throw right exception
                    throw;
                }
            }

            return jRequestContent;
        }

        private async Task<JObject> GetRequestBodyAsync(HttpRequestMessage request)
        {
            HttpRequestMessage clonedRequest = await request.CloneAsync();
            byte[] content = await clonedRequest.Content.ReadAsByteArrayAsync();
            return JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(content, 0, content.Length));
        }

        private JObject GetRequestHeader(HttpRequestHeaders headers)
        {
            JObject jHeaders = new JObject();
            foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
            {
                jHeaders.Add(header.Key, GetHeaderValuesAsString(header.Value));
            }

            return jHeaders;
        }

        private string GetHeaderValuesAsString(IEnumerable<string> headerValues)
        {
            if (headerValues == null || headerValues.Count() == 0)
                return string.Empty;

            StringBuilder builder = new StringBuilder();
            foreach (string headerValue in headerValues)
            {
                builder.Append(";");
                builder.Append(headerValue);
            }

            return builder.ToString();
        }

        private string GetRelativeUrl(Uri requestUri)
        {
            string version = "v1.0";
            if (requestUri.AbsoluteUri.Contains("beta"))
                version = "beta";

            return requestUri.AbsoluteUri.Substring(requestUri.AbsoluteUri.IndexOf(version) + version.ToCharArray().Count());
        }
    }
}
