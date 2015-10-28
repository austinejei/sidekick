using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Xml;
using DataLayer;

namespace AuthServer.SsoInfrastructure.Factories
{
    public class ClaimsPrincipalFactory : IClaimsPrincipalFactory
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        public ClaimsPrincipal Create(string userName)
        {

            var user = _context.Users.FirstOrDefault(u => u.UserName == userName);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userName),

                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                new Claim(ClaimTypes.Sid, user.Id),
                new Claim(ClaimTypes.GivenName, user.Fullname),
                //for saml 2.0 cannot be password.
                new Claim(ClaimTypes.AuthenticationMethod, new Uri("http://sidekick.local").AbsoluteUri),
                new Claim("StsAccountId", user.Id),
                new Claim(ClaimTypes.NameIdentifier, "Sidekick"),
                new Claim(
                    "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider",
                    "Sidekick"),
                //fixes Exception - ID4270: The ‘AuthenticationInstant’ used to create a ‘SAML11′ AuthenticationStatement cannot be null.
                new Claim(
                    ClaimTypes.AuthenticationInstant,
                    XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.Utc))
            };

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Forms"));
        }
    }
}