namespace Microsoft.Graph.Core.Test.Extensions
{
    using Microsoft.Graph.Core.Test.Requests;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Net.Http;

    [TestClass]
    public class BaseRequestExtensionsTests: RequestTestBase
    {
        string requestUrl = "https://foo.bar";

        [TestMethod]
        public void WithShouldRetry_ShouldDelegateToRetryOption()
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            var baseRequest = new BaseRequest(requestUrl, this.baseClient);
            baseRequest.WithShouldRetry((response) => true);

            Assert.IsInstanceOfType(baseRequest.GetHttpRequestMessage().Properties[typeof(GraphRequestContext).ToString()], typeof(GraphRequestContext), "Unexpected request context.");
            Assert.IsTrue(baseRequest.GetHttpRequestMessage().GetMiddlewareOption<RetryOption>().ShouldRetry(httpResponseMessage), "Unexpected middleware option.");
        }

        [TestMethod]
        public void WithMaxRetry_ShouldAddMaxRetryToRetryOption()
        {
            var baseRequest = new BaseRequest(requestUrl, this.baseClient);
            baseRequest.WithMaxRetry(3);

            Assert.IsInstanceOfType(baseRequest.GetHttpRequestMessage().Properties[typeof(GraphRequestContext).ToString()], typeof(GraphRequestContext), "Unexpected request context.");
            Assert.AreEqual(3, baseRequest.GetHttpRequestMessage().GetMiddlewareOption<RetryOption>().MaxRetry, "Unexpected max retry value.");
        }

        [TestMethod]
        public void WithMaxRedirects_ShouldAddMaxRedirectsToRedirectOption()
        {
            var baseRequest = new BaseRequest(requestUrl, this.baseClient);
            baseRequest.WithMaxRedirects(4);

            Assert.IsInstanceOfType(baseRequest.GetHttpRequestMessage().Properties[typeof(GraphRequestContext).ToString()], typeof(GraphRequestContext), "Unexpected request context");
            Assert.AreEqual(4, baseRequest.GetHttpRequestMessage().GetMiddlewareOption<RedirectOption>().MaxRedirects, "Unexpected max redirects value.");
        }
    }
}
