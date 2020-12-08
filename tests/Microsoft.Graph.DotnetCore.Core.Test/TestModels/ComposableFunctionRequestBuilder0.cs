using System;
using System.Collections.Generic;

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels
{

    class ComposableFunctionRequestBuilder0 : BaseFunctionMethodRequestBuilder<IBaseRequest>
    {
        /// <summary>
        /// The requestUrl should contain a method called microsoft.graph.composablefunction0
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="client"></param>
        public ComposableFunctionRequestBuilder0(
            string requestUrl,
            IBaseClient client)
            : base(requestUrl, client)
        {
            this.SetParameterString();
        }

        public ComposableFunctionRequestBuilder0(
            string requestUrl,
            IBaseClient client,
            string address)
            : base(requestUrl, client)
        {
            this.SetParameter("address", address, true);
            this.SetParameterString();
        }

        protected override IBaseRequest CreateRequest(string functionUrl, IEnumerable<Option> options)
        {
            // Don't need this right now. Can implement if we need it.
            throw new NotImplementedException();
        }

        public ComposableFunctionRequestBuilder1 RequestBuilder1(string anotherValue)
        {
            return new ComposableFunctionRequestBuilder1(
            this.AppendSegmentToRequestUrl("microsoft.graph.composablefunction1"),
            this.Client,
            anotherValue);
        }
    }
}
