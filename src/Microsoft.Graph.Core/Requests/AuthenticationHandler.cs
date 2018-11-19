using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Graph
{
    /// <summary>
    /// An <see cref="DelegatingHandler"/> implementation using standard .NET libraries.
    /// </summary>
    public class AuthenticationHandler: DelegatingHandler
    {
        /// <summary>
        /// AuthenticationProvider property
        /// </summary>
        public IAuthenticationProvider AuthenticationProvider { get; set; }

        /// <summary>
        /// Construct a new <see cref="AuthenticationHandler"/>
        /// </summary>
        public AuthenticationHandler()
        {

        }

        /// <summary>
        /// Construct a new <see cref="AuthenticationHandler"/>
        /// </summary>
        /// <param name="authenticationProvider">An authentication provider to pass to <see cref="AuthenticationHandler"/> for authenticating requests.</param>
        /// <param name="innerHandler">A HTTP message handler to pass to the <see cref="AuthenticationHandler"/> for sending requests.</param>
        public AuthenticationHandler(IAuthenticationProvider authenticationProvider, HttpMessageHandler innerHandler)
        {
            InnerHandler = innerHandler;
            AuthenticationProvider = authenticationProvider;
        }

        /// <summary>
        /// Send a HTTP request
        /// </summary>
        /// <param name="httpRequest">The <see cref="HttpRequestMessage"/> to send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequest, CancellationToken cancellationToken)
        {
            // Authenticate request using AuthenticationProvider
            await AuthenticationProvider.AuthenticateRequestAsync(httpRequest);

            HttpResponseMessage response = await base.SendAsync(httpRequest, cancellationToken);

            return response;
        }
    }
}
