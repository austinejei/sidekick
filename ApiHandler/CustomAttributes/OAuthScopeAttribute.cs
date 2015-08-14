using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Newtonsoft.Json;

namespace ApiHandler.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class OAuthScopeAttribute : ActionFilterAttribute
    {
        
        public string Scopes { get; set; }

        public OAuthScopeAttribute(string scopes) //todo: mark as not null
        {
            Scopes = scopes;

        }

        public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var user = actionContext.RequestContext.Principal as ClaimsPrincipal;
            if (user==null)
            {
                HttpContent unauthorizedUserContent = new StringContent(JsonConvert.SerializeObject(new
                {
                    ErrorCode =
                        "401.1",
                    Description
                        = "Unauthorized user"
                        
                }));

                unauthorizedUserContent.Headers.Add("Content-Type", "application/json");
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    StatusCode =
                        HttpStatusCode
                        .BadRequest,
                    ReasonPhrase = "Unauthorized user",
                    Content = unauthorizedUserContent
                };
            }
            else
            {
                //check if this is a bearer
                if (user.Identity.AuthenticationType.Equals("jwt",StringComparison.CurrentCultureIgnoreCase))
                {
                    var scopesClaims = user.Claims.Where(c => c.Type == "urn:oauth:scope");


                    if (!scopesClaims.Any(c => Scopes.Contains(c.Value)))
                    {
                        HttpContent unauthorizedUserContent = new StringContent(JsonConvert.SerializeObject(new
                        {
                            ErrorCode =
                                "401.2",
                            Description
                                = "Your scope definition list does not include this resource"

                        }));

                        unauthorizedUserContent.Headers.Clear();
                        unauthorizedUserContent.Headers.Add("Content-Type", "application/json");
                        actionContext.Response = new HttpResponseMessage(HttpStatusCode.BadRequest)
                        {
                            StatusCode =
                                HttpStatusCode
                                .BadRequest,
                            ReasonPhrase = "Your scope definition list does not include this resource",
                            Content = unauthorizedUserContent
                        };
                    }

                }
               
            }
    


            return base.OnActionExecutingAsync(actionContext, cancellationToken);
        }
    }
}