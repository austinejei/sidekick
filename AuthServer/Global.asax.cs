using System;
using System.Collections.Generic;
using System.IdentityModel.Services;
using System.IdentityModel.Services.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace AuthServer
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            FederatedAuthentication.FederationConfigurationCreated += FederatedAuthentication_FederationConfigurationCreated;
        }

        private static void FederatedAuthentication_FederationConfigurationCreated(
            object sender,
            FederationConfigurationCreatedEventArgs e)
        {
            //from appsettings...
            const string domain = "";
            const bool requireSsl = false;
            const string authCookieName = "Sidekick.StsAuth";

            e.FederationConfiguration.CookieHandler = new ChunkedCookieHandler
            {
                Domain = domain,
                Name = authCookieName,
                RequireSsl = requireSsl,
                PersistentSessionLifetime =
                    new TimeSpan(0, 0, 30, 0)
            };
        }
    }
}
