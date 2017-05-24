using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using ApiHandlers.CustomAttributes;
using DataLayer;
using NLog;

namespace ApiHandlers
{
    /// <summary>
    /// Provides info about a user
    /// </summary>
    [Authorize, RoutePrefix("v1/me")]
    public class MeController : ApiController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ApplicationDbContext _dbContext = new ApplicationDbContext();

        /// <summary>
        /// List details about a user
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route, OAuthScope("user.profile")]
        public async Task<IHttpActionResult> ListUserProfile()
        {
            Logger.Debug("received request to display user's profile");

            var userFromDb = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            
            var user = User.Identity as ClaimsIdentity;

            return Ok(new
                      {
                         userFromDb.Id,
                          Username = user.Name, userFromDb.Fullname,
                          userFromDb.Email, userFromDb.PhoneNumber,
                          userFromDb.IsActive,
                      });
        }
    }
}