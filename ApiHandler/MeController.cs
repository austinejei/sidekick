using System;
using System.Configuration;
using System.Data.Entity;
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

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);


            return Ok(new
                      {
                          user.Id,
                          user.Fullname,
                          user.Email,
                          user.PhoneNumber,
                          user.IsActive,
                          
                      });
        }
    }
}