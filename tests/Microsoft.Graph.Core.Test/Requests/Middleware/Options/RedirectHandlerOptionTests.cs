// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Core.Test.Requests.Middleware.Options
{
    using System;
    using System.Net.Http;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RedirectHandlerOptionTests
    {
        [TestMethod]
        public void RedirectHandlerOption_ShouldUseDefaultValuesIfNotSpecified()
        {
            var retryOptions = new RedirectHandlerOption();
            Assert.AreEqual(RedirectHandlerOption.DEFAULT_MAX_REDIRECT, retryOptions.MaxRedirect, "Invalid default MaxRedirectt set.");
            Assert.IsTrue(retryOptions.ShouldRedirect(null), "Invalid default ShouldRedirect output.");
        }

        [TestMethod]
        public void RedirectHandlerOption_ShouldThrowMaximumValueExceededExceptionForMaxRedirect()
        {
            ServiceException ex = Assert.ThrowsException<ServiceException>(() => new RedirectHandlerOption() { MaxRedirect = 21 });
            Assert.AreEqual(ex.Error.Code, ErrorConstants.Codes.MaximumValueExceeded, "Invalid exception code.");
            Assert.AreEqual(ex.Error.Message, string.Format(ErrorConstants.Messages.MaximumValueExceeded, "MaxRedirect", RedirectHandlerOption.MAX_MAX_REDIRECT), "Invalid exception message.");
        }

        [TestMethod]
        public void RedirectHandlerOption_ShouldAcceptCorrectValue()
        {
            int maxRedirect = 15;
            var retryOptions = new RedirectHandlerOption() { MaxRedirect = maxRedirect, ShouldRedirect = ShouldRedirect };
            Assert.AreEqual(maxRedirect, retryOptions.MaxRedirect, "Invalid MaxRedirect time set.");
            Assert.AreEqual(ShouldRedirect, retryOptions.ShouldRedirect, "Invalid ShouldRedirect delegate set.");
        }

        private bool ShouldRedirect(HttpResponseMessage rwsponse)
        {
            return false;
        }
    }
}
