using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using DataLayer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApiContrib.Caching;
using WebApiContrib.MessageHandlers;

namespace ApiDelegatingHandlers
{
    public class RateLimitDelegatingHandler:ThrottlingHandler
    {


       
        public RateLimitDelegatingHandler() : base(new InMemoryThrottleStore(), c =>
                                                                                {
                                                                                    const int defaultRateLimit = 60;
                                                                                    try
                                                                                    {
                                                                                        var metaData = JObject.Parse(c);
                                                                                        var rateLimit = metaData["rateLimit"];
                                                                                        if (rateLimit != null)
                                                                                        {
                                                                                            return
                                                                                                int.Parse(
                                                                                                    rateLimit
                                                                                                        .ToString());
                                                                                        }
                                                                                    }
                                                                                    catch (Exception ex)
                                                                                    {

                                                                                    }



                                                                                    return defaultRateLimit;
                                                                                }, TimeSpan.FromMinutes(1), "Too many requests. Please try again later or contact Sidekick technical team to increase your rate limit")
        {
        }

        protected override string GetUserIdentifier(HttpRequestMessage request)
        {
            var user = request.GetRequestContext().Principal;
            if (user != null)
            {
                var userPrincipal = user.Identity as ClaimsIdentity;

                if (userPrincipal == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        rateLimit = 300,
                        allowSso = false
                    });
                }

                var metaClaim = userPrincipal.FindFirst("sidekick.client.meta");

                if (metaClaim == null)
                {

                    return JsonConvert.SerializeObject(new
                    {
                        rateLimit = 30,
                        allowSso = false
                    });
                }
                return metaClaim.Value;
            }
            if (request.RequestUri.ToString().Contains("swagger"))
            {
                return JsonConvert.SerializeObject(new
                {
                    rateLimit = 30000,
                    allowSso = false
                });
            }
            return base.GetUserIdentifier(request);
        }
    }
}
