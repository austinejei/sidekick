using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using DataLayer;
using Newtonsoft.Json.Linq;
using WebApiContrib.Caching;
using WebApiContrib.MessageHandlers;

namespace ApiDelegatingHandlers
{
    public class UserAwareThrottlingHandler:ThrottlingHandler
    {


       
        public UserAwareThrottlingHandler() : base(new InMemoryThrottleStore(), c =>
                                                                                {
                                                                                    const int defaultRateLimit = 60;
                                                                                    App thirdParty;
                                                                                    using (var thirdPartyRepository = new ApplicationDbContext())
                                                                                    {
                                                                                        thirdParty =
                                                                                            thirdPartyRepository.Apps.FirstOrDefault(
                                                                                                t => t.Name == c);
                                                                                    }

                                                                                    if (thirdParty!=null)
                                                                                    {
                                                                                        if (!string.IsNullOrEmpty(thirdParty.Meta))
                                                                                        {
                                                                                            var metaData = JObject.Parse(thirdParty.Meta);

                                                                                            var rateLimit = metaData["rateLimit"];
                                                                                            if (rateLimit != null)
                                                                                            {
                                                                                                return
                                                                                                    int.Parse(
                                                                                                        rateLimit
                                                                                                            .ToString());
                                                                                            }
                                                                                        }
                                                                                    }


                                                                                    return defaultRateLimit;
                                                                                }, TimeSpan.FromMinutes(1), "Too many requests. Please try again later or contact Sidekick technical team to increase your rate limit")
        {
        }

        protected override string GetUserIdentifier(HttpRequestMessage request)
        {
            var user = request.GetRequestContext().Principal;
            if (user!=null)
            {
                return user.Identity.Name;
            }
            return base.GetUserIdentifier(request);
        }
    }
}
