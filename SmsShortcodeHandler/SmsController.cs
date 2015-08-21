using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace SmsShortcodeHandler
{
    [AllowAnonymous,RoutePrefix("smsapp")]
    public class SmsController:ApiController
    {
        [Route,HttpGet]
        public async Task<IHttpActionResult> Index([FromUri] SmsModel model)
        {

            await Task.Delay(0);

            return new SmsResult(HttpStatusCode.OK, "welcome to sidekick SMS app :)");
        } 
    }
}
