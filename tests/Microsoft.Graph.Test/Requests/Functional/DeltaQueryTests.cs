using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Graph.Test.Requests.Functional
{
    [Ignore]
    [TestClass]
    public class DeltaQueryTests : GraphTestBase
    {
        [TestMethod]
        public async System.Threading.Tasks.Task UserDeltaLink()
        {
            try
            {
                // Get our first delta page.
                var userDeltaCollectionPage = await graphClient.Users.Delta().Request().GetAsync();
                
                // Go through all of the delta pages so that we can get the delta link on the last page.
                while (userDeltaCollectionPage.NextPageRequest != null)
                {
                    userDeltaCollectionPage = await userDeltaCollectionPage.NextPageRequest.GetAsync();
                }

                // At this point we're up to date. userDeltaCollectionPage now has a deltalink.  
                object deltaLink;

                // Now let's use the deltalink to make sure there aren't any changes. We won't test this collection
                // since other tests could be making changes to the users in the org.
                if (userDeltaCollectionPage.AdditionalData.TryGetValue("@odata.deltaLink", out deltaLink))
                {
                    userDeltaCollectionPage.InitializeNextPageRequest(graphClient, deltaLink.ToString());
                    userDeltaCollectionPage = await userDeltaCollectionPage.NextPageRequest.GetAsync();
                }
                Assert.IsNotNull(deltaLink, "We did not get a deltalink back as expected.");

            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Inconclusive("Error code: {0}", e.Error.Code);
            }
        }

        [TestMethod]
        public async System.Threading.Tasks.Task GroupDeltaLink()
        {
            try
            {
                // Get our first delta page.
                var groupDeltaCollectionPage = await graphClient.Groups.Delta().Request().GetAsync();

                // Go through all of the delta pages so that we can get the delta link on the last page.
                while (groupDeltaCollectionPage.NextPageRequest != null)
                {
                    groupDeltaCollectionPage = await groupDeltaCollectionPage.NextPageRequest.GetAsync();
                }

                // At this point we're up to date. groupDeltaCollectionPage now has a deltalink.  
                object deltaLink;

                // Now let's use the deltalink to make sure there aren't any changes.
                if (groupDeltaCollectionPage.AdditionalData.TryGetValue("@odata.deltaLink", out deltaLink))
                {
                    groupDeltaCollectionPage.InitializeNextPageRequest(graphClient, deltaLink.ToString());
                    groupDeltaCollectionPage = await groupDeltaCollectionPage.NextPageRequest.GetAsync();

                    // This could be false in case a change has occurred to a group since the last deltapage.
                    Assert.IsTrue((groupDeltaCollectionPage.Count == 0), "groupDeltaCollectionPage has unexpected entry.");
                }
                Assert.IsNotNull(deltaLink, "We did not get a deltalink back as expected.");

                // Let's test what happens when we add a group.

                // Create a group. There can only be a single planner plan per group.
                var myGroup = new Group();
                myGroup.Description = "A temporary group.";
                myGroup.DisplayName = "Test group";
                myGroup.GroupTypes = new List<string>() { "Unified" };
                myGroup.MailEnabled = true;
                myGroup.MailNickname = "BobTestGroup";
                myGroup.SecurityEnabled = false;

                // Call Graph service API to create the new group.
                var syncdGroup = await graphClient.Groups.Request().AddAsync(myGroup);

                // Lets add a member to the group.
                var userToAddToGroup = new User();
                userToAddToGroup.Id = "ff1fa027-1a7a-4041-aac7-77bd88af7c9f";
                await graphClient.Groups[syncdGroup.Id].Members.References.Request().AddAsync(userToAddToGroup);


                // Call with the deltalink. We have to wait since there is some latency between the time that the 
                // group is created and the time when the delta is registered.
                await System.Threading.Tasks.Task.Delay(10000);
                groupDeltaCollectionPage.InitializeNextPageRequest(graphClient, deltaLink.ToString());
                groupDeltaCollectionPage = await groupDeltaCollectionPage.NextPageRequest.GetAsync();

                Assert.IsTrue((groupDeltaCollectionPage.Count == 1), "Expected a single change since we added a group. Something else happened.");

                // Clean up the group we created.
                var headers = new HeaderOption("Content-type", "application/json"); // Need this due to bug. Can't delete without this.
                await graphClient.Groups[syncdGroup.Id].Request().DeleteAsync();

            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Inconclusive("Error code: {0}", e.Error.Code);
            }
        }
    }
}
