// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Net.Http;

    /// <summary>
    /// Extension methods for <see cref="BaseRequest"/>
    /// </summary>
    public static class BaseRequestExtensions
    {
        /// <summary>
        /// Updates the Microsoft Graph scopes for the request (incremental conscent)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="baseRequest">The <see cref="BaseRequest"/> for the request.</param>
        /// <param name="scopes">Scopes required to access a protected API.</param>
        /// <returns></returns>
        public static T WithScopes<T>(this T baseRequest, string[] scopes) where T : IBaseRequest
        {
            string authOptionKey = typeof(AuthOption).ToString();
            if (baseRequest.MiddlewareOptions.ContainsKey(authOptionKey))
            {
                (baseRequest.MiddlewareOptions[authOptionKey] as AuthOption).Scopes = scopes;
            }
            else
            {
                baseRequest.MiddlewareOptions.Add(authOptionKey, new AuthOption { Scopes = scopes });
            }
            return baseRequest;
        }

        /// <summary>
        /// Sets MSAL's ForceRefresh property for the request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="baseRequest">The <see cref="BaseRequest"/> for the request.</param>
        /// <param name="forceRefresh">If <c>true</c>, ignore any access token in the cache and attempt to acquire new access token
        /// using the refresh token for the account if this one is available.</param>
        /// <returns></returns>
        public static T WithForceRefresh<T>(this T baseRequest, bool forceRefresh) where T : IBaseRequest
        {
            string authOptionKey = typeof(AuthOption).ToString();
            if (baseRequest.MiddlewareOptions.ContainsKey(authOptionKey))
            {
                (baseRequest.MiddlewareOptions[authOptionKey] as AuthOption).ForceRefresh = forceRefresh;
            }
            else
            {
                baseRequest.MiddlewareOptions.Add(authOptionKey, new AuthOption { ForceRefresh = forceRefresh });
            }
            return baseRequest;
        }

        /// <summary>
        /// Sets <see cref="Func{HttpResponseMessage, Boolean}"/> delegate to the request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="baseRequest">The <see cref="BaseRequest"/> for the request.</param>
        /// <param name="shouldRetry">A <see cref="Func{HttpResponseMessage, Boolean}"/> for the request.</param>
        /// <returns></returns>
        public static T WithShouldRetry<T>(this T baseRequest, Func<HttpResponseMessage, bool> shouldRetry) where T : IBaseRequest
        {
            string retryOptionKey = typeof(RetryOption).ToString();
            if (baseRequest.MiddlewareOptions.ContainsKey(retryOptionKey))
            {
                (baseRequest.MiddlewareOptions[retryOptionKey] as RetryOption).ShouldRetry = shouldRetry;
            }
            else
            {
                baseRequest.MiddlewareOptions.Add(retryOptionKey, new RetryOption { ShouldRetry = shouldRetry });
            }
            return baseRequest;
        }

        /// <summary>
        /// Sets maximum retry to the request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="baseRequest">The <see cref="BaseRequest"/> for the request.</param>
        /// <param name="maxRetry">The maxRetry for the request.</param>
        /// <returns></returns>
        public static T WithMaxRetry<T>(this T baseRequest, int maxRetry) where T : IBaseRequest
        {
            string retryOptionKey = typeof(RetryOption).ToString();
            if (baseRequest.MiddlewareOptions.ContainsKey(retryOptionKey))
            {
                (baseRequest.MiddlewareOptions[retryOptionKey] as RetryOption).MaxRetry = maxRetry;
            }
            else
            {
                baseRequest.MiddlewareOptions.Add(retryOptionKey, new RetryOption { MaxRetry = maxRetry });
            }
            return baseRequest;
        }

        /// <summary>
        /// Sets the maximum number of redirects for the request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="baseRequest">The <see cref="BaseRequest"/> for the request.</param>
        /// <param name="maxRedirects">Maximum number of redirects allowed for the request</param>
        /// <returns></returns>
        public static T WithMaxRedirect<T>(this T baseRequest, int maxRedirects) where T : IBaseRequest
        {
            string redirectOptionKey = typeof(RedirectOption).ToString();
            if (baseRequest.MiddlewareOptions.ContainsKey(redirectOptionKey))
            {
                (baseRequest.MiddlewareOptions[redirectOptionKey] as RedirectOption).MaxRedirects = maxRedirects;
            }
            else
            {
                baseRequest.MiddlewareOptions.Add(redirectOptionKey, new RedirectOption { MaxRedirects = maxRedirects });
            }
            return baseRequest;
        }

        /// <summary>
        /// Adds a collection of middleware options to the request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="baseRequest">The <see cref="BaseRequest"/> for the request.</param>
        /// <param name="middlewareOptions">A collection of <see cref="IMiddlewareOption"/>.</param>
        /// <returns></returns>
        public static T AddMiddlewareOptions<T>(this T baseRequest, IMiddlewareOption[] middlewareOptions) where T: IBaseRequest
        {
            foreach (IMiddlewareOption option in middlewareOptions)
            {
                baseRequest.MiddlewareOptions.Add(option.GetType().ToString(), option);
            }

            return baseRequest;
        }
    }
}
