using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using Async = System.Threading.Tasks;

namespace Microsoft.Graph.Test.Requests.Functional
{
    [Ignore]
    [TestClass]
    public class MiscTests : GraphTestBase
    {
        /// <summary>
        /// This test will fail since the service doesn't expect Odata.type=extension. This is a break in the naming pattern that the service expects.
        /// </summary>
        [TestMethod]
        public async Async.Task GroupCreateExtension()
        {
            // Get a groups collection. We'll use the first entry to add the extension. Results in a call to the service.
            IGraphServiceGroupsCollectionPage groupPage = await graphClient.Groups.Request().GetAsync();

            // Create the extension property.
            OpenTypeExtension newExtension = new OpenTypeExtension();
            newExtension.ExtensionName = "com.contoso.trackingKey";
            newExtension.AdditionalData = new Dictionary<string, object>();
            newExtension.AdditionalData.Add("trackingKeyMajor", "ABC");
            newExtension.AdditionalData.Add("trackingKeyMinor", "123");

            // Add an extension to the group. Results in a call to the service.
            var extension = await graphClient.Groups[groupPage[0].Id].Extensions.Request().AddAsync(newExtension);
        }
    }
}
