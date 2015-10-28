namespace RelyingParty1.ViewModels
{
    using System.Collections.Generic;
    using System.Security.Claims;

    public class UserViewModel
    {
        public string UserName { get; set; }
        public IEnumerable<Claim> Claims { get; set; }
    }
}