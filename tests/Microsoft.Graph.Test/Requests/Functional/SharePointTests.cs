using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Async = System.Threading.Tasks;

namespace Microsoft.Graph.Test.Requests.Functional
{
    [Ignore]
    [TestClass]
    public class SharePointTests : GraphTestBase
    {
        // Test search a SharePoint site.
        [TestMethod]
        public async Async.Task SharePointSearchSites()
        {
            try
            {
                // Specify the search query parameter.
                var searchQuery = new QueryOption("search", "contoso");
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
        public async Async.Task SharePointGetDocumentLibraries()
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
        public async Async.Task SharePointGetNonDefaultDocumentLibraries()
        {
            try
            {
                // Specify the search query parameter.
                var searchQuery = new QueryOption("search", "sales");
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

        /// <summary>
        /// Tests the GetSiteByPath method added in GraphServiceSitesCollectionRequestBuilderExtension.cs
        /// https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/api/site_get
        /// </summary>
        /// Open question: how is a customer expected to get Site path. This part of the experience is unclear to me. 
        /// 
        [Ignore] // Need reset test data  in demo tenant
        [TestMethod]
        public async Async.Task SharePointGetSiteWithPath()
        {
            try
            {
                // Create the request to get the root site by using the root structural property. We don't generate 
                // request builders for structural properties so we need to use HttpRequestMessage to make the request.
                string requestUrlToGetSiteRootInfo = String.Format("{0}{1}", graphClient.Sites.Request().RequestUrl, "/root");
                HttpRequestMessage hrm = new HttpRequestMessage(HttpMethod.Get, requestUrlToGetSiteRootInfo);

                // Authenticate (add access token) to our HttpRequestMessage
                await graphClient.AuthenticationProvider.AuthenticateRequestAsync(hrm);

                HttpResponseMessage response = await graphClient.HttpProvider.SendAsync(hrm);

                Site site;

                // Get the Site.
                if (response.IsSuccessStatusCode)
                {
                    // Deserialize Site object.
                    var content = await response.Content.ReadAsStringAsync();
                    site = graphClient.HttpProvider.Serializer.DeserializeObject<Site>(content);
                }
                else
                    throw new ServiceException(
                        new Error
                        {
                            Code = response.StatusCode.ToString(),
                            Message = await response.Content.ReadAsStringAsync()
                        });
                               
                string siteResource = "portals2";

                // Get the portals/Information-Technology site.
                Site portalInfoTechSite = await graphClient.Sites.GetByPath(siteResource, site.SiteCollection.Hostname).Request().GetAsync();

                StringAssert.Contains(portalInfoTechSite.WebUrl, siteResource);
                StringAssert.Contains(portalInfoTechSite.Id, portalInfoTechSite.SiteCollection.Hostname); // Check if id format changes under us. 
                Assert.AreEqual(site.SiteCollection.Hostname, portalInfoTechSite.SiteCollection.Hostname);
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
            }
        }
        
        /// <summary>
        /// Test to get information about a SharePoint site by its URL.
        /// </summary>
        [TestMethod]
        public async Async.Task SharePointAccessSiteByUrl()
        {
            try
            {
                Site site = await graphClient.Shares[UrlToSharingToken("https://m365x462896.sharepoint.com/sites/portals2")].Site.Request().GetAsync();
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
