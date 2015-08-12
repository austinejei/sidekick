using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace DataLayer
{
    public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public DbSet<App> Apps { get; set; }

        public DbSet<AppScope> AppScopes { get; set; }

        public DbSet<OAuthScope> OAuthScopes { get; set; }

        public DbSet<UserApp> UserApps { get; set; }

        public DbSet<UserAppScope> UserAppScopes { get; set; }


        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}