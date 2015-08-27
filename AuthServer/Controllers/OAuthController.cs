using System;
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

            var redirectUrl = Request.QueryString.Get("redirect_url") ?? "";


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
            if (!ScopesAreValid(scopeList, clientId, out msg,out model))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, msg);
           
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
                    _dbContext.UserApps.Add(new UserApp
                    {
                        AppId = app.Id,
                        DateInstalled = DateTime.Now,
                        Username = User.Identity.Name,

                    });

                    //copy the scopes into the user's account
                    //in the future, the user may tend to disable some scopes for this app
                    foreach (var appScope in app.AppScopes)
                    {
                        _dbContext.UserAppScopes.Add(new UserAppScope
                        {
                            AppId = app.Id,
                            Enabled = true,
                            OAuthScopeId = appScope.OAuthScopeId,
                            Username = User.Identity.Name
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
                        foreach (var appScope in app.AppScopes)
                        {
                            _dbContext.UserAppScopes.Add(new UserAppScope
                            {
                                AppId = app.Id,
                                Enabled = true,
                                OAuthScopeId = appScope.OAuthScopeId,
                                Username = User.Identity.Name
                            });
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


                authentication.SignIn(identity);
            }
     

            if (!string.IsNullOrEmpty(Request.Form.Get("submit.Deny")))
            {
                return
               RedirectPermanent(redirectUrl + "?error=access_denied&state=" + Request.QueryString.Get("state"));

            }

            return View(model);
        }

      

        private bool ScopesAreValid(string[] scopes, string clientId, out string msg, out OAuthModel model)
        {
            if (scopes.Length == 0) //todo skip this if grant_type is SSO
            {
                msg = "no scopes found!";
                model = null;
                return false;
            }

            var app = _dbContext.Apps.FirstOrDefault(a => a.ClientId == clientId);
            if (app == null)
            {
                msg = "invalid client id";
                model = null;
                return false;
            }

            bool scopeOk = true;
            var developerInfo = _dbContext.Users.FirstOrDefault(u => u.UserName == app.Username);
            var oauthModel = new OAuthModel
                             {
                                 Developer = developerInfo.Fullname,
                                 AppName = app.Name,
                                 AppDescription = app.Description,
                                 DeveloperEmail = developerInfo.Email,
                                 Scopes = app.AppScopes.Select(s => new AppScopeList
                                                                    {
                                                                        Name=s.OAuthScope.Name,
                                                                        Alias=s.OAuthScope.Alias,
                                                                        Description=s.OAuthScope.Description,
                                                                    }).ToList()
                             };


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

            msg = errMsg;
            model = oauthModel;
            return scopeOk;
        }
    }
}