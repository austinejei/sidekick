using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using ApiHandler.CustomAttributes;
using DataLayer;
using NLog;

namespace ApiHandler
{
    [Authorize, RoutePrefix("v1/me")]
    public class MeController : ApiController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ApplicationDbContext _dbContext = new ApplicationDbContext();

        [HttpGet, Route, OAuthScope("user.profile")]
        public async Task<IHttpActionResult> ListUserProfile()
        {
            Logger.Debug("received request to display user's profile");

            var userFromDb = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            await Task.Delay(0);

            var user = User.Identity as ClaimsIdentity;

            return Ok(new
                      {
                          Id = user.Claims.FirstOrDefault(s=>s.Type==ClaimTypes.Sid).Value,
                          Username = user.Name,
                          Fullname = user.Claims.FirstOrDefault(s => s.Type == ClaimTypes.GivenName).Value,
                          Email=user.Claims.FirstOrDefault(s => s.Type == ClaimTypes.Email).Value,
                          PhoneNumber = user.Claims.FirstOrDefault(s => s.Type == ClaimTypes.MobilePhone).Value,
                          userFromDb.IsActive,
                      });
        }
    }
}