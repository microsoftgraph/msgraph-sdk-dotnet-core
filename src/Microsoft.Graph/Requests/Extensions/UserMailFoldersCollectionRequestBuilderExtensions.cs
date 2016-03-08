// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    public partial class UserMailFoldersCollectionRequestBuilder
    {
        /// <summary>
        /// Gets the Deleted Items mail folder request builder.
        /// </summary>
        public IMailFolderRequestBuilder DeletedItems
        {
            get { return new MailFolderRequestBuilder(this.AppendSegmentToRequestUrl(Constants.Url.DeletedItems), this.Client); }
        }

        /// <summary>
        /// Gets the Drafts mail folder request builder.
        /// </summary>
        public IMailFolderRequestBuilder Drafts
        {
            get { return new MailFolderRequestBuilder(this.AppendSegmentToRequestUrl(Constants.Url.Drafts), this.Client); }
        }

        /// <summary>
        /// Gets the Inbox mail folder request builder.
        /// </summary>
        public IMailFolderRequestBuilder Inbox
        {
            get { return new MailFolderRequestBuilder(this.AppendSegmentToRequestUrl(Constants.Url.Inbox), this.Client); }
        }

        /// <summary>
        /// Gets the Sent Items mail folder request builder.
        /// </summary>
        public IMailFolderRequestBuilder SentItems
        {
            get { return new MailFolderRequestBuilder(this.AppendSegmentToRequestUrl(Constants.Url.SentItems), this.Client); }
        }
    }
}
