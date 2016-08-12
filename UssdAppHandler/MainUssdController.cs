using System.Threading.Tasks;
using System.Web.Http;
using Api.Common;
using Smsgh.UssdFramework;
using Smsgh.UssdFramework.Stores;

namespace UssdAppHandler
{

    /// <summary>
    /// Main controller for USSD service
    /// </summary>
    [RoutePrefix("ussdapp"),AllowAnonymous, SwHideInDocs]
    public class MainUssdController:ApiController
    {
        [Route,HttpPost]
        public async Task<IHttpActionResult> Index(UssdRequest request)
        {
            //todo: properly configure redis before proceeding....
            return Ok(await Ussd.Process(new RedisStore(), request, "Main", "Start"));
        } 
    }
}
