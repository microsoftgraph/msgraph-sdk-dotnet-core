// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IAsyncMonitor<T>
    {
        Task<T> PollForOperationCompletionAsync(IProgress<AsyncOperationStatus> progress, CancellationToken cancellationToken);
    }
}
