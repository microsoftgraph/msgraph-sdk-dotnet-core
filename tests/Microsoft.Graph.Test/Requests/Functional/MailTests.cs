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
        [Ignore] // Setup Fiddler autoresponder 
        [TestMethod]
        public async Task MailGetGroupWithThrottlingError()
        {
            try
            {
                // All requests should have a client-request-id set so that the client can correlate a 
                // request with a response. 
                var headerOptions = new List<HeaderOption>()
                {
                    new HeaderOption("client-request-id", new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd").ToString())
                };

                // To get a throttling error, I mocked up a 429 response in a text file and turned on the Fiddler
                // autoresponder to return the text file as the response envelope. The autoresponder for this 
                // scenario responds to EXACT:https://graph.microsoft.com/v1.0/groups/036bd54c-c6e5-43eb-b8b5-03e019e75bd1
                var group = await graphClient.Groups["036bd54c-c6e5-43eb-b8b5-03e019e75bd1"].Request(headerOptions).GetAsync();
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                if ((int)e.StatusCode == 429) // Too Many Requests
                {
                    // We have the client-request-id for correlating the response to the request that failed.
                    IEnumerable<string> clientrequestidvalues;
                    Assert.IsTrue(e.ResponseHeaders.TryGetValues("client-request-id", out clientrequestidvalues));

                    // We have the Retry-After that the client can use to wait and resubmit the rejected request.
                    IEnumerable<string> retryaftervalues;
                    Assert.IsTrue(e.ResponseHeaders.TryGetValues("Retry-After", out retryaftervalues));
                }
                else
                {
                    Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
                }
            }
        }
        
        public async Task<Message> createEmail(string emailBody)
        {
            // Get the test user.
            var me = await graphClient.Me.Request().GetAsync();

            var subject = DateTime.Now.ToString();

            var message = new Message();
            message.Subject = subject;
            message.Body = new ItemBody() { Content = emailBody };
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

            return message;
        } 

        // Tests the SendMail action.
        [TestMethod]
        public async Task MailSendMail()
        {
            try
            {
                var message = await createEmail("Sent from the MailSendMail test.");

                // Send email to the test user.
                await graphClient.Me.SendMail(message, true).Request().PostAsync();

                var query = new List<Option>()
                {
                    new QueryOption("filter", "Subject eq '" + message.Subject + "'")
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

        // Test that we can set an attachment on a mail, send it, and then retrieve it.
        [TestMethod]
        public async Task MailSendMailWithFileAttachment()
        {
            try
            {
                var message = await createEmail("Sent from the MailSendMailWithAttachment test.");

                var attachment = new FileAttachment();
                attachment.ODataType = "#microsoft.graph.fileAttachment";
                attachment.Name = "MyFileAttachment.txt";
                attachment.ContentBytes = Microsoft.Graph.Test.Properties.Resources.textfile;

                message.Attachments = new MessageAttachmentsCollectionPage();
                message.Attachments.Add(attachment);

                await graphClient.Me.SendMail(message, true).Request().PostAsync();
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
            }
        }

        [TestMethod]
        public async Task MailGetMailWithFileAttachment()
        {
            try
            {
                // Find messages with attachments.
                var messageCollection = await graphClient.Me.Messages.Request()
                                                                     .Filter("hasAttachments eq true")
                                                                     .GetAsync();

                if (messageCollection.Count > 0)
                {
                    // Get information about attachments on the first message that has attachments.
                    var attachments = await graphClient.Me.Messages[messageCollection[0].Id]
                                                          .Attachments
                                                          .Request()
                                                          .GetAsync();

                    // Get an attachment.
                    var attachmment = await graphClient.Me.Messages[messageCollection[0].Id]
                                                          .Attachments[attachments[0].Id]
                                                          .Request()
                                                          .GetAsync();

                    if (attachmment is FileAttachment)
                        Assert.IsNotNull((attachmment as FileAttachment).ContentBytes, "The attachment doesn't contain expected content.");
                }
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
            }
        }


        [TestMethod]
        public async Task MailNextPageRequest()
        {
            try
            {
                var messages = new List<Message>();

                var messagePage = await graphClient.Me.Messages.Request().GetAsync();

                messages.AddRange(messagePage.CurrentPage);

                while (messagePage.NextPageRequest != null)
                {
                    messagePage = await messagePage.NextPageRequest.GetAsync();
                    messages.AddRange(messagePage.CurrentPage);
                }
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
            }
        }
    }
}
