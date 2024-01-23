// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Microsoft.Kiota.Http.HttpClientLibrary.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Graph.DotnetCore.Core.Test.Helpers
{
    public class ReadOnlySubStreamTests
    {
        private readonly Stream _baseStream;
        private readonly List<string> _segments = new List<string>() 
        {
            "1234567890","0987654321","1357924680","2468013579"
        };

        public ReadOnlySubStreamTests()
        {
            _baseStream = new MemoryStream();
            var writer = new StreamWriter(_baseStream);
            foreach (var segment in _segments)
            {
                writer.Write(segment);
            }
            writer.Flush();
            _baseStream.Position = 0;
        }

        [Fact]
        public void SubstreamsAreReadableOnce()
        {
            long startIndex = 0;
            foreach (var segment in _segments)
            {
                using var substream = new ReadOnlySubStream(_baseStream, startIndex, 10);
                startIndex += substream.Length;
                using var streamReader = new StreamReader(substream);
                var readBytes = streamReader.ReadToEnd();
                Assert.Equal(segment, readBytes);
            }
        }

        [Fact]
        public void SubstreamsAreReadableMultipleTimes()
        {
            long startIndex = 0;
            foreach (var segment in _segments)
            {
                using var substream = new ReadOnlySubStream(_baseStream, startIndex, 10);
                startIndex += substream.Length;
                using var streamReader = new StreamReader(substream);

                var readBytes = streamReader.ReadToEnd();
                Assert.Equal(segment, readBytes);

                // reset stream and read again
                substream.Position = 0;
                readBytes = streamReader.ReadToEnd();
                Assert.Equal(segment, readBytes);
            }
        }

        [Fact]
        public void SubstreamsAreReadableFromDifferentPositionsTimes()
        {
            long startIndex = 0;
            foreach (var segment in _segments)
            {
                using var substream = new ReadOnlySubStream(_baseStream, startIndex, 10);
                startIndex += substream.Length;
                using var streamReader = new StreamReader(substream);

                var readBytes = streamReader.ReadToEnd();
                Assert.Equal(segment, readBytes);

                // reset stream to middle and read again
                substream.Seek(5,SeekOrigin.Begin);
                readBytes = streamReader.ReadToEnd();
                Assert.Equal(segment[5..], readBytes);

                // reset stream and read again
                substream.Position = 0;
                readBytes = streamReader.ReadToEnd();
                Assert.Equal(segment, readBytes);
            }
        }
    }
}
