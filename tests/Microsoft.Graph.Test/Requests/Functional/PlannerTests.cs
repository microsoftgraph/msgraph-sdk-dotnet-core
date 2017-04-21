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
        [TestMethod]
        public async System.Threading.Tasks.Task PlannerTaskCreate()
        {
            var superGroupId = "c3aa76a3-0499-4183-ab80-4e5882bfccc5";





        }


        [TestMethod]
        public async System.Threading.Tasks.Task PlannerCreatePlan2()
        {
            PlannerPlan plannerPlan = new PlannerPlan();

            try
            {
                // Create a group. There can only be a single planner plan per group.
                var clientOnlyGroup = new Group();
                clientOnlyGroup.Description = "A temporary group to that contains a planner plan that we'll test.";
                clientOnlyGroup.DisplayName = "Test group";
                clientOnlyGroup.GroupTypes = new List<string>() { "Unified" };
                clientOnlyGroup.MailEnabled = true;
                clientOnlyGroup.MailNickname = "BobTestGroup";
                clientOnlyGroup.SecurityEnabled = false;

                // Call Graph service API to create the new group.
                var syncdGroup = await graphClient.Groups.Request().AddAsync(clientOnlyGroup);

                // Create a planner plan on the group we created.
                plannerPlan.Owner = syncdGroup.Id;
                plannerPlan.Title = "Plan to make Planner great - test plan";

                // Call the Graph service API to add the planner plan to the group. Get a planner plan back.
                //var plannerPlanFromResponse = await graphClient.Groups[syncdGroup.Id].Planner.Plans.Request().AddAsync(plannerPlan);



                Plan plan = new Plan();
                plan.Owner = syncdGroup.Id;
                plan.Title = "Plan to make Planner great - test plan";

                await graphClient.Groups[syncdGroup.Id].Plans.References.Request().AddAsync(plan);

                //var plannerPlanFromResponse = await graphClient.Planner.Plans.Request().AddAsync(plannerPlan);

                //Assert.IsNotNull(plannerPlanFromResponse);
                //StringAssert.Contains(plannerPlan.Title, plannerPlanFromResponse.Title, "Created planner title does not match.");
                //Assert.IsInstanceOfType(plannerPlanFromResponse, typeof(PlannerPlan));
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
            }
            catch (Exception e)
            {
                Assert.Fail($"An error occurred that probably wasn't caused by the service or client. Error message: {e.Message}");
            }
        }


        // Call this before each planner test. 
        // https://developer.microsoft.com/en-us/graph/docs/api-reference/beta/api/planner_post_plans
        [TestMethod]
        public async System.Threading.Tasks.Task PlannerCreatePlan()
        {

            // Create a planner plan on the group we created.
            var onlineMarketingGroupId = "69cbc5f6-8269-423a-8a24-64a85c281765";
            PlannerPlan plannerPlan = new PlannerPlan();
            plannerPlan.Owner = onlineMarketingGroupId;
            plannerPlan.Title = "Plan to make Planner great - test plan";

            try
            {
                var plannerPlanFromResponse = await graphClient.Planner.Plans.Request().AddAsync(plannerPlan);


                Assert.IsNotNull(plannerPlanFromResponse);
                StringAssert.Contains(plannerPlan.Title, plannerPlanFromResponse.Title, "Created planner title does not match.");
                Assert.IsInstanceOfType(plannerPlanFromResponse, typeof(PlannerPlan));
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
            }
            catch (Exception e)
            {
                Assert.Fail($"An error occurred that probably wasn't caused by the service or client. Error message: {e.Message}");
            }
        }

        // Call this after each planner test. We need to delete the group and planner plan.
        // https://developer.microsoft.com/en-us/graph/docs/api-reference/beta/api/plannerplan_delete
        [TestMethod]
        public async System.Threading.Tasks.Task PlannerDeletePlan(string planId = "", string eTag = "")
        {

        }


        [TestMethod]
        public async System.Threading.Tasks.Task PlannerGetPlannerPlan()
        {
            try
            {
                // Get a group id that we know contains a plan.
                var onlineMarketingGroupId = "69cbc5f6-8269-423a-8a24-64a85c281765";
                var onlineMarketingPlanId = "HJLUP2ZwhE6-Gd0Sp3UMB2QAHsEe";

                var plan = await graphClient.Planner.Plans[onlineMarketingPlanId].Request().GetAsync();

                Assert.IsNotNull(plan);
                Assert.IsInstanceOfType(plan, typeof(PlannerPlan));
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
            }
        }

        //[TestMethod]
        //public async System.Threading.Tasks.Task PlannerGetPlannerTasks()
        //{
        //    try
        //    {
        //        // Get a group id that we know contains a plan.
        //        var onlineMarketingGroupId = "69cbc5f6-8269-423a-8a24-64a85c281765";
        //        var onlineMarketingPlanId = "HJLUP2ZwhE6-Gd0Sp3UMB2QAHsEe";

        //        var plan = await graphClient.Planner.Plans[onlineMarketingPlanId].PlannerTasks.Request().GetAsync();

        //        //Assert.IsNotNull(plan);
        //        //Assert.IsInstanceOfType(plan, typeof(PlannerPlan));
        //    }
        //    catch (Microsoft.Graph.ServiceException e)
        //    {
        //        Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
        //    }
        //}


    }
}
