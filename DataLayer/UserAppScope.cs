using System.ComponentModel.DataAnnotations;

namespace DataLayer
{
    public class UserAppScope
    {
        [Key]
        public int Id { get; set; }

        public int UserAppId { get; set; }
        public virtual UserApp UserApp { get; set; }

        public int OAuthScopeId { get; set; }
        public virtual OAuthScope OAuthScope { get; set; }

        public bool Enabled { get; set; }
    }
}