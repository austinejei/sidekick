using System.Collections.Generic;

namespace DataLayer
{
    public class App
    {
        public App()
        {
            AppScopes = new List<AppScope>();
        }
        public int Id { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public bool IsActive { get; set; }

        public string Meta { get; set; } //to contain stuff like ratelimit in json payload

        public virtual ICollection<AppScope> AppScopes  { get; set; }
        public int DeveloperId { get; set; }

    }
}