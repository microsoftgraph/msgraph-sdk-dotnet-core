using System;
using System.Collections.Generic;

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels
{

    class ComposableFunctionRequestBuilder1 : BaseFunctionMethodRequestBuilder<IBaseRequest>
    {
        /// <summary>
        /// The requestUrl should contain a method called ComposableFunction0
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="client"></param>
        public ComposableFunctionRequestBuilder1(
            string requestUrl,
            IBaseClient client)
            : base(requestUrl, client)
        {
        }

        public ComposableFunctionRequestBuilder1(
            string requestUrl,
            IBaseClient client,
            string anotherValue)
            : base(requestUrl, client)
        {
            this.SetParameter("anotherValue", anotherValue, true);
            this.SetFunctionParameters();
        }

        public ComposableFunctionRequestBuilder1(
            string requestUrl,
            IBaseClient client,
            string anotherValue,
            string secondValue)
            : base(requestUrl, client)
        {
            this.SetParameter("anotherValue", anotherValue, true);
            this.SetParameter("secondValue", secondValue, true);
            this.SetFunctionParameters();
        }

        protected override IBaseRequest CreateRequest(string functionUrl, IEnumerable<Option> options)
        {
            // Gives us access to the URL for test
            return new BaseRequest(functionUrl, this.Client, options);
        }
    }
}
