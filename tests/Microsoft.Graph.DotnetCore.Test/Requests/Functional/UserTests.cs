// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Graph.DotnetCore.Test.Requests.Functional
{
    public class UserTests : GraphTestBase
    {


        [Fact(Skip = "No CI set up for functional tests - add email addresses to run this test.")]
        public async System.Threading.Tasks.Task UserGetMailtipsTestEnumFlags()
        {
            try
            {
                var emailAddresses = new List<string>();
                emailAddresses.Add("11111@aaaaaaa.onmicrosoft.com");
                emailAddresses.Add("22222@aaaaaaa.onmicrosoft.com");
                emailAddresses.Add("33333@aaaaaaa.onmicrosoft.com");

                var mailTipsCollectionPage = await graphClient.Me.GetMailTips(emailAddresses, MailTipsType.AutomaticReplies | 
                                                                                              MailTipsType.CustomMailTip | 
                                                                                              MailTipsType.MaxMessageSize | 
                                                                                              MailTipsType.RecipientScope |
                                                                                              MailTipsType.TotalMemberCount).Request().PostAsync();

                foreach (var mt in mailTipsCollectionPage)
                {
                    // All of the supplied users should have an email address.
                    Assert.NotNull(mt.EmailAddress);
                }
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.True(false, "Something happened, check out a trace. Error code: " + e.Error.Code);
            }
        }

    }
}
