using System.ComponentModel.DataAnnotations;

namespace DataLayer
{
    public class UserAppScope
    {
        [Key]
        public int Id { get; set; }

        public int AppId { get; set; }
        public virtual App App { get; set; }

        public int OAuthScopeId { get; set; }
        public virtual OAuthScope OAuthScope { get; set; }

        public bool Enabled { get; set; }
        public string Username { get; set; }
    }
}