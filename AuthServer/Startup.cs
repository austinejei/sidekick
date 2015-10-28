using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AuthServer.Startup))]
namespace AuthServer
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            ConfigureSso(app);
        }
    }
}
