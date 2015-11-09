using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebApplication1.Controllers
{
    [AllowAnonymous]
    public class OAuthClientController :Controller
    {
        public async Task<ActionResult> Start()
        {
            await Task.Delay(0);

            var url =
                string.Format("http://sidekick.local/oauth/authorize?client_id={0}&scope={1}&state={2}&response_type={3}&redirect_uri={4}"
                    , "354yeghdsfc", "user.profile emails.send", Guid.NewGuid().ToString("N"), "code", "http://localhost/WebApplication1/oauthclient/callback");


            return Redirect(url);
        }

        public async Task<ActionResult> CallBack(string code)
        {

            var accessToken = string.Empty;
            using (var client = new HttpClient())
            {
               client.DefaultRequestHeaders.Clear();

                client.DefaultRequestHeaders.Add("Accept", "application/json");
                //client.DefaultRequestHeaders.Add("Content-Type", "x-www-form-urlencoded");

                HttpContent content = new FormUrlEncodedContent(new Dictionary<string,string>
                {
                    {"code",code},
                    {"client_id","354yeghdsfc"},
                    {"client_secret","ytfghnfg454"},
                    {"grant_type","authorization_code"},
                    {"scope","user.profile emails.send"},
                    {"redirect_uri","http://localhost/WebApplication1/oauthclient/callback"}
                });

                var result = await client.PostAsync(new Uri("http://sidekick.local/oauth/token"), content);

                if (result.IsSuccessStatusCode)
                {
                    var resultContent = await result.Content.ReadAsStringAsync();

                    accessToken = JObject.Parse(resultContent)["access_token"].ToString();
                }
            }


            if (string.IsNullOrEmpty(accessToken))
            {
                return new HttpUnauthorizedResult("oauth did not work!!");
            }

            var contentResult = string.Empty;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();

                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);



                var result = await client.GetAsync(new Uri("http://localhost:5342/v1/me"));

                if (result.IsSuccessStatusCode)
                {
                    contentResult = await result.Content.ReadAsStringAsync();
                }
            }

            if (string.IsNullOrEmpty(contentResult))
            {
                return new HttpUnauthorizedResult("oauth did not work!!");
            }

            var user = JsonConvert.DeserializeObject<ApiUser>(contentResult);

            var claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.Name,user.Username));
            claims.Add(new Claim(ClaimTypes.MobilePhone,user.PhoneNumber));
            claims.Add(new Claim(ClaimTypes.Email,user.Email));
            claims.Add(new Claim(ClaimTypes.NameIdentifier,user.Id));
            claims.Add(new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider",
                        "SidekickClient", ClaimValueTypes.String));

            var identity = new ClaimsIdentity("ApplicationCookie",
                    ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            identity.AddClaims(claims);
            //Thread.CurrentPrincipal = new ClaimsPrincipal(identity);

            HttpContext.GetOwinContext().Authentication.SignIn(identity);

            return RedirectToAction("Get", "User");
        }

    }

    public class ApiUser
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }

    }
}