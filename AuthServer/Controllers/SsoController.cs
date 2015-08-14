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
using Newtonsoft.Json.Serialization;

namespace AuthServer.Controllers
{
    [Authorize]
    public class SsoController : Controller
    {
        private readonly ApplicationDbContext _dbContext = new ApplicationDbContext();

        // GET: SSO
       
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

            if (string.IsNullOrEmpty(redirectUrl))
            {
                return
                   RedirectPermanent(redirectUrl + "?error=empty_redirect_url&state=" + Request.QueryString.Get("state"));
            }
            if (app==null)
            {
  
                return
                    RedirectPermanent(redirectUrl + "?error=access_denied&state=" + Request.QueryString.Get("state"));
            }

            if (app.IsActive)
            {
                return
                   RedirectPermanent(redirectUrl + "?error=client_not_active&state=" + Request.QueryString.Get("state"));
            }


            if (app.IsTrusted)
            {
                return
              RedirectPermanent(redirectUrl + "?error=client_not_trusted&state=" + Request.QueryString.Get("state"));
            }

            const string ssoKey = "345685uryjfhdvbsvbdfghjfgihotreywtreasdxgfvjhkilgkhmvn-=bsdrt3456"; //must be given to sso client for decryption

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
                                                                           }, new JsonSerializerSettings
                                                                              {
                                                                                  ContractResolver =
                                                                                      new CamelCasePropertyNamesContractResolver
                                                                                      ()
                                                                              }),
                ssoKey, JwtHashAlgorithm.HS512);


            return
            RedirectPermanent(redirectUrl + "?token="+ssoToken+"&state=" + Request.QueryString.Get("state"));
        }
    }

    public class SsoModel
    {
        public string Client { get; set; }
    
    }
}