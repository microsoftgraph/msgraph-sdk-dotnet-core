// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Core.Test.Requests.Middleware.Options
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Net.Http;

    [TestClass]
    public class RetrytHandlerOptionTests
    {
        [TestMethod]
        public void RetrytHandlerOption_ShouldUseDefaultValuesIfNotSpecified()
        {
            var retryOptions = new RetryHandlerOption();
            Assert.AreEqual(RetryHandlerOption.DEFAULT_DELAY, retryOptions.Delay, "Invalid default delay time set.");
            Assert.AreEqual(RetryHandlerOption.DEFAULT_MAX_RETRY, retryOptions.MaxRetry, "Invalid default maximum retry count set.");
            Assert.IsTrue(retryOptions.ShouldRetry(0, 0, null), "Invalid default ShouldRetry output.");
        }

        [TestMethod]
        public void RetrytHandlerOption_ShouldThrowMaximumValueExceededExceptionForDelayAndMaxRetry()
        {
            ServiceException ex = Assert.ThrowsException<ServiceException>(() => new RetryHandlerOption() { Delay = 181, MaxRetry = 11 });
            Assert.AreEqual(ex.Error.Code, ErrorConstants.Codes.MaximumValueExceeded, "Invalid exception.");
        }

        [TestMethod]
        public void RetrytHandlerOption_ShouldThrowMaximumValueExceededExceptionForDelay()
        {
            ServiceException ex = Assert.ThrowsException<ServiceException>(() => new RetryHandlerOption() { Delay = 200 });
            Assert.AreEqual(ex.Error.Code, ErrorConstants.Codes.MaximumValueExceeded, "Invalid exception code.");
            Assert.AreEqual(ex.Error.Message, string.Format(ErrorConstants.Messages.MaximumValueExceeded, "Delay", RetryHandlerOption.MAX_DELAY), "Invalid exception message.");
        }

        [TestMethod]
        public void RetrytHandlerOption_ShouldThrowMaximumValueExceededExceptionForMaxRetry()
        {
            ServiceException ex = Assert.ThrowsException<ServiceException>(() => new RetryHandlerOption() { Delay = 180, MaxRetry = 15 });
            Assert.AreEqual(ex.Error.Code, ErrorConstants.Codes.MaximumValueExceeded, "Invalid exception code.");
            Assert.AreEqual(ex.Error.Message, string.Format(ErrorConstants.Messages.MaximumValueExceeded, "MaxRetry", RetryHandlerOption.MAX_MAX_RETRY), "Invalid exception message.");
        }

        [TestMethod]
        public void RetrytHandlerOption_ShouldAcceptCorrectValue()
        {
            int delay = 20;
            int maxRetry = 5;
            var retryOptions = new RetryHandlerOption() { Delay = delay, MaxRetry = maxRetry, ShouldRetry = ShouldRetry };
            Assert.AreEqual(delay, retryOptions.Delay, "Invalid delay time set.");
            Assert.AreEqual(maxRetry, retryOptions.MaxRetry, "Invalid MaxRetry set.");
            Assert.AreEqual(ShouldRetry, retryOptions.ShouldRetry, "Invalid ShouldRetry delegate set.");
        }

        private bool ShouldRetry(int delay, int attempts, HttpResponseMessage rwsponse)
        {
            return false;
        }
    }
}
