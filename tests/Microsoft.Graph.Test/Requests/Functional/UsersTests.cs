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
        // Currently (10/5/2016), you can only set the mailboxsettings directly on the property, 
        // not with a patched user. Opened issue against service API.
        [TestMethod]
        public async Task UserGetSetAutomaticReply()
        {
            try
            {
                var query = new List<Option>()
                {
                    new QueryOption("$select", "mailboxsettings")
                };

                var user = await graphClient.Me.Request(query).GetAsync();

                await graphClient.Me.Request().UpdateAsync(user);
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Inconclusive("The service doesn't yet support PATCH on entity with mailboxsettings: {0}", e.Error.Code);
            }

            /* Notes
             * 
             * GET https://graph.microsoft.com/v1.0/me?$select=mailboxsettings 
             * 
             * RESPONSE
             * 
             * {
                    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users(mailboxSettings)/$entity",
                    "id": "c8616fa2-6a9e-4196-9912-e7fbea37fbd1@d0b7ccde-8426-4e94-a77b-a53e1bcd66c6",
                    "mailboxSettings": {
                        "automaticRepliesSetting": {
                            "status": "alwaysEnabled",
                            "externalAudience": "all",
                            "scheduledStartDateTime": {
                                "dateTime": "2016-09-30T21:00:00.0000000",
                                "timeZone": "UTC"
                            },
                            "scheduledEndDateTime": {
                                "dateTime": "2016-10-01T21:00:00.0000000",
                                "timeZone": "UTC"
                            },
                            "internalReplyMessage": "<html>\n<body>\nI am currently on vacation.\n</body>\n</html>\n",
                            "externalReplyMessage": ""
                        },
                        "timeZone": "Pacific Standard Time",
                        "language": {
                            "locale": "en-US",
                            "displayName": "English (United States)"
                        }
                    }
                }
             * GET https://graph.microsoft.com/v1.0/me/mailboxsettings
             * 
             * RESPONSE
             * 
             * {
                    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('c8616fa2-6a9e-4196-9912-e7fbea37fbd1')/mailboxSettings",
                    "automaticRepliesSetting": {
                        "status": "alwaysEnabled",
                        "externalAudience": "all",
                        "scheduledStartDateTime": {
                            "dateTime": "2016-09-30T21:00:00.0000000",
                            "timeZone": "UTC"
                        },
                        "scheduledEndDateTime": {
                            "dateTime": "2016-10-01T21:00:00.0000000",
                            "timeZone": "UTC"
                        },
                        "internalReplyMessage": "<html>\n<body>\nI am currently on vacation. Sorry :(\n</body>\n</html>\n",
                        "externalReplyMessage": ""
                    },
                    "timeZone": "Pacific Standard Time",
                    "language": {
                        "locale": "en-US",
                        "displayName": "English (United States)"
                    }
                }
             * This PATCH is successful
             * PATCH https://graph.microsoft.com/v1.0/me/mailboxsettings
             * 
             * {
                    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('c8616fa2-6a9e-4196-9912-e7fbea37fbd1')/mailboxSettings",
                    "automaticRepliesSetting": {
                        "status": "alwaysEnabled",
                        "externalAudience": "all",
                        "scheduledStartDateTime": {
                            "dateTime": "2016-09-30T21:00:00.0000000",
                            "timeZone": "UTC"
                        },
                        "scheduledEndDateTime": {
                            "dateTime": "2016-10-01T21:00:00.0000000",
                            "timeZone": "UTC"
                        },
                        "internalReplyMessage": "<html>\n<body>\nI am currently on vacation. Sorry :(\n</body>\n</html>\n",
                        "externalReplyMessage": ""
                    },
                    "timeZone": "Pacific Standard Time",
                    "language": {
                        "locale": "en-US"
                    }
                }
             * This PATCH is unsuccessful
             * PATCH https://graph.microsoft.com/v1.0/me
             * 
             * {
             *      "id": "c8616fa2-6a9e-4196-9912-e7fbea37fbd1@d0b7ccde-8426-4e94-a77b-a53e1bcd66c6",
                    "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users(mailboxSettings)/$entity"
                    "mailboxSettings": {
                        "automaticRepliesSetting": {
                            "status": "alwaysEnabled",
                            "externalAudience": "all",
                            "scheduledStartDateTime": {
                                "dateTime": "10/03/2016 07:00:00",
                                "timeZone": "UTC"
                            },
                            "scheduledEndDateTime": {
                                "dateTime": "10/04/2016 07:00:00",
                                "timeZone": "UTC"
                            },
                            "internalReplyMessage": "<html>\n<body>\nI am currently on vacation. Sorry :(\n</body>\n</html>\n",
                            "externalReplyMessage": ""
                        },
                        "timeZone": "Pacific Standard Time",
                        "language": {
                            "locale": "en-US",
                            "displayName": "English (United States)"
                        }
                    },
                }
             */


        }

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
