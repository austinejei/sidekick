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
            const string certThumbprint = "‎‎b4f5aa91cc5110ae69eda952a4ab5a024c1dd764";
            const string authCookieName = "RP1Auth";

            


            e.FederationConfiguration = FederationConfigurationFactory.Create(
                //InfrastructureConstants.Rp1Url,
                "http://localhost/SsoClient/", // the '/' is very necessary at the end. DON'T remove it
                                               //InfrastructureConstants.StsUrl + "token/get",
                "http://sidekick.local/sso/token",
                domain,
                certThumbprint,
                authCookieName,
                requireSsl);

            e.FederationConfiguration.IdentityConfiguration.ClaimsAuthenticationManager = new ClaimsAppender();


        }
    }
}
