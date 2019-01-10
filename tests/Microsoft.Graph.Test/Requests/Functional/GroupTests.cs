using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Async = System.Threading.Tasks;

namespace Microsoft.Graph.Test.Requests.Functional
{
    [Ignore]
    [TestClass]
    public class GroupTests : GraphTestBase
    {
        /// <summary>
        /// Create a team on a group.
        /// </summary>
        [TestMethod]
        public async Async.Task GroupCreateTeam()
        {
            try
            {
                // Get a groups collection. We'll use the first entry to add the team. Results in a call to the service.
                IGraphServiceGroupsCollectionPage groupPage = await graphClient.Groups.Request().GetAsync();

                // Create a team with settings.
                Team team = new Team()
                {
                    MemberSettings = new TeamMemberSettings()
                    {
                        AllowCreateUpdateChannels = true
                    }
                };

                // Add a team to the group.  Results in a call to the service.
                await graphClient.Groups[groupPage[8].Id].Team.Request().PutAsync(team);
            }
            catch (ServiceException e)
            {
                Assert.Fail(e.Error.ToString());
            }
        }
    }
}