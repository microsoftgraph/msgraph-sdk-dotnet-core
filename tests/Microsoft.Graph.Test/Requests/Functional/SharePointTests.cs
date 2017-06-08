using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Microsoft.Graph.Test.Requests.Functional
{
    [Ignore]
    [TestClass]
    public class SharePointTests : GraphTestBase
    {
        // Test search a SharePoint site.
        [TestMethod]
        public async System.Threading.Tasks.Task SharePointSearchSites()
        {
            try
            {
                // Specify the search query parameter.
                var searchQuery = new QueryOption("search", "a");
                var options = new List<QueryOption>();
                options.Add(searchQuery);

                // Call the Microsoft Graph API. 
                // /sites?search=a
                var siteSearchResults = await graphClient.Sites.Request(options).GetAsync();

                Assert.IsTrue(siteSearchResults.Count > 0, "Expected at least one search result. Got zero. Check test data.");

            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
            }
        }

        // Test accessing the default document libraries for a SharePoint site.
        [TestMethod]
        public async System.Threading.Tasks.Task SharePointGetDocumentLibraries()
        {
            try
            {
                // Specify the search query parameter.
                var searchQuery = new QueryOption("search", "Office 365 Demos");
                var options = new List<QueryOption>();
                options.Add(searchQuery);

                // Call the Microsoft Graph API. Expecting a single search entry from the tenant.
                var siteSearchResults = await graphClient.Sites.Request(options).GetAsync();
                Assert.IsTrue(siteSearchResults.Count > 0, "Expected at least one search result. Got zero. Check test data.");

                // Call the Microsoft Graph API. Get the drives collection page.
                SiteDrivesCollectionPage drives = (SiteDrivesCollectionPage)graphClient.Sites[siteSearchResults[0].Id].Drives.Request().GetAsync().Result;


                Assert.IsTrue(drives.Count > 0, "Expected at least one drive result. Got zero. Check test data.");

            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
            }
        }

        // Test accessing the non-default document library on a SharePoint site.
        [TestMethod]
        public async System.Threading.Tasks.Task SharePointGetNonDefaultDocumentLibraries()
        {
            try
            {
                // Specify the search query parameter.
                var searchQuery = new QueryOption("search", "Office 365 Demos");
                var options = new List<QueryOption>();
                options.Add(searchQuery);

                // Call the Microsoft Graph API. Expecting a single search entry from the tenant.
                var siteSearchResults = await graphClient.Sites.Request(options).GetAsync();
                Assert.IsTrue(siteSearchResults.Count > 0, "Expected at least one search result. Got zero. Check test data.");

                // Call the Microsoft Graph API. Get the sites drives collection page.
                SiteDrivesCollectionPage drives = (SiteDrivesCollectionPage)graphClient.Sites[siteSearchResults[0].Id]
                                                                                       .Drives
                                                                                       .Request()
                                                                                       .GetAsync()
                                                                                       .Result;

                // Call the Microsoft Graph API. Get the drives collection page.
                DriveItemChildrenCollectionPage library = (DriveItemChildrenCollectionPage)graphClient.Sites[siteSearchResults[0].Id]
                                                                                                      .Drives[drives[0].Id]
                                                                                                      .Root
                                                                                                      .Children
                                                                                                      .Request()
                                                                                                      .GetAsync()
                                                                                                      .Result;

                Assert.IsTrue(library.Count > 0, "Expected at least one driveitem result. Got zero. Check test data.");

            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
            }
        }

        [Ignore] // Issue with this. Informed service API owner. Sharing token is not recognized.
        [TestMethod]
        public async System.Threading.Tasks.Task SharePointAccessSiteByUrl()
        {
            try
            {
                // 
                Site site = await graphClient.Shares[UrlToSharingToken("https://mod810997.sharepoint.com/sites/SMBverticals")].Site.Request().GetAsync();
                Assert.IsNotNull(site);
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
            }
        }

        string UrlToSharingToken(string inputUrl)
        {
            var base64Value = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(inputUrl));
            return "u!" + base64Value.TrimEnd('=').Replace('/', '_').Replace('+', '-');
        }

    }
}
