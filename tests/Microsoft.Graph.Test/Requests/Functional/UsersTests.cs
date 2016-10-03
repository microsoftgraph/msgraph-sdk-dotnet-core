// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Graph.Test.Requests.Functional
{
    [Ignore]
    [TestClass]
    public class UsersTests : GraphTestBase
    {
        // Filter on displayname
        // https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/41
        [TestMethod]
        public async Task UserFilterStartsWith()
        {
            try
            {
                var usersQuery = await graphClient.Users.Request().Filter("startswith(displayName,'A')").GetAsync();
                foreach (User u in usersQuery)
                {
                    StringAssert.StartsWith(u.DisplayName, "A", "Expected a display name that started with the letter A.");
                }
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
            }
        }

        // Get the test user's photo.
        [TestMethod]
        public async Task UserGetPhoto()
        {
            try
            {
                // Gets the user's photo.
                // http://graph.microsoft.io/en-us/docs/api-reference/v1.0/api/profilephoto_get
                // GET https://graph.microsoft.com/v1.0/me/photo/$value
                var originalPhoto = await graphClient.Me.Photo.Content.Request().GetAsync();

                Assert.IsNotNull(originalPhoto, "The photo value is null");
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                if (e.Error.Code == "ErrorItemNotFound")
                {
                    Assert.Fail("We didn't get a photo back from the service. Check that the target account has a photo.");
                }
                else
                {
                    Assert.Fail("Something happened. Catch the HTTP traffic and find out what happened.");
                }
            }
        }

        // Update the test user's photo
        [TestMethod]
        public async Task UserUpdatePhoto()
        {
            try
            {
                System.Drawing.ImageConverter converter = new System.Drawing.ImageConverter();
                var buff = (byte[])converter.ConvertTo(Microsoft.Graph.Test.Properties.Resources.hamilton, typeof(byte[]));
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(buff))
                {
                    // Sets the user's photo.
                    // http://graph.microsoft.io/en-us/docs/api-reference/v1.0/api/profilephoto_update
                    // PUT https://graph.microsoft.com/v1.0/me/photo/$value
                    await graphClient.Me.Photo.Content.Request().PutAsync(ms);
                }
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
            }
        }

        // Get the test user.
        [TestMethod]
        public async Task UserGetUser()
        {
            try
            {
                var user = await graphClient.Me.Request().GetAsync();
                Assert.IsNotNull(user.UserPrincipalName, "User principal name is not set.");
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened. Error code: {0}", e.Error.Code);
            }
        }

        [TestMethod]
        public async Task UserGetManager()
        {
            try
            {
                var managerDirObj = (User)await graphClient.Me.Manager.Request().GetAsync();

                Assert.IsNotNull(managerDirObj, "The manager wasn't returned.");
                Assert.IsFalse(managerDirObj.DisplayName == "", "The display name of the user's manager is not set.");
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened. Error code: {0}", e.Error.Code);
            }
        }

        // PUT https://graph.microsoft.com/v1.0/me/manager/$ref
        // {    "@odata.id": "https://graph.microsoft.com/v1.0/users/55aa3346-08cb-4e98-8567-879b039a72c1" }
        // http://graph.microsoft.io/en-us/docs/api-reference/v1.0/api/user_post_manager
        // We are getting and setting the user's manager.
        [TestMethod]
        public async Task UserUpdateManager()
        {
            try
            {
                var managerDirObj = (User)await graphClient.Me.Manager.Request().GetAsync();

                await graphClient.Me.Manager.Reference.Request().PutAsync(managerDirObj.Id);
                Assert.IsNotNull(managerDirObj, "The manager wasn't returned.");
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened. Error code: {0}", e.Error.Code);
            }
        }

        [TestMethod]
        public async Task UserAssignLicense()
        {
            try
            {
                // This is expected to fail since we aren't providing licenses.
                var user = await graphClient.Me.AssignLicense(new List<AssignedLicense>(), new List<System.Guid>()).Request().PostAsync();
                Assert.IsNull(user, "Expected that the request would cause a ServiceException. Last assumption is that setting an empty collection of licenses causes an error."); 

            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.AreEqual("Request_BadRequest", e.Error.Code, "Expected error: Request_BadRequest, actual error: {0}", e.Error.Code);
            }
        }

        /// <summary>
        /// Tests building a request for an action with a required parameter that's set.
        /// Tests the get member groups action with a set parameter.
        /// </summary>
        [TestMethod]
        public async Task UserGetMemberGroups_SecurityEnabledOnly_ValueSet()
        {
            try
            {
                var getMemberGroupsRequest = graphClient.Me
                                                        .GetMemberGroups(true)
                                                        .Request() as DirectoryObjectGetMemberGroupsRequest;

                var directoryObjectGetMemberGroupsCollectionPage = await getMemberGroupsRequest.PostAsync();

                Assert.IsNotNull(directoryObjectGetMemberGroupsCollectionPage, "Unexpected results.");
                Assert.AreEqual("POST", getMemberGroupsRequest.Method, "Unexpected HTTP method");
                Assert.IsTrue(getMemberGroupsRequest.RequestBody.SecurityEnabledOnly.Value, "Unexpected value for SecurityEnabledOnly in request body.");
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened. Error code: {0}", e.Error.Code);
            }
        }

        /// <summary>
        /// Tests building a request for an action with a required parameter set to null.
        /// Tests the get member groups action without a set parameter, default is null.
        /// </summary>
        [TestMethod]
        public async Task UserGetMemberGroups_SecurityEnabledOnly_ValueNotSet()
        {
            try
            {
                var getMemberGroupsRequest = graphClient.Me
                                                        .GetMemberGroups()
                                                        .Request() as DirectoryObjectGetMemberGroupsRequest;

                var directoryObjectGetMemberGroupsCollectionPage = await getMemberGroupsRequest.PostAsync();
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.AreEqual("Request_BadRequest", e.Error.Code, "Unexpected error occurred.");
            }
        }

        [TestMethod]
        // Addressing https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/28
        public async Task UpdateUser()
        {
            try
            {
                var me = await graphClient.Me.Request().GetAsync();

                var betterMe = new User()
                {
                    GivenName = "Beth"
                };

                // Update the user to Beth
                await graphClient.Users[me.UserPrincipalName].Request().UpdateAsync(betterMe);

                // Update the user back to me.
                await graphClient.Users[me.UserPrincipalName].Request().UpdateAsync(me);

            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened. Error code: {0}", e.Error.Code);
            }
        }
    }
}
