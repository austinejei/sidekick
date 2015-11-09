using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        public ActionResult Get()
        {
            return Content("User: " + User.Identity.Name);
        }
    }
}