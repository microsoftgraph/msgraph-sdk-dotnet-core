// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Tasks
{
    using Microsoft.Graph.DotnetCore.Core.Test.Requests;
    using Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Xunit;

    public class LargeFileUploadTests : RequestTestBase
    {
        [Fact]
        public void ThrowsArgumentExceptionOnInvalidSliceSize()
        {
            using (Stream stream = new MemoryStream())
            {
                // Arrange
                IUploadSession uploadSession = new Graph.Core.Models.UploadSession
                {
                    NextExpectedRanges = new List<string>() { "0-" },
                    UploadUrl = "http://localhost",
                    ExpirationDateTime = DateTimeOffset.Parse("2019-11-07T06:39:31.499Z")
                };

                int maxSliceSize = 1000;//invalid slice size that is not a multiple of 320

                // Act 
                var exception = Assert.Throws<ArgumentException>(() => new LargeFileUploadTask<TestDriveItem>(uploadSession, stream, maxSliceSize));

                // Assert
                Assert.Equal("maxSliceSize", exception.ParamName);
            }
        }

        [Fact]
        public void ShouldNotThrowArgumentExceptionOnConstructorWithoutSliceSize()
        {
            // Try to upload 1Mb stream without specifying the slice size(should default to 5Mb)
            byte[] mockData = new byte[1000000];
            using (Stream stream = new MemoryStream(mockData))
            {
                // Arrange
                IUploadSession uploadSession = new Graph.Core.Models.UploadSession
                {
                    NextExpectedRanges = new List<string>() { "0-" },
                    UploadUrl = "http://localhost",
                    ExpirationDateTime = DateTimeOffset.Parse("2019-11-07T06:39:31.499Z")
                };

                // Act with constructor without chunk length
                var fileUploadTask = new LargeFileUploadTask<TestDriveItem>(uploadSession, stream);
                var uploadSlices = fileUploadTask.GetUploadSliceRequests();

                // Assert
                //We have only 1 slices
                Assert.Single(uploadSlices);

                var onlyUploadSlice = uploadSlices.First();
                Assert.Equal(stream.Length, onlyUploadSlice.TotalSessionLength);
                Assert.Equal(0, onlyUploadSlice.RangeBegin);
                Assert.Equal(stream.Length - 1, onlyUploadSlice.RangeEnd);
                Assert.Equal(stream.Length , onlyUploadSlice.RangeLength); //verify the last slice is the right size
            }
        }

        [Fact]
        public void BreaksDownStreamIntoRangesCorrectly()
        {
            byte[] mockData = new byte[1000000];//create a stream of about 1M so we can split it into a few 320K slices
            using (Stream stream = new MemoryStream(mockData))
            {
                // Arrange
                IUploadSession uploadSession = new Graph.Core.Models.UploadSession
                {
                    NextExpectedRanges = new List<string>() { "0-" },
                    UploadUrl = "http://localhost",
                    ExpirationDateTime = DateTimeOffset.Parse("2019-11-07T06:39:31.499Z")
                };

                int maxSliceSize = 320 * 1024;

                // Act 
                var fileUploadTask = new LargeFileUploadTask<TestDriveItem>(uploadSession, stream, maxSliceSize);
                var uploadSlices = fileUploadTask.GetUploadSliceRequests();

                // Assert
                //We have only 4 slices
                Assert.Equal(4, uploadSlices.Count());

                long currentRangeBegins = 0;
                foreach (var uploadSlice in uploadSlices)
                {
                    Assert.Equal(stream.Length, uploadSlice.TotalSessionLength);
                    Assert.Equal(currentRangeBegins, uploadSlice.RangeBegin);
                    currentRangeBegins += maxSliceSize;
                }

                //The last slice is a a bit smaller than the rest
                var lastUploadSlice = uploadSlices.Last();
                Assert.Equal(stream.Length - 1, lastUploadSlice.RangeEnd);
                Assert.Equal(stream.Length % maxSliceSize, lastUploadSlice.RangeLength); //verify the last slice is the right size
            }
        }
    }
}
