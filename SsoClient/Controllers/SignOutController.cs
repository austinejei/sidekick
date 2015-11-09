using System;
using System.IdentityModel.Services;
using System.Web.Mvc;

namespace SsoClient.Controllers
{
    [Authorize]
    public class SignOutController : Controller
    {
        [Route("signout/get")]
        [HttpGet]
        public RedirectResult Get()
        {
            //from config in prod
            
            const string DefaultViewInRp = "User/Get";

            var federationAuthenticationModule = FederatedAuthentication.WSFederationAuthenticationModule;
            federationAuthenticationModule.SignOut(false); //not initiated by sts so false...

            var signOutRequest = new SignOutRequestMessage(new Uri("http://sidekick.local/sso/signout"))
                                     {
                                         Reply =
                                             this.Request.UrlReferrer != null
                                                 ? this.Request.UrlReferrer
                                                       .AbsoluteUri
                                                 : federationAuthenticationModule
                                                       .Realm + DefaultViewInRp
            };

            return new RedirectResult(signOutRequest.WriteQueryString());
        }
    }
}