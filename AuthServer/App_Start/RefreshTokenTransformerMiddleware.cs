using System;
using System.Data.Entity;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DataLayer;
using Microsoft.Owin;

namespace AuthServer
{
    public class RefreshTokenTransformerMiddleware:OwinMiddleware
    {
        private readonly OwinMiddleware _nextMiddleware;
        public RefreshTokenTransformerMiddleware(OwinMiddleware next) : base(next)
        {
            _nextMiddleware = next;
        }

        public override async Task Invoke(IOwinContext context)
        {
            if ("POST".Equals(context.Request.Method,StringComparison.CurrentCultureIgnoreCase))
            {
                var streamBodyString = new StreamReader(context.Request.Body).ReadToEnd();
              

                if (!string.IsNullOrEmpty(streamBodyString))
                {
                    var streamBody = HttpUtility.ParseQueryString(streamBodyString);

                    var grantType = streamBody["grant_type"];

                    if ("refresh_token".Equals(grantType))
                    {
                        var refreshToken = streamBody["refresh_token"];

                        var _dbContext = new ApplicationDbContext();


                        var userApp =await 
                               _dbContext.UserApps.FirstOrDefaultAsync(
                                   x => x.HashedRefreshToken == refreshToken);


                        if (userApp == null)
                        {
                            context.Response.StatusCode = 401;
                            context.Response.Write("invalid refresh token");
                            return;
                        }

                        streamBody["refresh_token"] = userApp.RefreshToken;

                        //we write back the stream
                        
                          var requestData = Encoding.UTF8.GetBytes(streamBody.ToString());
                        context.Request.Body = new MemoryStream(requestData);

                        
                    }
                    else
                    {
                        var requestData = Encoding.UTF8.GetBytes(streamBodyString);
                        context.Request.Body = new MemoryStream(requestData);

                    }
                }
            }

            await _nextMiddleware.Invoke(context);
        }
    }
}