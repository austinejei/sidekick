using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DataLayer;

namespace AuthServer.Controllers
{
    [Authorize]
    public class SsoController : Controller
    {
       
    }
}