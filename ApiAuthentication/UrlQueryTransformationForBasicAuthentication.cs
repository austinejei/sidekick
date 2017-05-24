using System;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;

using NLog;

namespace ApiAuthentication
{
    public class UrlQueryTransformationForBasicAuthentication : OwinMiddleware
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly OwinMiddleware _nextMiddleware;

        public UrlQueryTransformationForBasicAuthentication(OwinMiddleware next) : base(next)
        {
            
            _nextMiddleware = next;
        }


        public override async Task Invoke(IOwinContext context)
        {

            Logger.Debug("about to inspect URL query for ClientId and ClientSecret params");

            if (context.Request.Uri.ToString().ToLower().Contains("clientid") && context.Request.Uri.ToString().ToLower().Contains("clientsecret"))
            {

                var query = context.Request.Uri.Query.ToLower();

                var clientId = HttpUtility.ParseQueryString(query)["clientid"];
                var clientSecret = HttpUtility.ParseQueryString(query)["clientSecret"];


                var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
                context.Request.Headers["Authorization"] = "Basic " + auth;

          
            }

            await _nextMiddleware.Invoke(context);
        }
    }
}