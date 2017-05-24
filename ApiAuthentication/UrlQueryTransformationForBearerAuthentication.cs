using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;
using NLog;

namespace ApiAuthentication
{
    public class UrlQueryTransformationForBearerAuthentication : OwinMiddleware
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly OwinMiddleware _nextMiddleware;

        public UrlQueryTransformationForBearerAuthentication(OwinMiddleware next) : base(next)
        {

            _nextMiddleware = next;
        }


        public override async Task Invoke(IOwinContext context)
        {

            Logger.Debug("about to inspect URL query for access_token param");

            if (context.Request.Uri.ToString().ToLower().Contains("access_token"))
            {

                var query = context.Request.Uri.Query.ToLower();

                var accessToken = HttpUtility.ParseQueryString(query)["access_token"];
                
                context.Request.Headers["Authorization"] = "Bearer " + accessToken;
            }

            await _nextMiddleware.Invoke(context);
        }
    }
}