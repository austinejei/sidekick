using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AuthServer.Components;
using DataLayer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace AuthServer.Controllers
{
    [Authorize]
    public class SsoController : Controller
    {
        private readonly ApplicationDbContext _dbContext = new ApplicationDbContext();

        // GET: sso?client_id=354yeghdsfc&action=signin&state=45eyrtgfsdbs3&redirect_url=https://www.getpostman.com/oauth2/callback
       
        public async Task<ActionResult> Index()
        {
            


            var identity = User.Identity as ClaimsIdentity;

            if (identity == null)
            {
                return new HttpUnauthorizedResult();
            }


            var clientId = Request.QueryString.Get("client_id") ?? "";
      
            var app = await _dbContext.Apps.FirstOrDefaultAsync(a => a.ClientId == clientId);
            var redirectUrl = Request.QueryString.Get("redirect_url") ?? "";
            var action = Request.QueryString.Get("action") ?? "";

            if (string.IsNullOrEmpty(redirectUrl))
            {
                return Content("redirectUrl is empty :(");
            }

            if (string.IsNullOrEmpty(action))
            {
                return
                   RedirectPermanent(redirectUrl + "?error=empty_action&state=" + Request.QueryString.Get("state"));
            }
            if (string.IsNullOrEmpty(redirectUrl))
            {
                return
                   RedirectPermanent(redirectUrl + "?error=empty_redirect_url&state=" + Request.QueryString.Get("state"));
            }
            if (app==null)
            {
  
                return
                    RedirectPermanent(redirectUrl + "?error=client_not_found&state=" + Request.QueryString.Get("state"));
            }
            if (!app.RedirectUrl.Equals(redirectUrl,StringComparison.CurrentCultureIgnoreCase))
            {

                return
                    RedirectPermanent(redirectUrl + "?error=redirecturl_mismatch&state=" + Request.QueryString.Get("state"));
            }
            if (!app.IsActive)
            {
                return
                   RedirectPermanent(redirectUrl + "?error=client_not_active&state=" + Request.QueryString.Get("state"));
            }


            if (!app.IsTrusted)
            {
                return
              RedirectPermanent(redirectUrl + "?error=client_not_trusted&state=" + Request.QueryString.Get("state"));
            }

            if (string.IsNullOrEmpty(app.Meta))
            {
                return
              RedirectPermanent(redirectUrl + "?error=sso_not_configured_for_client&state=" + Request.QueryString.Get("state"));
            }

            var meta = JObject.Parse(app.Meta);
             var allowSsoString = meta["allowSso"];
            if (allowSsoString == null)
            {

                  return
              RedirectPermanent(redirectUrl + "?error=sso_not_configured_for_client&state=" + Request.QueryString.Get("state"));
            }
            var allowSso = bool.Parse(allowSsoString.ToString());
            if (!allowSso)
            {
                return
                RedirectPermanent(redirectUrl + "?error=client_not_allowed_sso&state=" + Request.QueryString.Get("state"));
            }

            string ssoKey = app.SsoEncryptionKey;


            if (action.Equals("signin",StringComparison.CurrentCultureIgnoreCase))
            {
                var ssoToken = JsonWebToken.Encode(JsonConvert.SerializeObject(new
                {
                    Username = identity.Name,
                    Email =
                        identity.Claims.FirstOrDefault(
                            c => c.Type == ClaimTypes.Email)
                            .Value,
                    Name =
                        identity.Claims.FirstOrDefault(
                            c =>
                                c.Type ==
                                ClaimTypes.GivenName).Value,
                    Roles = identity.Claims.Where(c=>c.Type==ClaimTypes.Role).Select(r=>r.Value).ToList(),
                    EncKey = ssoKey,
                    ClientId = clientId,
                    Action = action
                }, new JsonSerializerSettings
                {
                    ContractResolver =
                        new CamelCasePropertyNamesContractResolver
                        ()
                }),
            ssoKey, JwtHashAlgorithm.HS512);


                return
                RedirectPermanent(redirectUrl + "?token=" + ssoToken + "&state=" + Request.QueryString.Get("state"));
            }

            if (action.Equals("signout", StringComparison.CurrentCultureIgnoreCase))
            {
                var authentication = HttpContext.GetOwinContext().Authentication;

                authentication.SignOut();
                var ssoToken = JsonWebToken.Encode(JsonConvert.SerializeObject(new
                                                                               {
                                                                                   Username = User.Identity.Name,
                                                                                   ClientId = clientId,
                                                                                   Action = action
                                                                               }, new JsonSerializerSettings
                                                                                  {
                                                                                      ContractResolver =
                                                                                          new CamelCasePropertyNamesContractResolver
                                                                                          ()
                                                                                  }),
                    ssoKey, JwtHashAlgorithm.HS512);


                return
                    RedirectPermanent(redirectUrl + "?token=" + ssoToken + "&state=" + Request.QueryString.Get("state"));
            }

            return Content("unknown action");

        }
    }

}