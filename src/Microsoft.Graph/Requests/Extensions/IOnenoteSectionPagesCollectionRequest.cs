// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Linq.Expressions;
    using System.IO;

    /// <summary>
    /// The interface IOnenoteSectionPagesCollectionRequest.
    /// </summary>
    public partial interface IOnenoteSectionPagesCollectionRequest : IBaseRequest
    {
        /// <summary>
        /// Adds the specified MultipartContent OnenotePage to the collection via POST.
        /// </summary>
        /// <param name="onenotePage">The MultipartContent OnenotePage to add.</param>
        /// <returns>The created OnenotePage.</returns>
        System.Threading.Tasks.Task<OnenotePage> AddAsync(MultipartContent onenotePage);

        /// <summary>
        /// Adds the specified MultipartContent OnenotePage to the collection via POST.
        /// </summary>
        /// <param name="onenotePage">The MultipartContent OnenotePage to add.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The created OnenotePage.</returns>
        System.Threading.Tasks.Task<OnenotePage> AddAsync(MultipartContent onenotePage, CancellationToken cancellationToken);

        /// <summary>
        /// Adds the specified OnenotePage to the collection via POST.
        /// </summary>
        /// <param name="onenotePage">The OnenotePage to add in stream form.</param>
        /// <param name="contentType">The content type of the stream. Values can be text/html or application/xhtml+xml.</param>
        /// <returns>The created OnenotePage.</returns>
        System.Threading.Tasks.Task<OnenotePage> AddAsync(Stream onenotePage, string contentType);

        /// <summary>
        /// Adds the specified OnenotePage to the collection via POST.
        /// </summary>
        /// <param name="onenotePage">The OnenotePage to add in stream form.</param>
        /// <param name="contentType">The content type of the stream. Values can be text/html or application/xhtml+xml.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The created OnenotePage.</returns>
        System.Threading.Tasks.Task<OnenotePage> AddAsync(Stream onenotePage, string contentType, CancellationToken cancellationToken);

        /// <summary>
        /// Adds the specified OnenotePage to the collection via POST.
        /// </summary>
        /// <param name="onenotePageHtml">The OnenotePage to add.</param>
        /// <param name="contentType">The content type of the stream. Values can be text/html or application/xhtml+xml.</param>
        /// <returns>The created OnenotePage.</returns>
        System.Threading.Tasks.Task<OnenotePage> AddAsync(string onenotePageHtml, string contentType);

        /// <summary>
        /// Adds the specified OnenotePage to the collection via POST.
        /// </summary>
        /// <param name="onenotePageHtml">The OnenotePage to add.</param>
        /// <param name="contentType">The content type of the stream. Values can be text/html or application/xhtml+xml.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The created OnenotePage.</returns>
        System.Threading.Tasks.Task<OnenotePage> AddAsync(string onenotePageHtml, string contentType, CancellationToken cancellationToken);
    }
}
