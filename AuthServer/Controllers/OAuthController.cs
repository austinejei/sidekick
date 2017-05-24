using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages.Scope;
using DataLayer;

namespace AuthServer.Controllers
{
    [Authorize]
    public class OAuthController : Controller
    {

        private readonly ApplicationDbContext _dbContext = new ApplicationDbContext();

        public OAuthController()
        {
            
        }
        // GET: OAuth
        public async Task<ActionResult> Authorize()
        {
            var authentication = HttpContext.GetOwinContext().Authentication;

            var identity = User.Identity as ClaimsIdentity;

            if (identity == null)
            {
                return new HttpUnauthorizedResult();
            }



            var scopes = (Request.QueryString.Get("scope") ?? "");
            var scopeList = scopes.Split(' ');
            var clientId = Request.QueryString.Get("client_id") ?? "";

            var redirectUrl = Request.QueryString.Get("redirect_uri") ?? "";


            var app = await _dbContext.Apps.FirstOrDefaultAsync(a => a.ClientId == clientId);

            if (app==null)
            {
                return
                    RedirectPermanent(redirectUrl + "?error=client_not_found&state=" + Request.QueryString.Get("state"));
            }
            if (!app.IsOAuth)
            {
                return
                        RedirectPermanent(redirectUrl + "?error=client_not_configured_for_oauth&state=" + Request.QueryString.Get("state"));
            }
            if (!app.IsActive)
            {
                return
                    RedirectPermanent(redirectUrl + "?error=client_not_active&state=" + Request.QueryString.Get("state"));
            }
          
          
            string msg;
            OAuthModel model;
            if (!ScopesAreValid(scopeList, app, out msg, out model))
            {
                return
                    RedirectPermanent(redirectUrl +
                                      $"?error=scope_error&rror_description={msg}&state={Request.QueryString.Get("state")}");


            }

            var userExistingApp =
                   await
                       _dbContext.UserApps.FirstOrDefaultAsync(
                           a => a.AppId == app.Id && a.Username == User.Identity.Name);

            if (userExistingApp!=null)
            {
                if (!userExistingApp.IsInstalled)
                {
                    ViewBag.ReinstallMessage =
                        string.Format("Hi {0}, you uninstalled {1} on {2}. You need to grant permissions again.",
                            User.Identity.Name, app.Name, userExistingApp.DateUninstalled.Value.ToString("F"));
                }
                else
                {
                    identity = new ClaimsIdentity(identity.Claims, "Bearer", identity.NameClaimType, identity.RoleClaimType);
                    foreach (var scope in scopeList)
                    {
                        identity.AddClaim(new Claim("urn:oauth:scope", scope));
                    }




                    identity.AddClaim(new Claim(ClaimTypes.Expiration, app.AccessTokenExpiry.ToString()));
                    identity.AddClaim(new Claim("sidekick.client.istrusted", app.IsTrusted.ToString()));

                    identity.AddClaim(new Claim("sidekick.client.name", app.Username));
                    identity.AddClaim(new Claim("sidekick.client.meta", app.Meta));
                    identity.AddClaim(new Claim("sidekick.client.appId", app.Id.ToString()));
                    identity.AddClaim(new Claim("sidekick.client.appName", app.Name));
                    identity.AddClaim(new Claim("sidekick.client.refreshTokenExpiry", app.RefreshTokenExpiry.ToString()));
                    identity.AddClaim(new Claim("sidekick.client.allowedIps",
                        string.IsNullOrEmpty(app.AllowedIp) ? "*" : app.AllowedIp));



                    authentication.SignIn(identity);
                    return View(model);
                }
            }

            ViewBag.Scopes = scopes;
            ViewBag.ClientId = clientId;

            if (!string.IsNullOrEmpty(Request.Form.Get("submit.Grant")))
            {
          
              
               
                if (userExistingApp==null)//if it's not installed
                {
                    //install the app onto the user's account
                    var userApp = new UserApp
                    {
                        AppId = app.Id,
                        DateInstalled = DateTime.Now,
                        Username = User.Identity.Name,
                        IsInstalled = true,

                    };
                    _dbContext.UserApps.Add(userApp);
                    await _dbContext.SaveChangesAsync();

                    //copy the scopes into the user's account
                    //in the future, the user may tend to disable some scopes for this app
                    foreach (var appScope in app.AppScopes)
                    {
                        _dbContext.UserAppScopes.Add(new UserAppScope
                        {
                            UserAppId = userApp.Id,
                            Enabled = true,
                            OAuthScopeId = appScope.OAuthScopeId,
                        });
                    }

                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    if (!userExistingApp.IsInstalled) //it was uninstalled
                    {
                        userExistingApp.IsInstalled = true;
                        userExistingApp.DateUninstalled = null;
                        //the reason we're doing this is because, when the user uninstalls the app, we need to remove all the userappscope relations,
                        //then during installation, we get them back

                        var existingUserAppScopes =
                            _dbContext.UserAppScopes.Where(s => s.UserAppId == userExistingApp.Id);



                        var newUserAppScopes = new List<UserAppScope>();
                        foreach (var appScope in app.AppScopes)
                        {
                            newUserAppScopes.Add(new UserAppScope
                            {
                                UserAppId = userExistingApp.Id,
                                Enabled = true,
                                OAuthScopeId = appScope.OAuthScopeId,

                            });
                        }




                        var intersectingUserAppScopes = newUserAppScopes.Where(d => !existingUserAppScopes.Any(p => p.OAuthScopeId == d.OAuthScopeId));

                        var userAppScopes
                            = intersectingUserAppScopes as IList<UserAppScope> ?? intersectingUserAppScopes.ToList();
                        if (userAppScopes.Any())
                        {
                            _dbContext.UserAppScopes.AddRange(userAppScopes);
                        }


                        _dbContext.Entry(userExistingApp).State = EntityState.Modified;
                        await _dbContext.SaveChangesAsync();
                    }
                }

                identity = new ClaimsIdentity(identity.Claims, "Bearer", identity.NameClaimType, identity.RoleClaimType);
                foreach (var scope in scopeList)
                {
                    identity.AddClaim(new Claim("urn:oauth:scope", scope));
                }




                identity.AddClaim(new Claim(ClaimTypes.Expiration, app.AccessTokenExpiry.ToString()));
                identity.AddClaim(new Claim("sidekick.client.istrusted", app.IsTrusted.ToString()));

                identity.AddClaim(new Claim("sidekick.client.name", app.Username));
                identity.AddClaim(new Claim("sidekick.client.meta", app.Meta));
                identity.AddClaim(new Claim("sidekick.client.appId", app.Id.ToString()));
                identity.AddClaim(new Claim("sidekick.client.appName", app.Name));
                identity.AddClaim(new Claim("sidekick.client.refreshTokenExpiry", app.RefreshTokenExpiry.ToString()));
                identity.AddClaim(new Claim("sidekick.client.allowedIps",
                    string.IsNullOrEmpty(app.AllowedIp) ? "*" : app.AllowedIp));

                authentication.SignIn(identity);
            }
     

            if (!string.IsNullOrEmpty(Request.Form.Get("submit.Deny")))
            {
                return
               RedirectPermanent(redirectUrl + "?error=access_denied&state=" + Request.QueryString.Get("state"));

            }

            return View(model);
        }



