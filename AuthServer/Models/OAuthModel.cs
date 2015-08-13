using System.Collections.Generic;

namespace AuthServer.Controllers
{
    public class OAuthModel
    {
        public OAuthModel()
        {
            Scopes = new List<AppScopeList>();
        }
        public string Developer { get; set; }
        public string AppName { get; set; }
        public string AppDescription { get; set; }
        public List<AppScopeList> Scopes { get; set; }
        public string DeveloperEmail { get; set; }
    }
}