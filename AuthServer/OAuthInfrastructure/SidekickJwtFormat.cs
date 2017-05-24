using System;
using System.Configuration;
using System.Data.Entity;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Text;
using DataLayer;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;

namespace AuthServer.OAuthInfrastructure
{
    public class SidekickJwtFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private readonly OAuthAuthorizationServerOptions _options;

        public SidekickJwtFormat(OAuthAuthorizationServerOptions options)
        {
            _options = options;

        }

        public string SignatureAlgorithm
        {
            get { return "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256"; }
        }

        public string DigestAlgorithm
        {
            get { return "http://www.w3.org/2001/04/xmlenc#hmac-sha256"; }
        }


        public string Protect(AuthenticationTicket data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }



            var tokenExpiry = TimeSpan.Parse(data.Identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Expiration).Value);

            var appId = int.Parse(data.Identity.Claims.FirstOrDefault(c => c.Type == "sidekick.client.appId").Value);

            var user = data.Identity.Name;

            var db = new ApplicationDbContext();

            var userApp = db.UserApps.FirstOrDefault(s => s.AppId == appId && s.Username == user);

            if (userApp?.AccessTokenExpiresOn != null)
            {
                if (DateTime.Now < userApp.AccessTokenExpiresOn.Value) //if not expired....
                {
                    if (!string.IsNullOrEmpty(userApp.HashedAccessToken))
                    {
                        return userApp.HashedAccessToken;
                    }

                }
            }


            var issuer = "http://oauth.sidekick.com";
            var audience = "developers.sidekick.com";
            var key = Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes("35476utyhjdvcadye6746574retdfghjn"))); //signing key, modify it during production
            var now = DateTime.Now;
            //var expires = now.AddMinutes(_options.AccessTokenExpireTimeSpan.TotalMinutes);
            var expires = now.AddMinutes(tokenExpiry.TotalMinutes);
            var signingCredentials = new SigningCredentials(new InMemorySymmetricSecurityKey(key), SignatureAlgorithm, DigestAlgorithm);




            var token = new JwtSecurityToken(issuer, audience, data.Identity.Claims, now, expires, signingCredentials);


            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);


            if (userApp != null)
            {
                userApp.HashedAccessToken = Guid.NewGuid().ToString("N");
                userApp.AccessToken = accessToken;
                userApp.UpdatedAt = DateTime.Now;
                userApp.AccessTokenExpiresOn = expires;
                db.Entry(userApp).State = EntityState.Modified;
                db.SaveChanges();

                return userApp.HashedAccessToken;
            }
            return accessToken;
        }

        public string Protect1(AuthenticationTicket data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

      
            var tokenExpiry = TimeSpan.Parse(data.Identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Expiration).Value);

            var issuer = "http://oauth.sidekick.com";
            var audience = "developers.sidekick.com";
            var key = Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes("35476utyhjdvcadye6746574retdfghjn"))); //signing key, modify it during production
            var now = DateTime.Now;
            //var expires = now.AddMinutes(_options.AccessTokenExpireTimeSpan.TotalMinutes);
            var expires = now.AddMinutes(tokenExpiry.TotalMinutes);
            var signingCredentials = new SigningCredentials(new InMemorySymmetricSecurityKey(key), SignatureAlgorithm,
                DigestAlgorithm);
            var token = new JwtSecurityToken(issuer, audience, data.Identity.Claims, now, expires, signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public AuthenticationTicket Unprotect(string protectedText)
        {
            throw new NotImplementedException();
        }
    }
}