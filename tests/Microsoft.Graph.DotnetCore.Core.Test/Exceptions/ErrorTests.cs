// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Exceptions
{
    using Microsoft.Kiota.Serialization.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Xunit;
    public class ErrorTests
    {
        /* The following response body is used in jsonErrorResponseBody
         {
            "error": {
                "code": "BadRequest",
                "message": "Resource not found for the segment 'mer'.",
                "innerError": {
                    "request-id": "a9acfc00-2b19-44b5-a2c6-6c329b4337b3",
                    "date": "2019-09-10T18:26:26",
                    "code": "inner-error-code"
                },
                "target": "target-value",
                "unexpected-property": "unexpected-property-value",
                "details": [
                    {
                        "code": "details-code-value",
                        "message": "details",
                        "target": "details-target-value",
                        "unexpected-details-property": "unexpected-details-property-value"
                    },
                    {
                        "code": "details-code-value2"
                    }
                ]
            }
        }
        */
        // Use https://www.minifyjson.org/ if you need minify or beautify as part of an update.
        private const string jsonErrorResponseBody = "{\"error\":{\"code\":\"BadRequest\",\"message\":\"Resource not found for the segment 'mer'.\",\"innerError\":{\"request-id\":\"a9acfc00-2b19-44b5-a2c6-6c329b4337b3\",\"date\":\"2019-09-10T18:26:26\",\"code\":\"inner-error-code\"},\"target\":\"target-value\",\"unexpected-property\":\"unexpected-property-value\",\"details\":[{\"code\":\"details-code-value\",\"message\":\"details\",\"target\":\"details-target-value\",\"unexpected-details-property\":\"unexpected-details-property-value\"},{\"code\":\"details-code-value2\"}]}}";
        private readonly JsonParseNodeFactory _jsonParseNodeFactory;
        public ErrorTests()
        {
            _jsonParseNodeFactory = new JsonParseNodeFactory();
        }

        [Fact]
        public void VerifyToString()
        {
            var details = new List<ErrorDetail>();
            details.Add(new ErrorDetail()
            {
                Code = "errorDetailCode",
                Message = "errorDetailMessage",
                Target = "errorTarget"
            });

            var additionalData = new Dictionary<string, object>()
            {
                { "key", "value" }
            };

            var error = new Error
            {
                Code = "code",
                Message = "message",
                Target = "target",
                Details = details,
                InnerError = new Error
                {
                    Code = "innerCode",
                },
                ThrowSite = "throwSite",
                ClientRequestId = "clientRequestId",
                AdditionalData = additionalData
            };

            var errorStringBuilder = new StringBuilder();
            errorStringBuilder.Append("Code: code");
            errorStringBuilder.Append(Environment.NewLine);
            errorStringBuilder.Append("Message: message");
            errorStringBuilder.Append(Environment.NewLine);
            errorStringBuilder.Append("Target: target");
            errorStringBuilder.Append(Environment.NewLine);
            errorStringBuilder.Append("Details:");
            errorStringBuilder.Append(Environment.NewLine);
            errorStringBuilder.Append("\tDetail0:");
            errorStringBuilder.Append(Environment.NewLine);
            errorStringBuilder.Append("\t\tCode: errorDetailCode");
            errorStringBuilder.Append(Environment.NewLine);
            errorStringBuilder.Append("\t\tMessage: errorDetailMessage");
            errorStringBuilder.Append(Environment.NewLine);
            errorStringBuilder.Append("\t\tTarget: errorTarget");
            errorStringBuilder.Append(Environment.NewLine);
            errorStringBuilder.Append(Environment.NewLine);
            errorStringBuilder.Append("Inner error:");
            errorStringBuilder.Append(Environment.NewLine);
            errorStringBuilder.Append("\tCode: innerCode");
            errorStringBuilder.Append(Environment.NewLine);
            errorStringBuilder.Append("Throw site: throwSite");
            errorStringBuilder.Append(Environment.NewLine);
            errorStringBuilder.Append("ClientRequestId: clientRequestId");
            errorStringBuilder.Append(Environment.NewLine);
            errorStringBuilder.Append("AdditionalData:");
            errorStringBuilder.Append(Environment.NewLine);
            errorStringBuilder.Append("\tkey: value");
            errorStringBuilder.Append(Environment.NewLine);

            Assert.Equal(errorStringBuilder.ToString(), error.ToString());
        }

        [Fact]
        public void Validate_ErrorObjectDeserializes()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonErrorResponseBody));
            var rootNode = _jsonParseNodeFactory.GetRootParseNode(CoreConstants.MimeTypeNames.Application.Json, stream);
            Error error = rootNode.GetObjectValue<ErrorResponse>().Error;

            Assert.NotNull(error);
            Assert.Equal("BadRequest", error.Code);
            Assert.Equal("Resource not found for the segment 'mer'.", error.Message);
            Assert.NotNull(error.InnerError);
            Assert.Equal("a9acfc00-2b19-44b5-a2c6-6c329b4337b3", error.InnerError.AdditionalData["request-id"].ToString());
            Assert.Equal(DateTime.Parse("2019-09-10T18:26:26"), error.InnerError.AdditionalData["date"]);
            Assert.Equal("inner-error-code", error.InnerError.Code);
            Assert.Equal("target-value", error.Target);
            Assert.NotNull(error.AdditionalData);
            Assert.Equal("unexpected-property-value", error.AdditionalData["unexpected-property"].ToString());
            Assert.NotNull(error.Details);
            Assert.Collection<ErrorDetail>(error.Details, errorDetail => Assert.Equal("details-code-value", errorDetail.Code),
                                                          errorDetail => Assert.Equal("details-code-value2", errorDetail.Code));
            Assert.Equal("unexpected-details-property-value", error.Details.ToList<ErrorDetail>()[0].AdditionalData["unexpected-details-property"].ToString());
        }

        [Fact]
        public void VerifyToStringDoesNotThrowNullReferenceExceptionOnNullErrorProperty()
        {
            var errorResponseWithNullException = "{\r\n" +
                                                 "            \"error\": {\r\n" +
                                                 "                \"code\": \"BadRequest\",\r\n" +
                                                 "                \"message\": \"Resource not found for the segment 'mer'.\",\r\n" +
                                                 "                \"innerError\": {\r\n" +
                                                 "                    \"request-id\": \"a9acfc00-2b19-44b5-a2c6-6c329b4337b3\",\r\n" +
                                                 "                    \"date\": \"2019-09-10T18:26:26\",\r\n" +
                                                 "                    \"code\": \"inner-error-code\",\r\n" +
                                                 "                    \"exception\": null\r\n" +                    // Exception property present but set to null
                                                 "                },\r\n" +
                                                 "                \"target\": \"target-value\",\r\n" +
                                                 "                \"unexpected-property\": \"unexpected-property-value\"\r\n" +
                                                 "            }\r\n" +
                                                 "        }";

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(errorResponseWithNullException));
            var rootNode = _jsonParseNodeFactory.GetRootParseNode(CoreConstants.MimeTypeNames.Application.Json, stream);
            Error error = rootNode.GetObjectValue<ErrorResponse>().Error;

            // Assert we have deserialized the properties as expected.
            Assert.NotNull(error);
            Assert.Equal("BadRequest", error.Code);
            Assert.Equal("Resource not found for the segment 'mer'.", error.Message);
            Assert.NotNull(error.InnerError);
            Assert.Equal("a9acfc00-2b19-44b5-a2c6-6c329b4337b3", error.InnerError.AdditionalData["request-id"].ToString());
            Assert.Equal(DateTime.Parse("2019-09-10T18:26:26"), error.InnerError.AdditionalData["date"]);
            Assert.Null(error.InnerError.AdditionalData["exception"]);                  // Assert the property is null
            Assert.Equal("inner-error-code", error.InnerError.Code);
            Assert.Equal("target-value", error.Target);
            Assert.NotNull(error.AdditionalData);
            Assert.Equal("unexpected-property-value", error.AdditionalData["unexpected-property"].ToString());

            var errorString = error.ToString();

            // Assert we can create the string representation
            Assert.Contains("exception: null", errorString);
        }
    }
}
