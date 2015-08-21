using System.Threading.Tasks;
using System.Web.Http;
using Smsgh.UssdFramework;
using Smsgh.UssdFramework.Stores;

namespace UssdAppHandler
{
    [RoutePrefix("ussdapp"),AllowAnonymous]
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
