using System.Collections.Generic;

namespace AuthServer.SsoInfrastructure.ViewModels
{
    public class SignOutViewModel
    {
        public string ReturnUrl { get; set; }

        public IEnumerable<string> RealmsToSignOut { get; set; }
    }
}