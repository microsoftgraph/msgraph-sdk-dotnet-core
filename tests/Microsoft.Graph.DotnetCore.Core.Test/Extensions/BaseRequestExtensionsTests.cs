namespace Microsoft.Graph.DotnetCore.Core.Test.Extensions
{
    using Microsoft.Graph.DotnetCore.Core.Test.Mocks;
    using Microsoft.Graph.DotnetCore.Core.Test.Requests;
    using Moq;
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    public class BaseRequestExtensionsTests: IDisposable
    {
        private const string requestUrl = "http://foo.bar";
        private const string defaultAuthHeader = "Default-token";
        private HttpProvider httpProvider;
        private BaseClient baseClient;
        private MockSerializer serializer = new MockSerializer();
        private TestHttpMessageHandler testHttpMessageHandler;
        private MockAuthenticationProvider defaultAuthProvider;

        public BaseRequestExtensionsTests()
        {
            defaultAuthProvider = new MockAuthenticationProvider(defaultAuthHeader);
            testHttpMessageHandler = new TestHttpMessageHandler();
            httpProvider = new HttpProvider(testHttpMessageHandler, true, serializer.Object);
            baseClient = new BaseClient("https://localhost/v1.0", defaultAuthProvider.Object, httpProvider);
        }

        public void Dispose()
        {
            httpProvider.Dispose();
        }

        [Fact]
        public void WithShouldRetry_ShouldDelegateToRetryOption()
        {
            using (HttpResponseMessage httpResponseMessage = new HttpResponseMessage())
            {
                var baseRequest = new BaseRequest(requestUrl, baseClient);
                baseRequest.WithShouldRetry((response) => true);

                Assert.IsType<GraphRequestContext>(baseRequest.GetHttpRequestMessage().Properties[typeof(GraphRequestContext).ToString()]);
                Assert.True(baseRequest.GetHttpRequestMessage().GetMiddlewareOption<RetryHandlerOption>().ShouldRetry(httpResponseMessage));
            }
        }

        [Fact]
        public void WithMaxRetry_ShouldAddMaxRetryToRetryOption()
        {
            var baseRequest = new BaseRequest(requestUrl, baseClient);
            baseRequest.WithMaxRetry(3);

            Assert.IsType<GraphRequestContext>(baseRequest.GetHttpRequestMessage().Properties[typeof(GraphRequestContext).ToString()]);
            Assert.Equal(3, baseRequest.GetHttpRequestMessage().GetMiddlewareOption<RetryHandlerOption>().MaxRetry);
        }

        [Fact]
        public void WithMaxRedirects_ShouldAddMaxRedirectsToRedirectOption()
        {
            var baseRequest = new BaseRequest(requestUrl, baseClient);
            baseRequest.WithMaxRedirects(4);

            Assert.IsType<GraphRequestContext>(baseRequest.GetHttpRequestMessage().Properties[typeof(GraphRequestContext).ToString()]);
            Assert.Equal(4, baseRequest.GetHttpRequestMessage().GetMiddlewareOption<RedirectHandlerOption>().MaxRedirects);
        }

        [Fact]
        public void WithPerRequestAuthProvider_ShouldAddPerRequestAuthProviderToAuthHandlerOption()
        {
            var requestMockAuthProvider = new MockAuthenticationProvider("PerRequest-Token");

            var baseRequest = new BaseRequest(requestUrl, baseClient);
            baseRequest.Client.PerRequestAuthProvider = () => requestMockAuthProvider.Object;
            baseRequest.WithPerRequestAuthProvider();
            var httpRequestMessage = baseRequest.GetHttpRequestMessage();

            Assert.IsType<GraphRequestContext>(baseRequest.GetHttpRequestMessage().Properties[typeof(GraphRequestContext).ToString()]);
            Assert.NotSame(baseClient.AuthenticationProvider, httpRequestMessage.GetMiddlewareOption<AuthenticationHandlerOption>().AuthenticationProvider);
            Assert.Same(requestMockAuthProvider.Object, httpRequestMessage.GetMiddlewareOption<AuthenticationHandlerOption>().AuthenticationProvider);
        }

        [Fact]
        public async Task WithPerRequestAuthProvider_ShouldUsePerRequestAuthProviderAsync()
        {
            string authorizationHeader = "PerRequest-Token";
            var requestMockAuthProvider = new MockAuthenticationProvider(authorizationHeader);

            var baseRequest = new BaseRequest(requestUrl, baseClient);
            baseRequest.Client.PerRequestAuthProvider = () => requestMockAuthProvider.Object;
            baseRequest.WithPerRequestAuthProvider();

            using (var httpResponseMessage = new HttpResponseMessage())
            {
                var httpRequestMessage = baseRequest.GetHttpRequestMessage();
                testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), httpResponseMessage);

                var returnedResponseMessage = await httpProvider.SendAsync(httpRequestMessage);

                Assert.Equal(httpResponseMessage, returnedResponseMessage);
                Assert.Equal(authorizationHeader, returnedResponseMessage.RequestMessage.Headers.Authorization.Parameter);
            }
        }

        [Fact]
        public async Task WithPerRequestAuthProvider_ShouldUseDefaultAuthProviderAsync()
        {
            string perRequestAutHeader = "PerRequest-Token";
            var requestMockAuthProvider = new MockAuthenticationProvider(perRequestAutHeader);

            var baseRequest = new BaseRequest(requestUrl, baseClient);
            baseRequest.Client.PerRequestAuthProvider = () => requestMockAuthProvider.Object;

            using (var httpResponseMessage = new HttpResponseMessage())
            {
                var httpRequestMessage = baseRequest.GetHttpRequestMessage();
                testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), httpResponseMessage);

                var returnedResponseMessage = await httpProvider.SendAsync(httpRequestMessage);

                Assert.Equal(httpResponseMessage, returnedResponseMessage);
                Assert.NotEqual(perRequestAutHeader, returnedResponseMessage.RequestMessage.Headers.Authorization.Parameter);
                Assert.Equal(defaultAuthHeader, returnedResponseMessage.RequestMessage.Headers.Authorization.Parameter);
            }
        }
    }
}
