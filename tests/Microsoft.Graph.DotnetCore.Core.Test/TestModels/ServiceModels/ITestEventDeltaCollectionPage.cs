// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{
    /// <summary>
    /// The interface IUserEventsCollectionPage.
    /// </summary>
    [InterfaceConverter(typeof(InterfaceConverter<TestEventDeltaCollectionPage>))]
    public interface ITestEventDeltaCollectionPage : ICollectionPage<TestEvent>
    {
        /// <summary>
        /// Gets the next page <see cref="ITestEventDeltaCollectionPage"/> instance.
        /// </summary>
        ITestEventDeltaRequest NextPageRequest { get; }

        /// <summary>
        /// Initializes the NextPageRequest property.
        /// </summary>
        void InitializeNextPageRequest(IBaseClient client, string nextPageLinkString);
    }

}