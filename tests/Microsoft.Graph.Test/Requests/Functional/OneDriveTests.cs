using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Graph.Test.Requests.Functional
{
    [Ignore]
    [TestClass]
    public class OneDriveTests : GraphTestBase
    {
        // https://github.com/OneDrive/onedrive-sdk-csharp/blob/master/docs/chunked-uploads.md
        // https://dev.onedrive.com/items/upload_large_files.htm
        [TestMethod]
        public async Task OneDriveUploadLargeFile()
        {
            try
            {
                System.Drawing.ImageConverter converter = new System.Drawing.ImageConverter();
                var buff = (byte[])converter.ConvertTo(Microsoft.Graph.Test.Properties.Resources.hamilton, typeof(byte[]));
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(buff))
                {
                    // Describe the file to upload. Pass into CreateUploadSession, when the service works as expected.
                    //var props = new DriveItemUploadableProperties();
                    //props.Name = "_hamilton.png";
                    //props.Description = "This is a pictureof Mr. Hamilton.";
                    //props.FileSystemInfo = new FileSystemInfo();
                    //props.FileSystemInfo.CreatedDateTime = System.DateTimeOffset.Now;
                    //props.FileSystemInfo.LastModifiedDateTime = System.DateTimeOffset.Now;

                    // Get the provider. 
                    // POST /v1.0/drive/items/01KGPRHTV6Y2GOVW7725BZO354PWSELRRZ:/_hamiltion.png:/microsoft.graph.createUploadSession
                    // The CreateUploadSesssion action doesn't seem to support the options stated in the metadata.
                    var uploadSession = await graphClient.Drive.Items["01KGPRHTV6Y2GOVW7725BZO354PWSELRRZ"].ItemWithPath("_hamilton.png").CreateUploadSession().Request().PostAsync();

                    var maxChunkSize = 320 * 1024; // 320 KB - Change this to your chunk size. 5MB is the default.
                    var provider = new ChunkedUploadProvider(uploadSession, graphClient, ms, maxChunkSize);

                    // Setup the chunk request necessities
                    var chunkRequests = provider.GetUploadChunkRequests();
                    var readBuffer = new byte[maxChunkSize];
                    var trackedExceptions = new List<Exception>();
                    DriveItem itemResult = null;

                    //upload the chunks
                    foreach (var request in chunkRequests)
                    {
                        // Do your updates here: update progress bar, etc.
                        // ...
                        // Send chunk request
                        var result = await provider.GetChunkRequestResponseAsync(request, readBuffer, trackedExceptions);

                        if (result.UploadSucceeded)
                        {
                            itemResult = result.ItemResponse;
                        }
                    }

                    // Check that upload succeeded
                    if (itemResult == null)
                    {
                        // Retry the upload
                        // ...
                    }
                }
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
            }
        }


        [TestMethod]
        public async Task OneDriveNextPageRequest()
        {
            try
            {
                var driveItems = new List<DriveItem>();

                var driveItemsPage = await graphClient.Me.Drive.Root.Children.Request().Top(4).GetAsync();

                Assert.IsNotNull(driveItemsPage, "Expected that a page of OneDrive items is deserialized into an object.");

                driveItems.AddRange(driveItemsPage.CurrentPage);

                while (driveItemsPage.NextPageRequest != null)
                {
                    driveItemsPage = await driveItemsPage.NextPageRequest.GetAsync();
                    driveItems.AddRange(driveItemsPage.CurrentPage);
                }
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
            }
        }
    }
}
