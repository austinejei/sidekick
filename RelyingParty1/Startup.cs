using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Sidekick.OAuthClient;

[assembly: OwinStartup(typeof(RelyingParty1.Startup))]

namespace RelyingParty1
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
           
        }
    }
}
