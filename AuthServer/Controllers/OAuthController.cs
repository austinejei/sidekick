using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using DataLayer;

namespace AuthServer.Controllers
{
    [Authorize]
    public class OAuthController : Controller
    {

        private readonly ApplicationDbContext _dbContext = new ApplicationDbContext();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Authorize(OAuthModel model)
        {
            var authentication = HttpContext.GetOwinContext().Authentication;
            var scopes = (Request.QueryString.Get("scope") ?? "").Split(' ');

            if (!string.IsNullOrEmpty(Request.Form.Get("submit.Grant")))
            {
                var identity = User.Identity as ClaimsIdentity;
                identity = new ClaimsIdentity(identity.Claims, "Bearer", identity.NameClaimType, identity.RoleClaimType);
                foreach (var scope in scopes)
                {
                    identity.AddClaim(new Claim("urn:oauth:scope", scope));
                }
                authentication.SignIn(identity);
            }

            var redirectUrl = Request.QueryString.Get("redirect_url") ?? "";

            if (!string.IsNullOrEmpty(Request.Form.Get("submit.Deny")))
            {
                return
                    RedirectPermanent(redirectUrl + "?error=access_denied&state=" + Request.QueryString.Get("state"));
            }

           
            return View();
        }

        // GET: OAuth
        public ActionResult Authorize()
        {


            var identity = User.Identity as ClaimsIdentity;

            if (identity == null)
            {
                return new HttpUnauthorizedResult();
            }


            var scopes = (Request.QueryString.Get("scope") ?? "").Split(' ');
            var clientId = Request.QueryString.Get("client_id") ?? "";

            //todo: check if app has been installed into user's account


            string msg;
            OAuthModel model;
            if (!ScopesAreValid(scopes, clientId, out msg,out model))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, msg);
           
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