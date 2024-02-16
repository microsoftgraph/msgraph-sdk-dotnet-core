// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Exceptions
{
    using System;
    using System.Text.Json;
    using Xunit;
    public class ServiceExceptionTests
    {
        [Fact]
        public void IsMatch_ErrorCodeRequired()
        {
            var serviceException = new ServiceException("errorCode");

            Assert.Throws<ArgumentException>(() => serviceException.IsMatch(null));
        }

        [Fact]
        public void IsMatch_NestedErrors()
        {
            var responsebody = JsonSerializer.Serialize(new
            {
                Code = "errorCode",
                InnerError = new
                {
                    Code = "differentErrorCode",
                    InnerError = new
                    {
                        Code = "errorCodeMatch",
                    }
                }
            });
            var serviceException = new ServiceException("errorCode",null ,0,responsebody);

            Assert.True(serviceException.IsMatch("errorcodematch"));
        }

        [Fact]
        public void IsMatch_NoMatch()
        {
            var responsebody = JsonSerializer.Serialize(new
            {
                Code = "errorCode",
                InnerError = new 
                {
                    Code = "differentErrorCode",
                    InnerError = new
                    {
                        Code = "errorCodeMatch",
                    }
                }
            });
            var serviceException = new ServiceException("errorCode",null ,0,responsebody);

            Assert.False(serviceException.IsMatch("noMatch"));
        }
        
        [Fact(Skip = "Changed the signature of ServiceException.ToString() in ccecc486cce5769313c0cb59ab56142d1b43ce5a")]
        public void ToString_ErrorNull()
        {
            var serviceException = new ServiceException(null);

            Assert.Null(serviceException.ToString());
        }

        [Fact]
        public void IsMatch_ThrowsNoException() 
        {
            var serviceException = new ServiceException(null);

            bool result = serviceException.IsMatch("Any Error");
            
            Assert.False(result);
        }
    }
}
