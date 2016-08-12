using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Api.Common;

namespace SmsShortcodeHandler
{
    /// <summary>
    /// Main controller for SMS short-code handler
    /// </summary>
    [AllowAnonymous,RoutePrefix("smsapp"), SwHideInDocs]
    public class SmsController:ApiController
    {
        /// <summary>
        /// Main entry
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route,HttpGet]
        public async Task<IHttpActionResult> Index([FromUri] SmsModel model)
        {

            await Task.Delay(0);

            return new SmsResult(HttpStatusCode.OK, "welcome to sidekick SMS app :)");
        } 
    }
}
