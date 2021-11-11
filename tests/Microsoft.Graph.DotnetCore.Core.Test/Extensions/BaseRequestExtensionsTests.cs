namespace Microsoft.Graph.DotnetCore.Core.Test.Extensions
{
    using Microsoft.Graph.DotnetCore.Core.Test.Mocks;
    using Microsoft.Kiota.Http.HttpClientLibrary.Extensions;
    using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;
    using System;
    using System.Net.Http;
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

            var defaultHandlers = GraphClientFactory.CreateDefaultHandlers();
            var pipeline = GraphClientFactory.CreatePipeline(defaultHandlers, this.testHttpMessageHandler);

            httpProvider = new HttpProvider(pipeline, true, serializer.Object);
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
                int delay = 1;
                int attempt = 1;
                var baseRequest = new BaseRequest(requestUrl, baseClient);
                baseRequest.WithShouldRetry((d, a, r) => false);

                Assert.IsType<GraphRequestContext>(baseRequest.GetHttpRequestMessage().Properties[nameof(GraphRequestContext)]);
                Assert.False(baseRequest.GetHttpRequestMessage().GetRequestOption<RetryHandlerOption>().ShouldRetry(delay, attempt, httpResponseMessage));
            }
        }

        [Fact]
        public void WithMaxRetry_ShouldAddMaxRetryToRetryOption()
        {
            var baseRequest = new BaseRequest(requestUrl, baseClient);
            baseRequest.WithMaxRetry(3);

            Assert.IsType<GraphRequestContext>(baseRequest.GetHttpRequestMessage().Properties[nameof(GraphRequestContext)]);
            Assert.Equal(3, baseRequest.GetHttpRequestMessage().GetRequestOption<RetryHandlerOption>().MaxRetry);
        }

        [Fact]
        public void WithMaxRedirects_ShouldAddMaxRedirectsToRedirectOption()
        {
            var baseRequest = new BaseRequest(requestUrl, baseClient);
            baseRequest.WithMaxRedirects(4);
            var request = baseRequest.GetHttpRequestMessage();

            Assert.IsType<GraphRequestContext>(baseRequest.GetHttpRequestMessage().Properties[nameof(GraphRequestContext)]);
            Assert.Equal(4, baseRequest.GetHttpRequestMessage().GetRequestOption<RedirectHandlerOption>().MaxRedirect);
        }
        /* TODO bring me back!
        [Fact]
        public void WithPerRequestAuthProvider_ShouldAddPerRequestAuthProviderToAuthHandlerOption()
        {
            var requestMockAuthProvider = new MockAuthenticationProvider("PerRequest-Token");

            var baseRequest = new BaseRequest(requestUrl, baseClient);
            baseRequest.Client.PerRequestAuthProvider = () => requestMockAuthProvider.Object;
            var httpRequestMessage = baseRequest.GetHttpRequestMessage();

            Assert.IsType<GraphRequestContext>(baseRequest.GetHttpRequestMessage().Properties[nameof(GraphRequestContext)]);
            Assert.NotSame(baseClient.AuthenticationProvider, httpRequestMessage.GetRequestOption<AuthenticationHandlerOption>().AuthenticationProvider);
            Assert.Same(requestMockAuthProvider.Object, httpRequestMessage.GetRequestOption<AuthenticationHandlerOption>().AuthenticationProvider);
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

        [Fact]
        public void WithScopes_ShouldUseScopesProvided()
        {
            //Arrange
            var scopes = new string[] { "User.Read", "Mail.Send"};
            var baseRequest = new BaseRequest(requestUrl, baseClient);
            
            // Act
            baseRequest.WithScopes(scopes);

            // Assert
            Assert.IsType<GraphRequestContext>(baseRequest.GetHttpRequestMessage().Properties[nameof(GraphRequestContext)]);
            var messageScopes = baseRequest.GetHttpRequestMessage().GetRequestOption<AuthenticationHandlerOption>()
                .AuthenticationProviderOption.Scopes;
            Assert.Equal(2, messageScopes.Length);
            Assert.Equal(scopes[0], messageScopes[0]);
            Assert.Equal(scopes[1], messageScopes[1]);
        }
        */
    }
}
