

namespace RelyingParty1
{
    using System.IdentityModel.Services;
    using System.IdentityModel.Services.Configuration;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.Optimization;

    

    using RelyingParty1.Services;

    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            FederatedAuthentication.FederationConfigurationCreated += FederatedAuthentication_FederationConfigurationCreated;
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        private static void FederatedAuthentication_FederationConfigurationCreated(
            object sender,
            FederationConfigurationCreatedEventArgs e)
        {
            //from appsettings...
            const string Domain = "";
            const bool RequireSsl = false;
            const string CertThumbprint = "8c64cb079328ec72d294cad7d4482d79b8431239";
            const string AuthCookieName = "RP1Auth";

            e.FederationConfiguration = FederationConfigurationFactory.Create(
                //InfrastructureConstants.Rp1Url,
                "http://localhost/SidekickTest/", // the '/' is very necessary at the end. DON'T remove it
                //InfrastructureConstants.StsUrl + "token/get",
                "http://sidekick.local/sso/token",
                Domain,
                CertThumbprint,
                AuthCookieName,
                RequireSsl);
            e.FederationConfiguration.IdentityConfiguration.ClaimsAuthenticationManager = new ClaimsAppender();

        }
    }
}
