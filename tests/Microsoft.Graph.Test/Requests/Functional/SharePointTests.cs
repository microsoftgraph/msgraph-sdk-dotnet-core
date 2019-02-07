using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Async = System.Threading.Tasks;

namespace Microsoft.Graph.Test.Requests.Functional
{
    [Ignore]
    [TestClass]
    public class Given_a_valid_SharePoint_Site : GraphTestBase
    {
        // Test search a SharePoint site.
        [TestMethod]
        public async Async.Task It_searches_the_SharePoint_Site_and_returns_results()
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

        // Test accessing the document libraries for a SharePoint site.
        [TestMethod]
        public async Async.Task It_gets_the_sites_drives()
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
        public async Async.Task It_gets_the_sites_drives_root_children()
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
        public async Async.Task It_gets_a_site_by_path()
        {
            try
            {
                // Create the request to get the root site by using the root structural property. We don't generate 
                // request builders for structural properties so we need to use HttpRequestMessage to make the request.
                string requestUrlToGetSiteRootInfo = String.Format("{0}{1}", graphClient.Sites.Request().RequestUrl, "/root");
                HttpRequestMessage hrm = new HttpRequestMessage(HttpMethod.Get, requestUrlToGetSiteRootInfo);

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
                               
                string siteResource = "sites/IT";

                // Get the sites/IT site.
                Site portalInfoTechSite = await graphClient.Sites.GetByPath(siteResource, site.SiteCollection.Hostname).Request().GetAsync();

                StringAssert.Contains(portalInfoTechSite.WebUrl, siteResource);
                StringAssert.Contains(portalInfoTechSite.Id, portalInfoTechSite.SiteCollection.Hostname); // Check if id format changes under us. 
                Assert.AreEqual(site.SiteCollection.Hostname, portalInfoTechSite.SiteCollection.Hostname);

                // Get the site's drive
                Drive techDrive = await graphClient.Sites.GetByPath(siteResource, site.SiteCollection.Hostname).Drive.Request().GetAsync();
                Assert.IsNotNull(techDrive);
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
            }
        }

        /// <summary>
        /// Test the custom 'Root' partial request builder.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Async.Task It_gets_the_root_site()
        {
            try
            {
                Site site = await graphClient.Sites.Root.Request().GetAsync();
                Assert.IsNotNull(site);
            }
            catch (Exception)
            {
                Assert.Fail("An unexpected exception was thrown. This test case failed.");
            }
        }

        /// <summary>
        /// Test to get information about a SharePoint site by its URL.
        /// </summary>
        [TestMethod]
        public async Async.Task It_gets_a_site_by_URL()
        {
            try
            {
                Site site = await graphClient.Shares[UrlToSharingToken("https://m365x462896.sharepoint.com/sites/portals2")].Site.Request().GetAsync();

                var sites = await graphClient.Sites.Request(new List<Option>() {new QueryOption("search", "*")}).GetAsync();
                
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
