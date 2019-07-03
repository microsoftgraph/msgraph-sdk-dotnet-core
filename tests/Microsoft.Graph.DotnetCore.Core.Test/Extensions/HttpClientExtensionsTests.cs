// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Extensions
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using Xunit;
    public class HttpClientExtensionsTests
    {
        [Fact]
        public void SetFeatureFlag_should_add_new_flag_to_featureflag_header()
        {
            HttpClient client = new HttpClient();
            client.SetFeatureFlag(FeatureFlag.LongRunningOperationHandler);

            string expectedHeaderValue = Enum.Format(typeof(FeatureFlag), FeatureFlag.LongRunningOperationHandler, "x");

            Assert.True(client.DefaultRequestHeaders.Contains(CoreConstants.Headers.FeatureFlag));
            Assert.True(client.DefaultRequestHeaders.GetValues(CoreConstants.Headers.FeatureFlag).Count().Equals(1));
            Assert.Equal(client.DefaultRequestHeaders.GetValues(CoreConstants.Headers.FeatureFlag).First(), expectedHeaderValue);
        }

        [Fact]
        public void SetFeatureFlag_should_add_flag_to_existing_featureflag_header_values()
        {
            FeatureFlag flags = FeatureFlag.AuthHandler | FeatureFlag.CompressionHandler | FeatureFlag.RetryHandler | FeatureFlag.RedirectHandler;

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add(CoreConstants.Headers.FeatureFlag, Enum.Format(typeof(FeatureFlag), FeatureFlag.DefaultHttpProvider, "x"));
            client.SetFeatureFlag(flags);

            // 0000004F
            string expectedHeaderValue = Enum.Format(typeof(FeatureFlag), flags |= FeatureFlag.DefaultHttpProvider, "x");

            Assert.True(client.DefaultRequestHeaders.Contains(CoreConstants.Headers.FeatureFlag));
            Assert.True(client.DefaultRequestHeaders.GetValues(CoreConstants.Headers.FeatureFlag).Count().Equals(1));
            Assert.Equal(client.DefaultRequestHeaders.GetValues(CoreConstants.Headers.FeatureFlag).First(), expectedHeaderValue);
        }
    }
}
