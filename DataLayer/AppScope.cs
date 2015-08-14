using System.ComponentModel.DataAnnotations;
using System.Security.Permissions;

namespace DataLayer
{
    public class AppScope
    {
        [Key]
        public int Id { get; set; }

        public int AppId { get; set; }
        public virtual App App { get; set; }

        public int OAuthScopeId { get; set; }
        public virtual OAuthScope OAuthScope { get; set; }
    }
}