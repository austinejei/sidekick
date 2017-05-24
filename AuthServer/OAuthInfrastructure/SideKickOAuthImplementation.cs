using System;
using System.Collections.Concurrent;
using System.Data.Entity;
using System.Linq;
using DataLayer;
using Microsoft.Owin;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;

namespace AuthServer.OAuthInfrastructure
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
            
           // AccessTokenExpireTimeSpan = TimeSpan.FromHours(1); //modify during production..you might wanna read from db

            AccessTokenFormat = new SidekickJwtFormat(this);

        }

        private void ReceiveRefreshToken(AuthenticationTokenReceiveContext context)
        {
            
            context.DeserializeTicket(context.Token);
        }

        private void CreateRefreshToken(AuthenticationTokenCreateContext context)
        {
        
            //var isTrusted = bool.Parse(context.Ticket.Identity.Claims.FirstOrDefault(c => c.Type == "sidekick.client.istrusted").Value);
            var isTrusted = bool.Parse(context.Ticket.Identity.Claims.FirstOrDefault(c => c.Type == "sidekick.client.istrusted").Value);

            var refreshTokenExpiry = TimeSpan.Parse(context.Ticket.Identity.Claims.FirstOrDefault(c => c.Type == "sidekick.client.refreshTokenExpiry").Value);


            //if (isTrusted)
            //{
            //    context.SetToken(context.SerializeTicket());
            //}

            if (isTrusted)
            {
                var appId =
                    int.Parse(
                        context.Ticket.Identity.Claims.FirstOrDefault(c => c.Type == "sidekick.client.appId").Value);

                var _dbContext = new ApplicationDbContext();

                var userApp =
                    _dbContext.UserApps.FirstOrDefaultAsync(
                            x => x.AppId == appId && x.Username == context.Ticket.Identity.Name)
                        .Result;


                //we need a way to know if the current accessToken has expired....
                if (userApp?.RefreshTokenExpiresOn != null)
                {
                  
                    userApp.RefreshToken = context.SerializeTicket();

                    userApp.HashedRefreshToken = Guid.NewGuid().ToString("N");

                    context.SetToken(userApp.HashedRefreshToken);
                    _dbContext.Entry(userApp).State = EntityState.Modified;
                    _dbContext.SaveChanges();
                }
                else
                {
                    if (userApp != null)
                    {
                        userApp.RefreshToken = context.SerializeTicket();

                        userApp.HashedRefreshToken = Guid.NewGuid().ToString("N");

                        userApp.RefreshTokenExpiresOn = DateTime.Now.AddMinutes(refreshTokenExpiry.TotalMinutes);

                        _dbContext.Entry(userApp).State = EntityState.Modified;
                        _dbContext.SaveChanges();

                        context.SetToken(userApp.HashedRefreshToken);
                    }

                }

            }

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