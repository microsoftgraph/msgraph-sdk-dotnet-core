using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Graph.Test.Requests.Functional
{
    //[Ignore]
    [TestClass]
    public class PlannerTests : GraphTestBase
    {
        // Working as expected.
        [TestMethod]
        public async System.Threading.Tasks.Task PlannerGetPlannerPlan()
        {
            try
            {
                var plannerPlan = await GetPlannerPlan();

                Assert.IsNotNull(plannerPlan);
                Assert.IsInstanceOfType(plannerPlan, typeof(PlannerPlan));
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
            }
        }

        public async System.Threading.Tasks.Task<PlannerPlan> GetPlannerPlan(string planId = "")
        {
            if (planId == "")
            {
                planId = "HJLUP2ZwhE6-Gd0Sp3UMB2QAHsEe"; // OnlineMarketingGroup PlanId in test tenant.
            }

            try
            {
                return await graphClient.Planner.Plans[planId].Request().GetAsync();
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Tried to get a PlannerPlan and failed. Error code: {0}", e.Error.Code);
            }
            return null;
        }

        // Successful 4/27/2017 - Gets planner tasks
        [TestMethod]
        public async System.Threading.Tasks.Task PlannerGetPlannerTasks()
        {
            try
            {
                // Get a default plan
                var plannerPlan = await GetPlannerPlan();

                var plannerPlanTasksCollectionPage = await graphClient.Planner.Plans[plannerPlan.Id].Tasks.Request().GetAsync();

                Assert.IsNotNull(plannerPlanTasksCollectionPage);
                Assert.IsInstanceOfType(plannerPlanTasksCollectionPage, typeof(IPlannerPlanTasksCollectionPage));
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
            }
        }

        // Successful 4/27/2017 - Creates a task without a bucket.
        [TestMethod]
        //https://developer.microsoft.com/en-us/graph/docs/api-reference/beta/api/planner_post_tasks
        public async System.Threading.Tasks.Task PlannerTaskCreate()
        {
            // Get a default plan
            var plannerPlan = await GetPlannerPlan();
            PlannerTask plannerTaskOnClient = new PlannerTask();
            plannerTaskOnClient.PlanId = plannerPlan.Id;
            plannerTaskOnClient.Title = "New task title";
            plannerTaskOnClient.Assignments = new PlannerAssignments();
            plannerTaskOnClient.Assignments.AddAssignee("me");

            try
            {
                PlannerTask plannerTaskOnService = await graphClient.Planner.Tasks.Request().AddAsync(plannerTaskOnClient);

                Assert.IsNotNull(plannerTaskOnService);
                Assert.AreEqual(plannerTaskOnClient.Title, plannerTaskOnService.Title);
                Assert.AreEqual(1, plannerTaskOnService.Assignments.Count);
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
            }
        }








        //[TestMethod]
        //public async System.Threading.Tasks.Task PlannerCreatePlan2()
        //{
        //    PlannerPlan plannerPlan = new PlannerPlan();

        //    try
        //    {
        //        // Create a group. There can only be a single planner plan per group.
        //        var clientOnlyGroup = new Group();
        //        clientOnlyGroup.Description = "A temporary group to that contains a planner plan that we'll test.";
        //        clientOnlyGroup.DisplayName = "Test group";
        //        clientOnlyGroup.GroupTypes = new List<string>() { "Unified" };
        //        clientOnlyGroup.MailEnabled = true;
        //        clientOnlyGroup.MailNickname = "BobTestGroup";
        //        clientOnlyGroup.SecurityEnabled = false;

        //        // Call Graph service API to create the new group.
        //        var syncdGroup = await graphClient.Groups.Request().AddAsync(clientOnlyGroup);

        //        // Create a planner plan on the group we created.
        //        plannerPlan.Owner = syncdGroup.Id;
        //        plannerPlan.Title = "Plan to make Planner great - test plan";

        //        // Call the Graph service API to add the planner plan to the group. Get a planner plan back.
        //        //var plannerPlanFromResponse = await graphClient.Groups[syncdGroup.Id].Planner.Plans.Request().AddAsync(plannerPlan);



        //        Plan plan = new Plan();
        //        plan.Owner = syncdGroup.Id;
        //        plan.Title = "Plan to make Planner great - test plan";

        //        await graphClient.Groups[syncdGroup.Id].Plans.References.Request().AddAsync(plan);

        //        //var plannerPlanFromResponse = await graphClient.Planner.Plans.Request().AddAsync(plannerPlan);

        //        //Assert.IsNotNull(plannerPlanFromResponse);
        //        //StringAssert.Contains(plannerPlan.Title, plannerPlanFromResponse.Title, "Created planner title does not match.");
        //        //Assert.IsInstanceOfType(plannerPlanFromResponse, typeof(PlannerPlan));
        //    }
        //    catch (Microsoft.Graph.ServiceException e)
        //    {
        //        Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
        //    }
        //    catch (Exception e)
        //    {
        //        Assert.Fail($"An error occurred that probably wasn't caused by the service or client. Error message: {e.Message}");
        //    }
        //}


        // This doesn't appear to work. A plan is created by default for a group and we can only create a single plan per group.
        // And since we can't delete a group, this wouldn't work since the limit appears to be one.
        // https://developer.microsoft.com/en-us/graph/docs/api-reference/beta/api/planner_post_plans
        //[TestMethod]
        //public async System.Threading.Tasks.Task PlannerCreatePlan()
        //{

        //    // Create a planner plan on the group we created.
        //    var onlineMarketingGroupId = "69cbc5f6-8269-423a-8a24-64a85c281765";
        //    PlannerPlan plannerPlan = new PlannerPlan();
        //    plannerPlan.Owner = onlineMarketingGroupId;
        //    plannerPlan.Title = "Plan to make Planner great - test plan";

        //    try
        //    {
        //        var plannerPlanFromResponse = await graphClient.Planner.Plans.Request().AddAsync(plannerPlan);


        //        Assert.IsNotNull(plannerPlanFromResponse);
        //        StringAssert.Contains(plannerPlan.Title, plannerPlanFromResponse.Title, "Created planner title does not match.");
        //        Assert.IsInstanceOfType(plannerPlanFromResponse, typeof(PlannerPlan));
        //    }
        //    catch (Microsoft.Graph.ServiceException e)
        //    {
        //        Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
        //    }
        //    catch (Exception e)
        //    {
        //        Assert.Fail($"An error occurred that probably wasn't caused by the service or client. Error message: {e.Message}");
        //    }
        //}

        // Not implemented yet 4/27
        // https://developer.microsoft.com/en-us/graph/docs/api-reference/beta/api/plannerplan_delete
        //[TestMethod]
        //public async System.Threading.Tasks.Task PlannerDeletePlan(string planId = "", string eTag = "")
        //{

        //}
    }
}
