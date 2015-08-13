using System;
using System.Configuration;
using System.Text;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;

namespace ApiAuthentication
{
    public class BearerAuthentication : JwtBearerAuthenticationOptions
    {

        public BearerAuthentication() 
        {
            var issuer = "http://oauth.sidekick.com";
            var audience = "developers.sidekick.com";
            var key = Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes("35476utyhjdvcadye6746574retdfghjn"))); ;

            AuthenticationMode = AuthenticationMode.Active;
            AllowedAudiences = new[] { audience };
            IssuerSecurityTokenProviders = new[]
                                           {
                                               new SymmetricKeyIssuerSecurityTokenProvider(issuer, key)
                                           };
        }

       
    }

   
}
