using System;
using System.Collections.Generic;
using Microsoft.Owin;
using Owin;

namespace Sidekick.OAuthClient
{
    public static class SidekickAuthenticationExtensions
    {
        public static IAppBuilder SidekickAuthentication(this IAppBuilder app,
            SidekickAuthenticationOptions options)
        {
            if (app == null)
                throw new ArgumentNullException("app");
            if (options == null)
                throw new ArgumentNullException("options");

            app.Use(typeof(SidekickAuthenticationMiddleware), app, options);

            return app;
        }

        /// <summary>
        /// Uses Sidekick's Authentication/Authorization to identify user and request associated permissions to be granted.
        /// </summary>
        /// <param name="app">Must be valid and not null</param>
        /// <param name="clientId">Your Client ID. Obtained from your Sidekick API Access page</param>
        /// <param name="clientSecret">Your Client Secret. Obtained from your Sidekick API Access page</param>
        /// <param name="scopes">A list of scopes/resources you wish to access on behalf of the authenticated user. Must match the same scopes defined on the API Access page on Sidekick.</param>
        /// <param name="callBackPath">A relative path to your callback URL on API Access page on Sidekick. <br/>If your callback URL on Sidekick is http://myapp.com/oauth then make it /oauth. Same applies to local environments.</param>
        /// <param name="authorizationEndpoint">Must be valid and not null</param>
        /// <param name="tokenEndpoint">Must be valid and not null</param>
        /// <returns></returns>
        public static IAppBuilder SidekickAuthentication(this IAppBuilder app, string clientId,
            string clientSecret, IList<string> scopes, string callBackPath, string authorizationEndpoint, string tokenEndpoint)
        {
            return app.SidekickAuthentication(new SidekickAuthenticationOptions
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = scopes,
                CallbackPath = new PathString(callBackPath),
                Endpoints = new SidekickAuthenticationOptions.SidekickAuthenticationEndpoints
                            {
                                AuthorizationEndpoint = authorizationEndpoint,
                                TokenEndpoint = tokenEndpoint
                            }
            });
        }
    }
}
