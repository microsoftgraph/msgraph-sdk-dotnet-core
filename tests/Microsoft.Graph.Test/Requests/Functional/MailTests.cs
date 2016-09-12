using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Graph.Test.Requests.Functional
{
    [Ignore]
    [TestClass]
    public class MailTests : GraphTestBase
    {
        // Tests the SendMail action.
        [TestMethod]
        public async Task MailSendMail()
        {
            try
            {
                // Get the test user.
                var me = await graphClient.Me.Request().GetAsync();

                var subject = DateTime.Now.ToString();

                var message = new Message();
                message.Subject = subject;
                message.Body = new ItemBody() { Content = "This is the body" };
                var recipients = new List<Recipient>()
                {
                    new Recipient()
                    {
                        EmailAddress = new EmailAddress()
                        {
                            Address = me.Mail
                        }   
                    }
                };

                message.ToRecipients = recipients;

                // Send email to the test user.
                await graphClient.Me.SendMail(message, true).Request().PostAsync();

                var query = new List<Option>()
                {
                    new QueryOption("filter", "Subject eq '" + subject + "'")
                };

                // Check the we found the sent email in the sent items folder.
                var mailFolderMessagesCollectionPage = await graphClient.Me.MailFolders["sentitems"].Messages.Request(query).GetAsync();

                Assert.IsNotNull(mailFolderMessagesCollectionPage, "Unexpected results, the results contains a null collection.");
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
            }
        }
    }
}