        private bool ScopesAreValid(string[] scopes, App app, out string msg, out OAuthModel model)
        {
            if (scopes.Length == 0) //todo skip this if grant_type is SSO
            {
                msg = "no scopes found!";
                model = null;
                return false;
            }

            // var app = _dbContext.Apps.FirstOrDefault(a => a.ClientId == clientId);
            //if (app == null)
            //{
            //    msg = "invalid client id";
            //    model = null;
            //    return false;
            //}

            bool scopeOk = true;


            string errMsg = string.Empty;
            foreach (var scope in scopes)
            {
                if (app.AppScopes.FirstOrDefault(s => s.OAuthScope.Name == scope) == null)
                {
                    errMsg = string.Format("Scope, {0}, has not been registered with your account", scope);
                    scopeOk = false;
                    break;
                }
            }

            var oauthModel = new OAuthModel();

            if (scopeOk)
            {
                var developerInfo = _dbContext.Users.FirstOrDefault(u => u.UserName == app.Username);

                if (developerInfo == null)
                {
                    errMsg = "Developer\'s information cannot be found";
                    scopeOk = false;
                }
                else if (!developerInfo.IsActive)
                {
                    errMsg = string.Format("Developer, {0}, has been de-activated on AppTellSL", developerInfo.Fullname);
                    scopeOk = false;
                }
                else
                {
                    oauthModel = new OAuthModel
                    {
                        Developer = developerInfo.Fullname,
                        AppName = app.Name,
                        AppDescription = app.Description,
                        DeveloperEmail = developerInfo.Email,
                        Scopes = app.AppScopes.Select(s => new AppScopeList
                        {
                            Name = s.OAuthScope.Name,
                            Alias = s.OAuthScope.Alias,
                            Description = s.OAuthScope.Description,
                        }).ToList()
                    };
                }

            }



            msg = errMsg;
            model = oauthModel;
            return scopeOk;
        }
    }
}