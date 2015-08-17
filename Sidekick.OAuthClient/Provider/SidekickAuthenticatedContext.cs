using System.Security.Claims;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Provider;
using Newtonsoft.Json.Linq;

namespace Sidekick.OAuthClient.Provider
{
    public class SidekickAuthenticatedContext:BaseContext
    {
        /// <summary>
        /// Initializes a <see cref="SidekickAuthenticatedContext"/>
        /// </summary>
        /// <param name="context">The OWIN environment</param>
        /// <param name="user">The JSON-serialized user</param>
        /// <param name="accessToken">Unity Access token</param>
        /// <param name="refreshToken">Unity Refresh token</param>
        public SidekickAuthenticatedContext(IOwinContext context, JObject user, string accessToken, string refreshToken)
            : base(context)
        {
            User = user;
            AccessToken = accessToken;
            RefreshToken = refreshToken;

            Id = TryGetValue(user, "Id");

            UserName = TryGetValue(user, "Fullname");
            MobileNumber = TryGetValue(user, "PhoneNumber");


            Email = TryGetValue(user, "Email");
            FirstName = TryGetValue(user, "Fullname").Split(' ')[0];
            LastName = TryGetValue(user, "Fullname").Split(' ')[1];

            Active = bool.Parse(TryGetValue(user, "IsActive"));
        }



        /// <summary>
        /// Gets the JSON-serialized user
        /// </summary>
        /// <remarks>
        /// Contains the Sidekick user obtained from the User Info endpoint. By default this is https://api.smsgh.com/v3/account/profile but it can be
        /// overridden in the options
        /// </remarks>
        public JObject User { get; private set; }

        /// <summary>
        /// Gets the Sidekick access token
        /// </summary>
        public string AccessToken { get; private set; }

        /// <summary>
        /// Gets the Sidekick refresh token, if the application's scope allows it
        /// </summary>
        public string RefreshToken { get; private set; }

        /// <summary>
        /// Gets the Sidekick ID / User Info Endpoint
        /// </summary>
        public string Id { get; private set; }




        /// <summary>
        /// Gets the Sidekick User Name
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// Gets the Sidekick Mobile Number
        /// </summary>
        public string MobileNumber { get; private set; }

  

        /// <summary>
        /// Gets the Sidekick User Email
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Gets the user's First Name
        /// </summary>
        public string FirstName { get; private set; }

        /// <summary>
        /// Gets the user's name
        /// </summary>
        public string LastName { get; private set; }

        /// <summary>
        /// Gets the user's Status
        /// </summary>
        public bool Active { get; private set; }

        /// <summary>
        /// Gets the <see cref="ClaimsIdentity"/> representing the user
        /// </summary>
        public ClaimsIdentity Identity { get; set; }

        /// <summary>
        /// Gets or sets a property bag for common authentication properties
        /// </summary>
        public AuthenticationProperties Properties { get; set; }

        private static string TryGetValue(JObject user, string propertyName)
        {
            JToken value;
            return user.TryGetValue(propertyName, out value) ? value.ToString() : null;
        }
    }
}