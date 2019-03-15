namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    public class BatchRequestStep
    {
        public string RequestId { get; set; }
        public HttpRequestMessage Request { get; set; }
        public IList<string> DependsOn { get; set; }

        public BatchRequestStep(string requestId, HttpRequestMessage request, IList<string> dependsOn)
        {
            RequestId = requestId;
            Request = request;
            DependsOn = dependsOn;
        }
    }
}
