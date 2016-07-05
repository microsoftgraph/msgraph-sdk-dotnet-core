// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Core.Test.TestModels
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Graph;

    public partial class TestAsyncMonitor : AsyncMonitor<DerivedTypeClass>
    {
        public TestAsyncMonitor(IBaseClient client, string monitorUrl)
            : base(client, monitorUrl)
        {
        }

        /// <summary>
        /// Polls until the async operation is complete and returns the resulting DerivedTypeClass.
        /// </summary>
        public Task<DerivedTypeClass> CompleteOperationAsync(IProgress<AsyncOperationStatus> progress, CancellationToken cancellationToken)
        {
                
            return this.PollForOperationCompletionAsync(progress, cancellationToken);
                
        }
    }
}
