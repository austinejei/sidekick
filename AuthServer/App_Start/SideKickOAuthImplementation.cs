using System;
using System.Collections.Concurrent;

using System.Linq;
using System.Security.Claims;
using Microsoft.Owin;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;

namespace AuthServer
{
    public class SideKickOAuthImplementation : OAuthAuthorizationServerOptions
    {
        public SideKickOAuthImplementation()
        {
            AuthorizeEndpointPath = new PathString("/oauth/authorize");
            TokenEndpointPath = new PathString("/oauth/token");
            ApplicationCanDisplayErrors = true;
            AllowInsecureHttp = true;
            Provider = new SidekickOAuthProvider();


            AuthorizationCodeProvider = new AuthenticationTokenProvider
                                        {
                                            OnCreate = CreateAuthenticationCode,
                                            OnReceive = ReceiveAuthenticationCode,
                                        };

            RefreshTokenProvider = new AuthenticationTokenProvider
                                   {
                                       OnCreate = CreateRefreshToken,
                                       OnReceive = ReceiveRefreshToken,
                                   };
            
            AccessTokenExpireTimeSpan = TimeSpan.FromHours(1); //modify during production..you might wanna read from db

            AccessTokenFormat = new SidekickJwtFormat(this);

        }

        private void ReceiveRefreshToken(AuthenticationTokenReceiveContext context)
        {
            context.DeserializeTicket(context.Token);
        }

        private void CreateRefreshToken(AuthenticationTokenCreateContext context)
        {
            //todo: we can decide to skip creating refresh token if app is not a trusted app
            //like 
            //var appId = context.Ticket.Identity.Claims.FirstOrDefault(c => c.Type == "sidekick.client.appId");
            //this way, we can query the db and just fetch the appropriate flag
            context.SetToken(context.SerializeTicket());
        }

        private void ReceiveAuthenticationCode(AuthenticationTokenReceiveContext context)
        {
            string value;
            if (_authCodes.TryRemove(context.Token, out value))
            {
                context.DeserializeTicket(value);
            }
        }

        private readonly ConcurrentDictionary<string, string> _authCodes = new ConcurrentDictionary<string, string>();
        private void CreateAuthenticationCode(AuthenticationTokenCreateContext context)
        {
            context.SetToken(Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n"));
            _authCodes[context.Token] = context.SerializeTicket();
        }
    }
}