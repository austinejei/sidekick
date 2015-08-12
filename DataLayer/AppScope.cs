using System.Security.Permissions;

namespace DataLayer
{
    public class AppScope
    {
        public int Id { get; set; }

        public int ApiAccountId { get; set; }
        public virtual App App { get; set; }

        public int OAuthScopeId { get; set; }
        public virtual OAuthScope OAuthScope { get; set; }
    }
}