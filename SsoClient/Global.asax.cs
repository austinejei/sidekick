using System;
using System.Collections.Generic;
using System.IdentityModel.Services;
using System.IdentityModel.Services.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SsoClient.Services;

namespace SsoClient
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            FederatedAuthentication.FederationConfigurationCreated += FederatedAuthentication_FederationConfigurationCreated;
         
        }

        private static void FederatedAuthentication_FederationConfigurationCreated(
           object sender,
           FederationConfigurationCreatedEventArgs e)
        {
            //from appsettings...
            const string domain = "";
            const bool requireSsl = false;
            const string certThumbprint = "8c64cb079328ec72d294cad7d4482d79b8431239";
            const string authCookieName = "RP1Auth";

            e.FederationConfiguration = new FederationConfiguration(true);

            
            //e.FederationConfiguration = FederationConfigurationFactory.Create(
            //    //InfrastructureConstants.Rp1Url,
            //    "http://localhost/SsoClient/", // the '/' is very necessary at the end. DON'T remove it
            //    //InfrastructureConstants.StsUrl + "token/get",
            //    "http://sidekick.local/sso/token",
            //    domain,
            //    certThumbprint,
            //    authCookieName,
            //    requireSsl);

            e.FederationConfiguration.IdentityConfiguration.ClaimsAuthenticationManager = new ClaimsAppender();


        }
    }
}
