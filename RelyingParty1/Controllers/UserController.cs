namespace RelyingParty1.Controllers
{
    using System.Linq;
    using System.Security.Claims;
    using System.Web.Mvc;

    using ViewModels;

    [Authorize]
    public class UserController : Controller
    {
        [Route("~/")]
        [Route("user/get")]
        [HttpGet]
        public ViewResult Get()
        {

            ViewBag.Name = User.Identity.Name;
            return
                this.View(
                    new UserViewModel
                        {
                            UserName = ClaimsPrincipal.Current.Claims.First(c => c.Type == ClaimTypes.Name)
                                .Value,
                            Claims = ClaimsPrincipal.Current.Claims
                        });            
        }
    }
}