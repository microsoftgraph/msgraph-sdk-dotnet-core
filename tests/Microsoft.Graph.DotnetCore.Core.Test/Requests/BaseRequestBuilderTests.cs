// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests
{
    using Microsoft.Graph.DotnetCore.Core.Test.TestModels;
    using Moq;
    using System.Collections.Generic;
    using System.Net.Http;
    using Xunit;
    public class BaseRequestBuilderTests
    {
        [Fact]
        public void BaseRequestBuilder()
        {
            var requestUrl = "https://localhost";
            var client = new Mock<IBaseClient>().Object;

            var requestBuilder = new BaseRequestBuilder(requestUrl, client);

            Assert.Equal(requestUrl, requestBuilder.RequestUrl);
            Assert.Equal(client, requestBuilder.Client);
        }

        [Fact]
        public void AppendSegmentToRequestUrl()
        {
            var requestUrl = "https://localhost";
            var newUrlSegment = "segment";

            var requestBuilder = new BaseRequestBuilder(requestUrl, new Mock<IBaseClient>().Object);

            var appendedUrl = requestBuilder.AppendSegmentToRequestUrl(newUrlSegment);

            Assert.Equal(string.Join("/", requestUrl, newUrlSegment), appendedUrl);
        }

        /// <summary>
        /// Test that: 1) composable functions have all parameters set, and 2) query options are not yet applied to the URL.
        /// </summary>
        [Fact]
        public void ComposableFunctionTest()
        {
            var requestUrl = "https://localhost:443/microsoft.graph.composablefunction0";
            var client = new BaseClient(" ", new HttpClient()); // base url needs to be a non-zero length string.
            var parameter_first_function = "A1:B1";
            var parameter_second_function = "test-value";
            var queryOptions = new List<Option>() { new QueryOption("filter", "name")};

            // Create the composed function request builders
            var composableFunctionRequestBuilder0 = new ComposableFunctionRequestBuilder0(requestUrl, client, parameter_first_function);
            var composedRequestUrl = composableFunctionRequestBuilder0.RequestBuilder1(parameter_second_function).Request(queryOptions).RequestUrl;

            var expected = @"https://localhost:443/microsoft.graph.composablefunction0(address='A1:B1')/microsoft.graph.composablefunction1(anotherValue='test-value')";

            Assert.Equal(expected, composedRequestUrl);
        }
    }
}
