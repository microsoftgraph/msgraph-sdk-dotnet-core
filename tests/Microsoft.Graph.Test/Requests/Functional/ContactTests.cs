using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Graph.Test.Requests.Functional
{
    [Ignore]
    [TestClass]
    public class ContactTests : GraphTestBase
    {
        // OData URL convention test to make sure we handle 'sub-query option' on expanded navigation properties.
        // OData URL conventions 5.1.2 System Query Option $expand
        [TestMethod]
        public async Task ContactsExpandExtensionsPaging()
        {
            try
            {
                IUserContactsCollectionPage page = await graphClient.Me.Contacts.Request().Expand($"extensions($filter=Id eq 'Microsoft.OutlookServices.OpenTypeExtension.Com.Contoso.Mainer')").GetAsync();

                // When expanding extensions, a filter must be provided to specify which extensions to expand. For example $expand=Extensions($filter=Id eq 'Com.Insightly.CRMOpportunity').
                while (page.NextPageRequest != null)
                {
                    page = await page.NextPageRequest.GetAsync();
                }
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
            }
        }

        [TestMethod]
        public async Task GetContactsPaging()
        {
            try
            {
                IUserContactsCollectionPage page = await graphClient.Me.Contacts.Request().GetAsync();

                while (page.NextPageRequest != null)
                {
                    page = await page.NextPageRequest.GetAsync();
                }
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
            }
        }
    }
}
