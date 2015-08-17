using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Sidekick.OAuthClient.Provider;

namespace Sidekick.OAuthClient
{
    public class SidekickAuthenticationOptions : AuthenticationOptions
    {
       

        public class SidekickAuthenticationEndpoints
        {
            /// <summary>
            /// Endpoint which is used to redirect users to request Sidekick access
            /// </summary>
            /// <remarks>
            /// Defaults to http://localhost/authserver/oauth/authorize
            /// </remarks>
            public string AuthorizationEndpoint { get; set; }

            /// <summary>
            /// Endpoint which is used to exchange code for access token
            /// </summary>
            /// <remarks>
            /// Defaults to http://localhost/authserver/oauth/token
            /// </remarks>
            public string TokenEndpoint { get; set; }
        }

        private const string AuthorizationEndPoint = "http://localhost/authserver/oauth/authorize";
        private const string TokenEndpoint = "http://localhost/authserver/oauth/token";
        public SidekickAuthenticationOptions() : base("Sidekick")
        {
            Caption = Constants.DefaultAuthenticationType;
            CallbackPath = new PathString("/");
            AuthenticationMode = AuthenticationMode.Passive;
            Scope = new List<string>
            {
                "user.profile"
            };
            BackchannelTimeout = TimeSpan.FromSeconds(60);
            Endpoints = new SidekickAuthenticationEndpoints
            {
                AuthorizationEndpoint = AuthorizationEndPoint,
                TokenEndpoint = TokenEndpoint
            };
        }

        /// <summary>
        ///     Gets or sets the a pinned certificate validator to use to validate the endpoints used
        ///     in back channel communications belong to Sidekick.
        /// </summary>
        /// <value>
        ///     The pinned certificate validator.
        /// </value>
        /// <remarks>
        ///     If this property is null then the default certificate checks are performed,
        ///     validating the subject name and if the signing chain is a trusted party.
        /// </remarks>
        public ICertificateValidator BackchannelCertificateValidator { get; set; }

        /// <summary>
        ///     The HttpMessageHandler used to communicate with Sidekick.
        ///     This cannot be set at the same time as BackchannelCertificateValidator unless the value
        ///     can be downcast to a WebRequestHandler.
        /// </summary>
        public HttpMessageHandler BackchannelHttpHandler { get; set; }

        /// <summary>
        ///     Gets or sets timeout value in milliseconds for back channel communications with Sidekick.
        /// </summary>
        /// <value>
        ///     The back channel timeout in milliseconds.
        /// </value>
        public TimeSpan BackchannelTimeout { get; set; }

        /// <summary>
        ///     The request path within the application's base path where the user-agent will be returned.
        ///     The middleware will process this request when it arrives.
        ///     Default value is "/signin-Sidekick".
        /// </summary>
        public PathString CallbackPath { get; set; }

        /// <summary>
        ///     Get or sets the text that the user can display on a sign in user interface.
        /// </summary>
        public string Caption
        {
            get { return Description.Caption; }
            set { Description.Caption = value; }
        }

        /// <summary>
        ///     Gets or sets the Sidekick supplied Client ID
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        ///     Gets or sets the Sidekick supplied Client Secret
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets the sets of OAuth endpoints used to authenticate against Sidekick.  Overriding these endpoints allows you to use Sidekick Enterprise for
        /// authentication.
        /// </summary>
        public SidekickAuthenticationEndpoints Endpoints { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="ISidekickAuthenticationProvider" /> used in the authentication events
        /// </summary>
        public ISidekickAuthenticationProvider Provider { get; set; }

        /// <summary>
        /// A list of permissions to request.
        /// </summary>
        public IList<string> Scope { get; set; }

        /// <summary>
        ///     Gets or sets the name of another authentication middleware which will be responsible for actually issuing a user
        ///     <see cref="System.Security.Claims.ClaimsIdentity" />.
        /// </summary>
        public string SignInAsAuthenticationType { get; set; }

        /// <summary>
        ///     Gets or sets the type used to secure data handled by the middleware.
        /// </summary>
        public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }

        
    }
}