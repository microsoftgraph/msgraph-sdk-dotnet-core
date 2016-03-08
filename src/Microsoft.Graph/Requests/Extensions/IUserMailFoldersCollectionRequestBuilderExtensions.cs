// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    public partial interface IUserMailFoldersCollectionRequestBuilder
    {
        /// <summary>
        /// Gets the Deleted Items mail folder request builder.
        /// </summary>
        IMailFolderRequestBuilder DeletedItems { get; }

        /// <summary>
        /// Gets the Drafts mail folder request builder.
        /// </summary>
        IMailFolderRequestBuilder Drafts { get; }

        /// <summary>
        /// Gets the Inbox mail folder request builder.
        /// </summary>
        IMailFolderRequestBuilder Inbox { get; }

        /// <summary>
        /// Gets the Sent Items mail folder request builder.
        /// </summary>
        IMailFolderRequestBuilder SentItems { get; }
    }
}
