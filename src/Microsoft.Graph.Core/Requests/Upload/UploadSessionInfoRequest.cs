// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The UploadSessionInfoRequest class
    /// </summary>
    public class UploadSessionInfoRequest : BaseRequest
    {
        private readonly UploadResponseHandler responseHandler;

        /// <summary>
        /// Create a new UploadSessionInfoRequest
        /// </summary>
        /// <param name="session">The UploadSession to use in the request.</param>
        /// <param name="client">The <see cref="IBaseClient"/> for handling requests.</param>
        /// <param name="options">Query and header option name value pairs for the request.</param>
        public UploadSessionInfoRequest(IUploadSession session, IBaseClient client, IEnumerable<Option> options)
            : base(session.UploadUrl, client, options)
        {
            this.responseHandler = new UploadResponseHandler();
        }

        /// <summary>
        /// Deletes the specified Session
        /// </summary>
        /// <returns>The task to await.</returns>
        public Task DeleteAsync()
        {
            return this.DeleteAsync(CancellationToken.None);
        }

        /// <summary>
        /// Deletes the specified Session
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The task to await.</returns>
        public async Task DeleteAsync(CancellationToken cancellationToken)
        {
            this.Method = "DELETE";
            using (var response = await this.SendRequestAsync(null, cancellationToken).ConfigureAwait(false))
            {
            }
        }

        /// <summary>
        /// Gets the specified UploadSession.
        /// </summary>
        /// <returns>The Item.</returns>
        public Task<IUploadSession> GetAsync()
        {
            return this.GetAsync(CancellationToken.None);
        }

        /// <summary>
        /// Gets the specified UploadSession.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The Item.</returns>
        public async Task<IUploadSession> GetAsync(CancellationToken cancellationToken)
        {
            this.Method = "GET";

            using (var response = await this.SendRequestAsync(null, cancellationToken).ConfigureAwait(false))
            {
                var uploadResult = await this.responseHandler.HandleResponse<UploadSessionInfo>(response);
                return uploadResult.UploadSession;
            }
        }
    }
}