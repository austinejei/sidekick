using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DataLayer;
using Microsoft.Owin;
using NLog;

namespace ApiAuthentication
{
    public class IpAuthenticationMiddleware : OwinMiddleware
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly OwinMiddleware _nextMiddleware;
        private readonly ApplicationDbContext _dbContext;
        private List<string> _allowedIPs;
        public IpAuthenticationMiddleware(OwinMiddleware next) :
            base(next)
        {
            _nextMiddleware = next;
            _dbContext = new ApplicationDbContext();
            _allowedIPs = new List<string>()
            {
                {"127.0.0.1"}
            };
        }
        

        public override async Task Invoke(IOwinContext context)
        {

          
            var clientIp = context.Request.Headers["X-Forwarded-For"];
            var ipAddress = (string)context.Environment["server.RemoteIpAddress"];
            if (!string.IsNullOrEmpty(clientIp))
            {
                Logger.Info("X-Forwarded-For address is {0}", clientIp);
               
                var rawIpAddress = clientIp.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries)[0];
                //Logger.Debug("assigning");
                ipAddress = rawIpAddress;
            }



            Logger.Info("caller's API address is {0}", ipAddress);


            var app = context.Request.User as ClaimsPrincipal;

            if (app==null)
            {
                Logger.Fatal("User not found");
                context.Response.StatusCode = 404;
                context.Response.Headers.Add("Content-Type", new[] { "application/json" });

                await context.Response.WriteAsync("user not found");
                return;
            }
            var appIdClaim = app.FindFirst(x=>x.Type==ClaimTypes.SerialNumber);

            if (appIdClaim==null)
            {
                Logger.Fatal("app claim not found");
                context.Response.StatusCode = 404;
                context.Response.Headers.Add("Content-Type", new[] { "application/json" });

                await context.Response.WriteAsync("app claim not found");
                return;
            }

            var appId = int.Parse(appIdClaim.Value);
            var client = await _dbContext.Apps.FirstOrDefaultAsync(a => a.Id == appId);

            if (client==null)
            {
                Logger.Fatal("app claim not found");
                context.Response.StatusCode = 404;
                context.Response.Headers.Add("Content-Type", new[] { "application/json" });

                await context.Response.WriteAsync("app claim not found");
                return;
            }
            if (!string.IsNullOrEmpty(client.AllowedIp))
            {
                if (client.AllowedIp.Contains("*")) //merchant wants to allow any IP to access the service
                {
                    Logger.Debug("Whitelist contains *. Will allow any request to pass through");
                    await _nextMiddleware.Invoke(context);
                    return;
                }

                _allowedIPs.AddRange(client.AllowedIp.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList());

            }

            if (!_allowedIPs.Contains(ipAddress))
            {
                Logger.Fatal("IP, {0}, not allowed", ipAddress);
                context.Response.StatusCode = 403;
                context.Response.Headers.Add("Content-Type", new[] { "application/json" });

                await context.Response.WriteAsync("IP not allowed");
                return;
            }


            await _nextMiddleware.Invoke(context);

        }
    }
}