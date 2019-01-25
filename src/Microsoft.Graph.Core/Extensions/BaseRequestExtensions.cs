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
        /// Sets Microsoft Graph's scopes to the default Authentication Middleware Handler for this request in order to perform incremental conscent.
        /// This only works with the default authentication handler and default set of Microsoft graph authentication providers.
        /// If you use a custom authentication handler or authentication provider, you have to handle it's retreival in your implementation.
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
        /// Sets MSAL's ForceRefresh property to the default Authentication Middleware Handler for this request.
        /// This only works with the default authentication handler and default set of Microsoft graph authentication providers.
        /// If you use a custom authentication handler or authentication provider, you have to handle it's retreival in your implementation.
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
        /// Sets an authentication provider to the default Authentication Middleware Handler for this request.
        /// This only works with the default authentication handler.
        /// If you use a custom authentication handler, you have to handle it's retreival in your implementation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="baseRequest">The <see cref="BaseRequest"/> for the request.</param>
        /// <param name="authenticationProvider">A <see cref="IAuthenticationProvider"/></param>
        /// <returns></returns>
        public static T WithAuthProvider<T>(this T baseRequest, IAuthenticationProvider authenticationProvider) where T : IBaseRequest
        {
            string authOptionKey = typeof(AuthOption).ToString();
            if (baseRequest.MiddlewareOptions.ContainsKey(authOptionKey))
            {
                (baseRequest.MiddlewareOptions[authOptionKey] as AuthOption).AuthenticationProvider = authenticationProvider;
            }
            else
            {
                baseRequest.MiddlewareOptions.Add(authOptionKey, new AuthOption { AuthenticationProvider = authenticationProvider });
            }
            return baseRequest;
        }

        /// <summary>
        /// Sets a ShouldRetry <see cref="Func{HttpResponseMessage, Boolean}"/> delegate to the default Retry Middleware Handler for this request.
        /// This only works with the default Retry Middleware Handler.
        /// If you use a custom Retry Middleware Handler, you have to handle it's retreival in your implementation.
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
        /// Sets the maximum number of retries to the default Retry Middleware Handler for this request.
        /// This only works with the default Retry Middleware Handler.
        /// If you use a custom Retry Middleware Handler, you have to handle it's retreival in your implementation.
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
        /// Sets the maximum number of redirects to the default Redirect Middleware Handler for this request.
        /// This only works with the default Redirect Middleware Handler.
        /// If you use a custom Redirect Middleware Handler, you have to handle it's retreival in your implementation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="baseRequest">The <see cref="BaseRequest"/> for the request.</param>
        /// <param name="maxRedirects">Maximum number of redirects allowed for the request</param>
        /// <returns></returns>
        public static T WithMaxRedirects<T>(this T baseRequest, int maxRedirects) where T : IBaseRequest
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
    }
}
