namespace Microsoft.Graph.DotnetCore.Core.Test.Extensions
{
    using Microsoft.Graph.DotnetCore.Core.Test.Requests;
    using System.Net.Http;
    using Xunit;
    public class BaseRequestExtensionsTests: RequestTestBase
    {
        string requestUrl = "https://foo.bar";

        [Fact]
        public void WithShouldRetry_ShouldDelegateToRetryOption()
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            var baseRequest = new BaseRequest(requestUrl, this.baseClient);
            baseRequest.WithShouldRetry((response) => true);

            Assert.IsType<GraphRequestContext>(baseRequest.GetHttpRequestMessage().Properties[typeof(GraphRequestContext).ToString()]);
            Assert.True(baseRequest.GetHttpRequestMessage().GetMiddlewareOption<RetryOption>().ShouldRetry(httpResponseMessage));
        }

        [Fact]
        public void WithMaxRetry_ShouldAddMaxRetryToRetryOption()
        {
            var baseRequest = new BaseRequest(requestUrl, this.baseClient);
            baseRequest.WithMaxRetry(3);

            Assert.IsType<GraphRequestContext>(baseRequest.GetHttpRequestMessage().Properties[typeof(GraphRequestContext).ToString()]);
            Assert.Equal(3, baseRequest.GetHttpRequestMessage().GetMiddlewareOption<RetryOption>().MaxRetry);
        }

        [Fact]
        public void WithMaxRedirects_ShouldAddMaxRedirectsToRedirectOption()
        {
            var baseRequest = new BaseRequest(requestUrl, this.baseClient);
            baseRequest.WithMaxRedirects(4);

            Assert.IsType<GraphRequestContext>(baseRequest.GetHttpRequestMessage().Properties[typeof(GraphRequestContext).ToString()]);
            Assert.Equal(4, baseRequest.GetHttpRequestMessage().GetMiddlewareOption<RedirectOption>().MaxRedirects);
        }
    }
}
