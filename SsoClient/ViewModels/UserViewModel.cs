using System.Collections.Generic;
using System.Security.Claims;

namespace SsoClient.ViewModels
{
    public class UserViewModel
    {
        public string UserName { get; set; }
        public IEnumerable<Claim> Claims { get; set; }
    }
}