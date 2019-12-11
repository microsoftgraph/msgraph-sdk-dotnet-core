// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests.Content
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using Xunit;
    using System.Threading.Tasks;

    public class BatchResponseContentTests
    {
        [Fact]
        public async Task BatchResponseContent_InitializeWithNoContentAsync()
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            BatchResponseContent batchResponseContent = new BatchResponseContent(httpResponseMessage);

            Dictionary<string, HttpResponseMessage> responses = await batchResponseContent.GetResponsesAsync();
            HttpResponseMessage httpResponse = await batchResponseContent.GetResponseByIdAsync("1");

            Assert.NotNull(responses);
            Assert.Null(httpResponse);
            Assert.NotNull(batchResponseContent.Serializer);
            Assert.True(responses.Count.Equals(0));
        }

        [Fact]
        public async Task BatchResponseContent_InitializeWithEmptyResponseContentAsync()
        {
            string jsonResponse = "{ responses: [] }";
            HttpContent content = new StringContent(jsonResponse);
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            httpResponseMessage.Content = content;

            BatchResponseContent batchResponseContent = new BatchResponseContent(httpResponseMessage);

            Dictionary<string, HttpResponseMessage> responses = await batchResponseContent.GetResponsesAsync();
            HttpResponseMessage httpResponse = await batchResponseContent.GetResponseByIdAsync("1");

            Assert.NotNull(responses);
            Assert.Null(httpResponse);
            Assert.NotNull(batchResponseContent.Serializer);
            Assert.True(responses.Count.Equals(0));
        }

        [Fact]
        public void BatchResponseContent_InitializeWithNullResponseMessage()
        {
            ClientException ex = Assert.Throws<ClientException>(() => new BatchResponseContent(null));

            Assert.Equal(ErrorConstants.Codes.InvalidArgument, ex.Error.Code);
            Assert.Equal(string.Format(ErrorConstants.Messages.NullParameter, "httpResponseMessage"), ex.Error.Message);
        }

        [Fact]
        public async Task BatchResponseContent_GetResponsesAsync()
        {
            string responseJSON = "{\"responses\":"
                +"[{"
                    +"\"id\": \"1\","
                    +"\"status\":200,"
                    + "\"headers\":{\"Cache-Control\":\"no-cache\",\"OData-Version\":\"4.0\",\"Content-Type\":\"application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=false;charset=utf-8\"},"
                    + "\"body\":{\"@odata.context\":\"https://graph.microsoft.com/v1.0/$metadata#users/$entity\",\"displayName\":\"MOD Administrator\",\"jobTitle\":null,\"id\":\"9f4fe8ea-7e6e-486e-b8f4-VkHdanfIomf\"}"
                + "},"
                +"{"
                    +"\"id\": \"2\","
                    +"\"status\":200,"
                    + "\"headers\":{\"Cache-Control\":\"no-store, no-cache\",\"OData-Version\":\"4.0\",\"Content-Type\":\"application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=false;charset=utf-8\"},"
                    + "\"body\":{\"@odata.context\":\"https://graph.microsoft.com/v1.0/$metadata#drives/$entity\",\"createdDateTime\":\"2019-01-12T09:05:38Z\",\"description\":\"\",\"id\":\"b!random-VkHdanfIomf\",\"lastModifiedDateTime\":\"2019-03-06T06:59:04Z\",\"name\":\"OneDrive\",\"webUrl\":\"https://m365x751487-my.sharepoint.com/personal/admin_m365x751487_onmicrosoft_com/Documents\",\"driveType\":\"business\",\"createdBy\":{\"user\":{\"displayName\":\"System Account\"}},\"lastModifiedBy\":{\"user\":{\"displayName\":\"System Account\"}},\"owner\":{\"user\":{\"email\":\"admin@M365x751487.OnMicrosoft.com\",\"id\":\"6b4fa8ea-7e6e-486e-a8f4-d00a5b23488c\",\"displayName\":\"MOD Administrator\"}},\"quota\":{\"deleted\":0,\"remaining\":1099509670098,\"state\":\"normal\",\"total\":1099511627776,\"used\":30324}}"
                + "},"
                +"{"
                    +"\"id\": \"3\","
                    +"\"status\":201,"
                    + "\"headers\":{\"Location\":\"https://graph.microsoft.com/v1.0/users/9f4fe8ea-7e6e-486e-a8f4-nothing-here/onenote/notebooks/1-zyz-a1c1-441a-8b41-9378jjdd2\",\"Preference-Applied\":\"odata.include-annotations=*\",\"Cache-Control\":\"no-cache\",\"OData-Version\":\"4.0\",\"Content-Type\":\"application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=false;charset=utf-8\"},"
                    + "\"body\":{\"@odata.context\":\"https://graph.microsoft.com/v1.0/$metadata#users('9f4fe8ea-7e6e-486e-a8f4-nothing-here')/onenote/notebooks/$entity\",\"id\":\"1-9f4fe8ea-7e6e-486e-a8f4-nothing-here\",\"self\":\"https://graph.microsoft.com/v1.0/users/9f4fe8ea-7e6e-486e-a8f4-nothing-here/onenote/notebooks/1-9f4fe8ea-7e6e-486e-a8f4-nothing-here\",\"createdDateTime\":\"2019-03-06T08:08:09Z\",\"displayName\":\"My Notebook -442293399\",\"lastModifiedDateTime\":\"2019-03-06T08:08:09Z\"}"
                + "}]}";

            HttpContent content = new StringContent(responseJSON);
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponseMessage.Content = content;

            BatchResponseContent batchResponseContent = new BatchResponseContent(httpResponseMessage);

            Dictionary<string, HttpResponseMessage> responses = await batchResponseContent.GetResponsesAsync();

            Assert.NotNull(responses);
            Assert.True(responses.Count.Equals(3));
            Assert.False(responses["1"].Headers.CacheControl.NoStore);
            Assert.True(responses["2"].Headers.CacheControl.NoCache);
            Assert.True(responses["2"].Headers.CacheControl.NoStore);
            Assert.Equal(HttpStatusCode.Created, responses["3"].StatusCode);
        }

        [Fact]
        public async Task BatchResponseContent_GetResponseByIdAsync()
        {
            string responseJSON = "{\"responses\":"
                + "[{"
                    + "\"id\": \"1\","
                    + "\"status\":200,"
                    + "\"headers\":{\"Cache-Control\":\"no-cache\",\"OData-Version\":\"4.0\",\"Content-Type\":\"application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=false;charset=utf-8\"},"
                    + "\"body\":{\"@odata.context\":\"https://graph.microsoft.com/v1.0/$metadata#users/$entity\",\"displayName\":\"MOD Administrator\",\"jobTitle\":null,\"id\":\"9f4fe8ea-7e6e-486e-b8f4-VkHdanfIomf\"}"
                + "},"
                + "{"
                    + "\"id\": \"2\"," 
                    + "\"status\":409,"
                    + "\"headers\" : {\"Cache-Control\":\"no-cache\"},"
                    + "\"body\":{\"error\": {\"code\": \"20117\",\"message\": \"An item with this name already exists in this location.\",\"innerError\":{\"request-id\": \"nothing1b13-45cd-new-92be873c5781\",\"date\": \"2019-03-22T23:17:50\"}}}"
                +"}]}";

            HttpContent content = new StringContent(responseJSON);
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponseMessage.Content = content;

            BatchResponseContent batchResponseContent = new BatchResponseContent(httpResponseMessage);

            HttpResponseMessage response = await batchResponseContent.GetResponseByIdAsync("2");

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.True(response.Headers.CacheControl.NoCache);
        }


        [Fact]
        public async Task BatchResponseContent_GetResponseByIdAsyncWithDeseirializer()
        {
            // Arrange
            string responseJSON = "{\"responses\":"
                + "[{"
                    + "\"id\": \"1\","
                    + "\"status\":200,"
                    + "\"headers\":{\"Cache-Control\":\"no-cache\",\"OData-Version\":\"4.0\",\"Content-Type\":\"application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=false;charset=utf-8\"},"
                    + "\"body\":{\"@odata.context\":\"https://graph.microsoft.com/v1.0/$metadata#users/$entity\",\"displayName\":\"MOD Administrator\",\"jobTitle\":null,\"id\":\"9f4fe8ea-7e6e-486e-b8f4-VkHdanfIomf\"}"
                + "},"
                + "{"
                    + "\"id\": \"2\","
                    + "\"status\":200,"
                    + "\"headers\":{\"Cache-Control\":\"no-store, no-cache\",\"OData-Version\":\"4.0\",\"Content-Type\":\"application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=false;charset=utf-8\"},"
                    + "\"body\":{\"@odata.context\":\"https://graph.microsoft.com/v1.0/$metadata#drives/$entity\",\"createdDateTime\":\"2019-01-12T09:05:38Z\",\"description\":\"\",\"id\":\"b!random-VkHdanfIomf\",\"lastModifiedDateTime\":\"2019-03-06T06:59:04Z\",\"name\":\"OneDrive\",\"webUrl\":\"https://m365x751487-my.sharepoint.com/personal/admin_m365x751487_onmicrosoft_com/Documents\",\"driveType\":\"business\",\"createdBy\":{\"user\":{\"displayName\":\"System Account\"}},\"lastModifiedBy\":{\"user\":{\"displayName\":\"System Account\"}},\"owner\":{\"user\":{\"email\":\"admin@M365x751487.OnMicrosoft.com\",\"id\":\"6b4fa8ea-7e6e-486e-a8f4-d00a5b23488c\",\"displayName\":\"MOD Administrator\"}},\"quota\":{\"deleted\":0,\"remaining\":1099509670098,\"state\":\"normal\",\"total\":1099511627776,\"used\":30324}}"
                + "},"
                + "{"
                    + "\"id\": \"3\","
                    + "\"status\":201,"
                    + "\"headers\":{\"Location\":\"https://graph.microsoft.com/v1.0/users/9f4fe8ea-7e6e-486e-a8f4-nothing-here/onenote/notebooks/1-zyz-a1c1-441a-8b41-9378jjdd2\",\"Preference-Applied\":\"odata.include-annotations=*\",\"Cache-Control\":\"no-cache\",\"OData-Version\":\"4.0\",\"Content-Type\":\"application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=false;charset=utf-8\"},"
                    + "\"body\":{\"@odata.context\":\"https://graph.microsoft.com/v1.0/$metadata#users('9f4fe8ea-7e6e-486e-a8f4-nothing-here')/onenote/notebooks/$entity\",\"id\":\"1-9f4fe8ea-7e6e-486e-a8f4-nothing-here\",\"self\":\"https://graph.microsoft.com/v1.0/users/9f4fe8ea-7e6e-486e-a8f4-nothing-here/onenote/notebooks/1-9f4fe8ea-7e6e-486e-a8f4-nothing-here\",\"createdDateTime\":\"2019-03-06T08:08:09Z\",\"displayName\":\"My Notebook -442293399\",\"lastModifiedDateTime\":\"2019-03-06T08:08:09Z\"}"
                + "},"
                + "{"
                    + "\"id\": \"4\","
                    + "\"status\":409,"
                    + "\"headers\" : {\"Cache-Control\":\"no-cache\"},"
                    + "\"body\":{\"error\": {\"code\": \"20117\",\"message\": \"An item with this name already exists in this location.\",\"innerError\":{\"request-id\": \"nothing1b13-45cd-new-92be873c5781\",\"date\": \"2019-03-22T23:17:50\"}}}"
                + "}" +
                "]}";

            HttpContent content = new StringContent(responseJSON);
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponseMessage.Content = content;

            BatchResponseContent batchResponseContent = new BatchResponseContent(httpResponseMessage);

            // Act
            User user = await batchResponseContent.GetResponseByIdAsync<User>("1");
            // Assert we have a valid user
            Assert.Equal("MOD Administrator", user.DisplayName);

            // Act
            Drive drive = await batchResponseContent.GetResponseByIdAsync<Drive>("2");
            // Assert we have a valid drive object
            Assert.Equal("b!random-VkHdanfIomf", drive.Id);
            Assert.Equal("OneDrive", drive.Name);

            // Act
            Notebook notebook = await batchResponseContent.GetResponseByIdAsync<Notebook>("3");
            // Assert we have a valid notebook object
            Assert.Equal("1-9f4fe8ea-7e6e-486e-a8f4-nothing-here", notebook.Id);
            Assert.Equal("My Notebook -442293399", notebook.DisplayName);

            // Act
            ServiceException serviceException = await Assert.ThrowsAsync<ServiceException>(() => batchResponseContent.GetResponseByIdAsync<DriveItem>("4"));
            // Assert we detect the incorrect response and give usable Service Exception
            Assert.Equal("20117", serviceException.Error.Code);
            Assert.Equal(HttpStatusCode.Conflict, serviceException.StatusCode);//status 409
            Assert.NotNull(serviceException.RawResponseBody);
        }

        [Fact]
        public async Task BatchResponseContent_GetResponseByIdAsyncWithDeserializerWorksWithDateTimeOffsets()
        {
            // Arrange an example Event object with a few properties
            string responseJSON = "\n{\n" +
                                  "    \"responses\": [\n" +
                                  "        {\n" +
                                  "            \"id\": \"3\",\n" +
                                  "            \"status\": 200,\n" +
                                  "            \"headers\": {\n" +
                                  "                \"Cache-Control\": \"private\",\n" +
                                  "                \"OData-Version\": \"4.0\",\n" +
                                  "                \"Content-Type\": \"application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=false;charset=utf-8\",\n" +
                                  "                \"ETag\": \"W/\\\"h8TLt1Vki0W7hBZaqTqGTQAAQyxv+g==\\\"\"\n" +
                                  "            },\n" +
                                  "            \"body\": {\n" +
                                  "                \"@odata.context\": \"https://graph.microsoft.com/v1.0/$metadata#users('d9f7c4f6-e1bb-4032-a86d-6e84722b983d')/events/$entity\",\n" +
                                  "                \"@odata.etag\": \"W/\\\"h8TLt1Vki0W7hBZaqTqGTQAAQyxv+g==\\\"\",\n" +
                                  "                \"id\": \"AQMkADcyMWRhMWZmAC0xZTI1LTRjZjEtYTRjMC04M\",\n" +
                                  "                \"categories\": [],\n" +
                                  "                \"originalStartTimeZone\": \"Pacific Standard Time\",\n" +
                                  "                \"originalEndTimeZone\": \"Pacific Standard Time\",\n" +
                                  "                \"iCalUId\": \"040000008200E00074C5B7101A82E0080000000053373A40E03ED5010000000000000000100000007C41056410E97C44B2A34798E719B862\",\n" +
                                  "                \"reminderMinutesBeforeStart\": 15,\n" +
                                  "                \"type\": \"singleInstance\",\n" +
                                  "                \"webLink\": \"https://outlook.office365.com/owa/?itemid=AQMkADcyMWRhMWZmAC0xZTI1LTRjZjEtYTRjMC04MGY3OGEzNThiZDAARgAAA1AZwxLGN%2FJIv2Mj%2F0o8JqYHAIfEy7dVZItFu4QWWqk6hk0AAAIBDQAAAIfEy7dVZItFu4QWWqk6hk0AAAI4eQAAAA%3D%3D&exvsurl=1&path=/calendar/item\",\n" +
                                  "                \"onlineMeetingUrl\": null,\n" +
                                  "                \"recurrence\": null,\n" +
                                  "                \"responseStatus\": {\n" +
                                  "                    \"response\": \"notResponded\",\n" +
                                  "                    \"time\": \"0001-01-01T00:00:00Z\"\n" +
                                  "                },\n" +
                                  "                \"body\": {\n" +
                                  "                    \"contentType\": \"html\",\n" +
                                  "                    \"content\": \"<html>\\r\\n<head>\\r\\n<meta http-\",\n" +

                                  "                },\n" +
                                  "                \"start\": {\n" +
                                  "                    \"dateTime\": \"2019-07-30T22:00:00.0000000\",\n" +
                                  "                    \"timeZone\": \"UTC\"\n" +
                                  "                },\n" +
                                  "                \"end\": {\n" +
                                  "                    \"dateTime\": \"2019-07-30T23:00:00.0000000\",\n" +
                                  "                    \"timeZone\": \"UTC\"\n" +
                                  "                }" +
                                  "            }\n" +
                                  "        }\n" +
                                  "    ]\n" +
                                  "}";

            HttpContent content = new StringContent(responseJSON);
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content
            };

            BatchResponseContent batchResponseContent = new BatchResponseContent(httpResponseMessage);

            // Act
            Event eventItem = await batchResponseContent.GetResponseByIdAsync<Event>("3");

            // Assert we have a valid datetime in the event
            Assert.Equal("2019-07-30T23:00:00.0000000", eventItem.End.DateTime);
            Assert.Equal("UTC",eventItem.End.TimeZone);

            Assert.Equal("2019-07-30T22:00:00.0000000", eventItem.Start.DateTime);
            Assert.Equal("UTC", eventItem.Start.TimeZone);

        }
    }
}
