// ------------------------------------------------------------------------------
//  Copyright (c) 2016 Microsoft Corporation
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Test.Requests
{
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.Graph;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mocks;
    using Moq;

    [TestClass]
    public class BaseClientTests
    {
        private MockAuthenticationProvider authenticationProvider;

        [TestInitialize]
        public void Setup()
        {
            this.authenticationProvider = new MockAuthenticationProvider();
        }

        [TestMethod]
        public void BaseClient_InitializeBaseUrlWithoutTrailingSlash()
        {
            var expectedBaseUrl = "https://localhost";

            var baseClient = new BaseClient(expectedBaseUrl, this.authenticationProvider.Object);

            Assert.AreEqual(expectedBaseUrl, baseClient.BaseUrl, "Unexpected base URL initialized.");
        }

        [TestMethod]
        public void BaseClient_InitializeBaseUrlWithTrailingSlash()
        {
            var expectedBaseUrl = "https://localhost";

            var baseClient = new BaseClient("https://localhost/", this.authenticationProvider.Object);

            Assert.AreEqual(expectedBaseUrl, baseClient.BaseUrl, "Unexpected base URL initialized.");
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void BaseClient_InitializeEmptyBaseUrl()
        {
            try
            {
                var baseClient = new BaseClient(null, this.authenticationProvider.Object);
            }
            catch (ServiceException exception)
            {
                Assert.AreEqual(GraphErrorCode.InvalidRequest.ToString(), exception.Error.Code, "Unexpected error code.");
                Assert.AreEqual("Base URL cannot be null or empty.", exception.Error.Message, "Unexpected error message.");
                throw;
            }
        }
    }
}
