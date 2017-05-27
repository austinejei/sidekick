using System;
using System.Data.Entity;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Api.Common;
using DataLayer;
using Microsoft.Owin;

using NLog;

namespace ApiAuthentication
{
    public class BearerTokenTransformationMiddleware : OwinMiddleware
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly OwinMiddleware _nextMiddleware;

        private ApplicationDbContext _dbContext;

        public BearerTokenTransformationMiddleware(OwinMiddleware next) : base(next)
        {
           _dbContext = new ApplicationDbContext();
            _nextMiddleware = next;
        }


        public override async Task Invoke(IOwinContext context)
        {
            
            var header = context.Request.Headers.Get("Authorization");

            if (!string.IsNullOrWhiteSpace(header))
            {
                var authHeader = AuthenticationHeaderValue.Parse(header);

                if ("Bearer".Equals(authHeader.Scheme,
                    StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Debug("performing Bearer token transformation");

                    _dbContext = new ApplicationDbContext();
                    var userApp = await 
                        _dbContext.UserApps.FirstOrDefaultAsync(x => x.HashedAccessToken == authHeader.Parameter);

                    if (userApp==null)
                    {
                        Logger.Error("Bearer token {0} not found for any user",authHeader.Parameter);
                        context.Response.StatusCode = 404;
                        //context.Response.Headers.Add("WWW-Authenticate",new []{"Basic realm=\"USP\""});
                        await
                            context.Response.WriteAsync($"Bearer token,{authHeader.Parameter}, not found. Please restart the oauth flow");
                        return;
                    }

                    if (userApp.AccessTokenExpiresOn.HasValue)
                    {
                        if (DateTime.Now>userApp.AccessTokenExpiresOn.Value)
                        {
                            Logger.Error("Bearer token {0} is no longer valid. It was expired on {1}", authHeader.Parameter,userApp.AccessTokenExpiresOn);
                            context.Response.StatusCode = 401;
                            //context.Response.Headers.Add("WWW-Authenticate",new []{"Basic realm=\"USP\""});
                            await
                                context.Response.WriteAsync($"Bearer token,{authHeader.Parameter}, is no longer valid. It was expired on {userApp.AccessTokenExpiresOn}");
                            return;
                        }
                    }
                    context.Request.Headers["Authorization"] = "Bearer " + userApp.AccessToken;
                    
                    await _nextMiddleware.Invoke(context);
                }
                else
                {
                    await _nextMiddleware.Invoke(context);
                }
            }
            else
            {

                var routesToIgnore = new ApiHandlerHelper().GatherRoutesToIgnore();

                if (routesToIgnore.Any(route => context.Request.Uri.AbsolutePath.ToLower().Contains(route)))
                {
                    await _nextMiddleware.Invoke(context);
                    //return;
                }
             
                else
                {
                    await _nextMiddleware.Invoke(context);
                }

            }
        }
    }
}
