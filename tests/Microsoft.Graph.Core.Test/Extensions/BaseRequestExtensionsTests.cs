namespace Microsoft.Graph.Core.Test.Extensions
{
    using Microsoft.Graph.Core.Test.Mocks;
    using Microsoft.Graph.Core.Test.Requests;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    [TestClass]
    public class BaseRequestExtensionsTests: IDisposable
    {
        private const string requestUrl = "http://foo.bar";
        private const string defaultAuthHeader = "Default-token";
        private HttpProvider httpProvider;
        private BaseClient baseClient;
        private MockSerializer serializer = new MockSerializer();
        private TestHttpMessageHandler testHttpMessageHandler;
        private MockAuthenticationProvider defaultAuthProvider;

        [TestInitialize]
        public void SetUp()
        {
            defaultAuthProvider = new MockAuthenticationProvider(defaultAuthHeader);
            testHttpMessageHandler = new TestHttpMessageHandler();
            httpProvider = new HttpProvider(testHttpMessageHandler, true, serializer.Object);
            baseClient = new BaseClient("https://localhost/v1.0", defaultAuthProvider.Object, httpProvider);
        }

        [TestCleanup]
        public void Dispose()
        {
            httpProvider.Dispose();
        }

        [TestMethod]
        public void WithShouldRetry_ShouldDelegateToRetryOption()
        {
            using (HttpResponseMessage httpResponseMessage = new HttpResponseMessage())
            {
                var baseRequest = new BaseRequest(requestUrl, baseClient);
                baseRequest.WithShouldRetry((response) => true);

                Assert.IsInstanceOfType(baseRequest.GetHttpRequestMessage().Properties[typeof(GraphRequestContext).ToString()], typeof(GraphRequestContext), "Unexpected request context.");
                Assert.IsTrue(baseRequest.GetHttpRequestMessage().GetMiddlewareOption<RetryHandlerOption>().ShouldRetry(httpResponseMessage), "Unexpected middleware option.");
            }
        }

        [TestMethod]
        public void WithMaxRetry_ShouldAddMaxRetryToRetryOption()
        {
            var baseRequest = new BaseRequest(requestUrl, baseClient);
            baseRequest.WithMaxRetry(3);

            Assert.IsInstanceOfType(baseRequest.GetHttpRequestMessage().Properties[typeof(GraphRequestContext).ToString()], typeof(GraphRequestContext), "Unexpected request context.");
            Assert.AreEqual(3, baseRequest.GetHttpRequestMessage().GetMiddlewareOption<RetryHandlerOption>().MaxRetry, "Unexpected max retry value.");
        }

        [TestMethod]
        public void WithMaxRedirects_ShouldAddMaxRedirectsToRedirectOption()
        {
            var baseRequest = new BaseRequest(requestUrl, baseClient);
            baseRequest.WithMaxRedirects(4);

            Assert.IsInstanceOfType(baseRequest.GetHttpRequestMessage().Properties[typeof(GraphRequestContext).ToString()], typeof(GraphRequestContext), "Unexpected request context");
            Assert.AreEqual(4, baseRequest.GetHttpRequestMessage().GetMiddlewareOption<RedirectHandlerOption>().MaxRedirects, "Unexpected max redirects value.");
        }

        [TestMethod]
        public void WithPerRequestAuthProvider_ShouldAddPerRequestAuthProviderToAuthHandlerOption()
        {
            var requestMockAuthProvider = new MockAuthenticationProvider("PerRequest-Token");

            var baseRequest = new BaseRequest(requestUrl, baseClient);
            baseRequest.Client.PerRequestAuthProvider = () => requestMockAuthProvider.Object;
            baseRequest.WithPerRequestAuthProvider();
            var httpRequestMessage = baseRequest.GetHttpRequestMessage();

            Assert.IsInstanceOfType(httpRequestMessage.Properties[typeof(GraphRequestContext).ToString()], typeof(GraphRequestContext), "Unexpected request context.");
            Assert.AreNotSame(baseClient.AuthenticationProvider, httpRequestMessage.GetMiddlewareOption<AuthenticationHandlerOption>().AuthenticationProvider, "Unexpected auth provider set.");
            Assert.AreSame(requestMockAuthProvider.Object, httpRequestMessage.GetMiddlewareOption<AuthenticationHandlerOption>().AuthenticationProvider, "Unexpected auth provider set.");
        }

        [TestMethod]
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

                Assert.AreEqual(httpResponseMessage, returnedResponseMessage);
                Assert.AreEqual(authorizationHeader, returnedResponseMessage.RequestMessage.Headers.Authorization.Parameter);
            }
        }

        [TestMethod]
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

                Assert.AreEqual(httpResponseMessage, returnedResponseMessage);
                Assert.AreNotEqual(perRequestAutHeader, returnedResponseMessage.RequestMessage.Headers.Authorization.Parameter);
                Assert.AreEqual(defaultAuthHeader, returnedResponseMessage.RequestMessage.Headers.Authorization.Parameter);
            }
        }
    }
}
