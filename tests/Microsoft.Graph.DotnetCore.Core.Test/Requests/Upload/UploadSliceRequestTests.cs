// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Graph.DotnetCore.Core.Test.Mocks;
    using Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels;
    using Xunit;

    public class UploadSliceRequests : RequestTestBase
    {
        [Fact]
        public async Task PutAsyncReturnsExpectedUploadSessionAsync()
        {
            using (HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.OK))
            using (TestHttpMessageHandler testHttpMessageHandler = new TestHttpMessageHandler())
            {
                /* Arrange */
                // 1. create a mock response
                string requestUrl = "https://localhost/";
                string responseJSON = @"{
                  ""expirationDateTime"": ""2015 - 01 - 29T09: 21:55.523Z"",
                  ""nextExpectedRanges"": [
                  ""12345-55232"",
                  ""77829-99375""
                  ]
                }";
                HttpContent content = new StringContent(responseJSON, Encoding.UTF8, CoreConstants.MimeTypeNames.Application.Json);
                responseMessage.Content = content;

                // 2. Map the response
                testHttpMessageHandler.AddResponseMapping(requestUrl, responseMessage);

                // 3. Create a batch request object to be tested
                MockCustomHttpProvider customHttpProvider = new MockCustomHttpProvider(testHttpMessageHandler);
                BaseClient client = new BaseClient(requestUrl, authenticationProvider.Object, customHttpProvider);
                UploadSliceRequest<TestDriveItem> uploadSliceRequest = new UploadSliceRequest<TestDriveItem>(requestUrl, client, 0, 200, 1000);
                Stream stream = new MemoryStream(new byte[300]);

                /* Act */
                var uploadResult = await uploadSliceRequest.PutAsync(stream);
                var uploadSession = uploadResult.UploadSession;

                /* Assert */
                Assert.False(uploadResult.UploadSucceeded);
                Assert.NotNull(uploadSession);
                Assert.Null(uploadSession.UploadUrl);
                Assert.Equal(DateTimeOffset.Parse("2015 - 01 - 29T09: 21:55.523Z"), uploadSession.ExpirationDateTime);
                Assert.Equal("12345-55232", uploadSession.NextExpectedRanges.First());
                Assert.Equal("77829-99375", uploadSession.NextExpectedRanges.Last());
                Assert.Equal(2, uploadSession.NextExpectedRanges.Count());
            }
        }
    }
}