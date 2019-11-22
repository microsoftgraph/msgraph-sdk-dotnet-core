// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Net;
    using System.Net.Http;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;

    /// <summary>
    /// The ResponseHandler for upload requests
    /// </summary>
    public class UploadResponseHandler
    {
        private readonly ISerializer serializer;

        /// <summary>
        /// Constructs a new <see cref="ResponseHandler"/>.
        /// </summary>
        /// <param name="serializer"></param>
        public UploadResponseHandler(ISerializer serializer = null)
        {
            this.serializer = serializer ?? new Serializer();
        }

        /// <summary>
        /// Process raw HTTP response from Upload request
        /// </summary>
        /// <typeparam name="T">The type to return</typeparam>
        /// <param name="response">The HttpResponseMessage to handle.</param>
        /// <returns></returns>
        public async Task<UploadResult<T>> HandleResponse<T>(HttpResponseMessage response) 
        {
            if (response.Content == null)
            {
                throw new ServiceException(new Error
                {
                    Code = ErrorConstants.Codes.GeneralException,
                    Message = ErrorConstants.Messages.NoResponseForUplaod
                });
            }

            // Give back the info from the server for ongoing upload as the upload is ongoing
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            try
            {
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = this.serializer.DeserializeObject<ErrorResponse>(responseString);
                    var error = errorResponse.Error;
                    // Throw excpetion to know something went wrong.
                    throw new ServiceException(error, response.Headers, response.StatusCode, responseString);
                }

                //Upload has completed so return info on the download
                var uploadResult = new UploadResult<T>();

                if (response.StatusCode == HttpStatusCode.Created)
                {
                    uploadResult.ItemResponse = this.serializer.DeserializeObject<T>(responseString);
                    uploadResult.Location = response.Headers.Location;
                }
                else //its a 200 or 202
                {
                    UploadSessionInfo uploadSessionInfo = this.serializer.DeserializeObject<UploadSessionInfo>(responseString);

                    // try to get the session infomation
                    if (uploadSessionInfo?.NextExpectedRanges != null)
                    {
                        uploadResult.UploadSession = uploadSessionInfo;
                    }
                    else
                    {
                        //upload is most likely done. Item info may come in a 200 response
                        uploadResult.ItemResponse = this.serializer.DeserializeObject<T>(responseString);
                    };
                }

                return uploadResult;
            }
            catch (SerializationException exception)
            {
                throw new ServiceException(new Error()
                    {
                        Code = ErrorConstants.Codes.GeneralException,
                        Message = ErrorConstants.Messages.UnableToDeserializexContent,
                    }, 
                    response.Headers,
                    response.StatusCode,
                    responseString,
                    exception);
            }
        }
    }
}