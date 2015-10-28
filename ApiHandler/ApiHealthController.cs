using NLog;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http;

namespace ApiHandler
{
    /// <summary>
    /// Exposes the health of the API server
    /// </summary>
    [AllowAnonymous, RoutePrefix("v1/_ping")]
    public class ApiHealthController : ApiController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [HttpGet, Route]
        public async Task<IHttpActionResult> CurrentState()
        {
            Logger.Debug("received signal to check for API health");

            await Task.Delay(0);

            Logger.Info("API is alive and kicking.");
            return Ok(new
            {
                Status = ConfigurationManager.AppSettings["ApiState"]
            });
        }
    }
}