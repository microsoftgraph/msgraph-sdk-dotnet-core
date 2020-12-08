// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests
{
    using Microsoft.Graph.DotnetCore.Core.Test.TestModels;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        /// <summary>
        /// Test that null values are accepted for nullable parameters.
        /// </summary>
        [Fact]
        public void ComposableFunctionWithNullParamTest()
        {
            var requestUrl = "https://localhost:443/microsoft.graph.composablefunction0";
            var client = new BaseClient(" ", new HttpClient()); // base url needs to be a non-zero length string.
            string parameter_first_function = null;
            var parameter_second_function = "test-value";

            // Create the composed function request builders
            var composableFunctionRequestBuilder0 = new ComposableFunctionRequestBuilder0(requestUrl, client, parameter_first_function);
            var composedRequestUrl = composableFunctionRequestBuilder0.RequestBuilder1(parameter_second_function).Request().RequestUrl;

            var expected = @"https://localhost:443/microsoft.graph.composablefunction0(address=null)/microsoft.graph.composablefunction1(anotherValue='test-value')";

            Assert.Equal(expected, composedRequestUrl);
        }

        /// <summary>
        /// Test that parenthesis are added for a function without a parameter.
        /// This will help distinguish functions without parameters from cast
        /// syntax.
        /// </summary>
        [Fact]
        public void ComposableFunctionWithNoParamsTest()
        {
            var requestUrl = "https://localhost:443/microsoft.graph.composablefunction0";
            var client = new BaseClient(" ", new HttpClient()); // base url needs to be a non-zero length string.
            var parameter_second_function = "test-value";

            // Create the composed function request builders
            var composableFunctionRequestBuilder0 = new ComposableFunctionRequestBuilder0(requestUrl, client);
            var composedRequestUrl = composableFunctionRequestBuilder0.RequestBuilder1(parameter_second_function).Request().RequestUrl;

            var expected = @"https://localhost:443/microsoft.graph.composablefunction0()/microsoft.graph.composablefunction1(anotherValue='test-value')";

            Assert.Equal(expected, composedRequestUrl);
        }

        /// <summary>
        /// Functions only support filter and orderby
        /// </summary>
        [Theory]
        [InlineData("select", "name")]
        [InlineData("count", "true")]
        [InlineData("expand", "test")]
        public void ComposableFunctionUnexpectedQueryOptionTest(string name, string value)
        {
            var requestUrl = "https://localhost:443/microsoft.graph.composablefunction0";
            var client = new BaseClient(" ", new HttpClient()); // base url needs to be a non-zero length string.
            var queryOptions = new List<Option>() { new QueryOption(name, value) };

            // Create the composed function request builders
            var composableFunctionRequestBuilder0 = new ComposableFunctionRequestBuilder0(requestUrl, client, string.Empty);
            
            var exception = Assert.Throws<ArgumentException>(() => composableFunctionRequestBuilder0.Request(queryOptions));
            Assert.Equal("You can only use filter and orderby query options with this function.", exception.Message);
        }

        /// <summary>
        /// Functions only support filter and orderby
        /// </summary>
        [Theory]
        [InlineData("filter", "name")]
        [InlineData("orderby", "test")]
        public void ComposableFunctionExpectedQueryOptionTest(string name, string value)
        {
            var requestUrl = "https://localhost:443/microsoft.graph.composablefunction0";
            var client = new BaseClient(" ", new HttpClient()); // base url needs to be a non-zero length string.
            var queryOptions = new List<Option>() { new QueryOption(name, value) };

            // Create the composed function request builders
            var composableFunctionRequestBuilder1 = new ComposableFunctionRequestBuilder1(requestUrl, client, string.Empty);

            var baseRequest = composableFunctionRequestBuilder1.Request(queryOptions);
            var isQueryParamSet = baseRequest.QueryOptions.Any(qp => qp.Name.ToLower() == name);
            Assert.True(isQueryParamSet, $"The expected query parameter \"{name}\" was not set.");
        }
    }
}
