using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using DataLayer;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.OAuth;

namespace AuthServer
{
    public class SidekickOAuthProvider : OAuthAuthorizationServerProvider
    {
        public override Task AuthorizationEndpointResponse(OAuthAuthorizationEndpointResponseContext context)
        {
            
            return base.AuthorizationEndpointResponse(context);
        }

        public override Task GrantCustomExtension(OAuthGrantCustomExtensionContext context)
        {
            //todo: this will be used for SSO
            return base.GrantCustomExtension(context);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            ApplicationUser user;
            var signInManager = context.OwinContext.Get<ApplicationSignInManager>();

            var result =
                await signInManager.PasswordSignInAsync(context.UserName, context.Password, false, shouldLockout: false);

            switch (result)
            {
                case SignInStatus.Success:

                    using (var userRepo = new ApplicationDbContext())
                    {
                        user = await userRepo.Users.FirstOrDefaultAsync(u => u.UserName == context.UserName);
                    }

                    if (user != null)
                    {
                        var identity = new ClaimsIdentity("SidekickOAuth");

                        var clientName = context.OwinContext.Get<string>("sidekick.client.name");
                        var clientMeta = context.OwinContext.Get<string>("sidekick.client.meta");
                        var appId = context.OwinContext.Get<int>("sidekick.client.appId");
                        var isTrusted = context.OwinContext.Get<bool>("sidekick.client.istrusted");
                        var tokenExpiry = context.OwinContext.Get<TimeSpan>("sidekick.client.tokenexpiry");
                        var claims = new List<Claim>
                                     {
                                         new Claim(ClaimTypes.Name, context.UserName),
                                         new Claim(ClaimTypes.Email, user.Email),
                                         new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                                         new Claim(ClaimTypes.Sid, user.Id),
                                         new Claim(ClaimTypes.GivenName, user.Fullname),
                                         new Claim(ClaimTypes.Expiration,tokenExpiry.ToString()),
                                         new Claim("sidekick.client.istrusted",isTrusted.ToString()),
         
                                         new Claim("sidekick.client.name", clientName),
                                         new Claim("sidekick.client.meta", clientMeta),
                                         new Claim("sidekick.client.appId", appId.ToString()),
                            

                                     };

                        identity.AddClaims(claims);
                        context.Validated(identity);
                    }

                    break;
                case SignInStatus.LockedOut:

                case SignInStatus.RequiresVerification:

                case SignInStatus.Failure:
                    context.Rejected();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {

            try
            {
                string clientId, clientSecret;
                if (!context.TryGetBasicCredentials(out clientId, out clientSecret))
                {
                    context.TryGetFormCredentials(out clientId, out clientSecret);
                }

                App app = null;
                bool exceptionThrown = false;
                try
                {
                    

                    using (var db = new ApplicationDbContext())
                    {
                        app = await db.Apps.FirstOrDefaultAsync(c => c.ClientId == clientId && c.ClientSecret == clientSecret);
                    }
                    
                }
                catch (Exception exception)
                {
                    exceptionThrown = true;
                }

                if (exceptionThrown)
                {
                    context.Rejected();
                }

                else if (app == null)
                {
                    context.Rejected();
                }

                else if (!app.IsActive)
                {
                    context.Rejected();
                }
                else
                {
                    // var scopes = context.Parameters["scope"]; //todo: skip if SSO



                    context.OwinContext.Set("sidekick.client.name", app.Username);
                    context.OwinContext.Set("sidekick.client.appId", app.Id);
                    context.OwinContext.Set("sidekick.client.meta", app.Meta);
                    context.OwinContext.Set("sidekick.client.istrusted", app.IsTrusted);
                    context.OwinContext.Set("sidekick.client.tokenexpiry", app.AccessTokenExpiry);
      

                    context.Validated();
                }
            }
            catch (Exception)
            {

                context.Rejected();
            }
        }

        public override async Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            var app =
                await new ApplicationDbContext().Apps.FirstOrDefaultAsync(c => c.ClientId == context.ClientId);

            if (app != null)
            {
                context.Validated(app.RedirectUrl);

            }
            else
            {
                context.Rejected();
            }


        }

        public override async Task GrantClientCredentials(OAuthGrantClientCredentialsContext context)
        {
            var app =
                await new ApplicationDbContext().Apps.FirstOrDefaultAsync(c => c.ClientId == context.ClientId);
            if (app != null)
            {
                var user = await new ApplicationDbContext().Users.FirstOrDefaultAsync(u => u.UserName == app.Username);

                var identity = new ClaimsIdentity("SidekickOAuth");
                var claims = new List<Claim>
                             {
                                 new Claim(ClaimTypes.Name, user.UserName),
                                 new Claim(ClaimTypes.Email, user.Email),
                                 new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                                 new Claim(ClaimTypes.Sid, user.Id),
                                 new Claim(ClaimTypes.GivenName, user.Fullname),
        

                                 new Claim("sidekick.client.name", app.Username),
                                 new Claim("sidekick.client.appId", app.Id.ToString()),
                                 new Claim("sidekick.client.meta", app.Meta),
                                 new Claim("sidekick.client.istrusted", app.IsTrusted.ToString()),
                                 new Claim(ClaimTypes.Expiration, app.AccessTokenExpiry.ToString()),


                             };

                identity.AddClaims(claims);
                context.Validated();

            }
            else
            {
                context.Rejected();
            }

        }
    }
}