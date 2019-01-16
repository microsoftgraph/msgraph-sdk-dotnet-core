namespace Microsoft.Graph.DotnetCore.Core.Test.Extensions
{
    using Microsoft.Graph.DotnetCore.Core.Test.Requests;
    using System.Net.Http;
    using Xunit;
    public class BaseRequestExtensionsTests: RequestTestBase
    {
        string requestUrl = "https://foo.bar";
        BaseRequest baseRequest;
        public BaseRequestExtensionsTests()
        {
            baseRequest = new BaseRequest(requestUrl, this.baseClient);
        }

        [Fact]
        public void WithScopes_ShouldAddScopesToAuthOption()
        {
            string[] scopes = new string[] { "foo.bar", "user.bar", "user.foo"};

            baseRequest.WithScopes(scopes);

            Assert.IsType<RequestContext>(baseRequest.GetHttpRequestMessage().Properties[typeof(RequestContext).ToString()]);
            Assert.Same(scopes, baseRequest.GetHttpRequestMessage().GetMiddlewareOption<AuthOption>().Scopes);
        }

        [Fact]
        public void WithScopes_ShouldOnlyAddScopesToExistingAuthOption()
        {
            string[] scopes = new string[] { "foo.bar", "user.bar", "user.foo" };

            baseRequest
                .WithForceRefresh(false)
                .WithScopes(scopes);

            Assert.IsType<RequestContext>(baseRequest.GetHttpRequestMessage().Properties[typeof(RequestContext).ToString()]);
            Assert.Equal(false, baseRequest.GetHttpRequestMessage().GetMiddlewareOption<AuthOption>().ForceRefresh);
            Assert.Same(scopes, baseRequest.GetHttpRequestMessage().GetMiddlewareOption<AuthOption>().Scopes);
        }

        [Fact]
        public void WithForceRefresh_ShouldAddForceRefreshToAuthOption()
        {
            string requestUrl = "https://foo.bar";
            var request = new BaseRequest(requestUrl, this.baseClient);

            request.WithForceRefresh(true);

            Assert.IsType<RequestContext>(request.GetHttpRequestMessage().Properties[typeof(RequestContext).ToString()]);
            Assert.True(request.GetHttpRequestMessage().GetMiddlewareOption<AuthOption>().ForceRefresh);
        }

        [Fact]
        public void WithShouldRetry_ShouldDelegateToRetryOption()
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();

            baseRequest.WithShouldRetry((response) => true);

            Assert.IsType<RequestContext>(baseRequest.GetHttpRequestMessage().Properties[typeof(RequestContext).ToString()]);
            Assert.True(baseRequest.GetHttpRequestMessage().GetMiddlewareOption<RetryOption>().ShouldRetry(httpResponseMessage));
        }

        [Fact]
        public void WithMaxRetry_ShouldAddMaxRetryToRetryOption()
        {
            baseRequest.WithMaxRetry(3);

            Assert.IsType<RequestContext>(baseRequest.GetHttpRequestMessage().Properties[typeof(RequestContext).ToString()]);
            Assert.Equal(3, baseRequest.GetHttpRequestMessage().GetMiddlewareOption<RetryOption>().MaxRetry);
        }

        [Fact]
        public void WithMaxRedirects_ShouldAddMaxRedirectsToRedirectOption()
        {
            baseRequest.WithMaxRedirects(4);

            Assert.IsType<RequestContext>(baseRequest.GetHttpRequestMessage().Properties[typeof(RequestContext).ToString()]);
            Assert.Equal(4, baseRequest.GetHttpRequestMessage().GetMiddlewareOption<RedirectOption>().MaxRedirects);
        }

        [Fact]
        public void AddMiddlewareOptions_ShouldAddMiddlewareOptionsToTheRequestContext()
        {
            var request = new BaseRequest("http://localhost.bar", this.baseClient);
            var middlewareOptions = new IMiddlewareOption[]
            {
                new RetryOption { MaxRetry = 10, ShouldRetry = (response) => true },
                new AuthOption { ForceRefresh = false, Scopes = new string[] { "foo.bar", "user.read" } },
                new RedirectOption { MaxRedirects = 6 }
            };

            request.AddMiddlewareOptions(middlewareOptions);

            Assert.IsType<RequestContext>(request.GetHttpRequestMessage().Properties[typeof(RequestContext).ToString()]);
            Assert.Equal(middlewareOptions.Length, request.GetHttpRequestMessage().GetRequestContext().MiddlewareOptions.Count);
            Assert.Same(middlewareOptions[1], request.GetHttpRequestMessage().GetMiddlewareOption<AuthOption>());
        }
    }
}
