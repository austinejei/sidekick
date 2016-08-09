using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DataLayer;
using Microsoft.Owin;
using NLog;

namespace ApiAuthentication
{
    public class BasicAuthentication :OwinMiddleware
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly OwinMiddleware _nextMiddleware;

        private readonly ApplicationDbContext _dbContext;

        private List<string> _allowedIPs;

        public BasicAuthentication(OwinMiddleware next) : base(next)
        {
           _dbContext = new ApplicationDbContext();
            _nextMiddleware = next;
            _allowedIPs = ConfigurationManager.AppSettings["AllowedIPs"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }


        public override async Task Invoke(IOwinContext context)
        {
            
            var header = context.Request.Headers.Get("Authorization");

            if (!string.IsNullOrWhiteSpace(header))
            {
                var authHeader = AuthenticationHeaderValue.Parse(header);

                if ("Basic".Equals(authHeader.Scheme,
                    StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Debug("performing Basic authentication");


                    bool exceptionThrown = false;
                  
         
                    string[] parameters = {"a","b"};

                    App app = null;
                    try
                    {
                        parameters = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter)).Split(':');

                       

                        var publicKey = parameters[0];
                        var privateKey = parameters[1];

                        app = await _dbContext.Apps.FirstOrDefaultAsync(
                            t => t.ClientId == publicKey && t.ClientSecret == privateKey);

                  
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(exception.Message);
                        exceptionThrown = true;
                    }

           

                    if (app != null)
                    {
                        if (!app.IsActive)
                        {
                            Logger.Debug("Unsuccessful authorization: your account is not active");
                            context.Response.StatusCode = 403;
                            //context.Response.Headers.Add("WWW-Authenticate",new []{"Basic realm=\"USP\""});
                            await
                                context.Response.WriteAsync(string.Format("Your account status is {0}",
                                    app.IsActive));
                        }
                        else
                        {
                            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == app.Username);

                            var claims = new List<Claim>
                                         {
                                             new Claim(ClaimTypes.Name, user.UserName),
                                             new Claim(ClaimTypes.Sid, user.Id),
                                           
                                             new Claim(ClaimTypes.Email, user.Email),
                                             new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                                             new Claim(ClaimTypes.GivenName, user.Fullname),
                                             new Claim(ClaimTypes.SerialNumber,
                                                 app.Id.ToString()),

                                         };

                            var identity = new ClaimsIdentity(claims, "Basic");

                            context.Request.User = new ClaimsPrincipal(identity);

                            Logger.Info("successfully authenticated basic auth");
                            await _nextMiddleware.Invoke(context);
                        }

                    }
                    else if (exceptionThrown)
                    {
                        context.Response.StatusCode = 401;
                        //context.Response.Headers.Add("WWW-Authenticate", new[] { "Basic realm=\"USP\"" });
                        await context.Response.WriteAsync(string.Format("Invalid API credentials"));
                    }
                    else
                    {
                        Logger.Warn("Invalid API credential {0}", authHeader.Parameter);
                        context.Response.StatusCode = 401;
                        // context.Response.Headers.Add("WWW-Authenticate", new[] { "Basic realm=\"USP\"" });
                        await context.Response.WriteAsync(string.Format("Invalid API credentials"));
                    }



                }
                else
                {
                    await _nextMiddleware.Invoke(context);
                }
            }
            else
            {
                if (context.Request.Uri.ToString().ToLower().Contains("ping") || context.Request.Uri.ToString().ToLower().Contains("signalr"))
                {
                    await _nextMiddleware.Invoke(context);
                }
                else
                {
                    Logger.Info("no authentication header detected...request will be failed!");
                    context.Response.StatusCode = 401;
                    //context.Response.Headers.Add("WWW-Authenticate", new[] { "Basic realm=\"USP\"" });
                    await context.Response.WriteAsync(string.Format("no authorization header detected"));
                }

            }
        }
    }
}
