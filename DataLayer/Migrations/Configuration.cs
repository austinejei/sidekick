using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace DataLayer.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<DataLayer.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(DataLayer.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            context.OAuthScopes.AddOrUpdate(o => o.Name, new[]
                                                         {
                                                             new OAuthScope
                                                             {
                                                                 Id=1,
                                                                 Name = "user.profile",
                                                                 Alias = "Read Profile",
                                                                 Description = "Read your profile details"
                                                             },
                                                             new OAuthScope
                                                             {
                                                                 Id = 2,
                                                                 Name = "emails.send",
                                                                 Alias = "Send Emails",
                                                                 Description = "Send emails on your behalf"
                                                             }
                                                         });


            context.Apps.AddOrUpdate(a => a.ClientId, new[]
                                                      {
                                                          new App
                                                          {
                                                              Id = 1,
                                                              Name = "OneStep App",
                                                              Description = "This app is awesome. It helps you to do amazing stuff like send super fast email. Try it out!",
                                                              ClientId = "354yeghdsfc",
                                                              ClientSecret = "ytfghnfg454",
                                                              DateCreated = DateTime.Now,
                                                              //RedirectUrl = "https://developers.google.com/oauthplayground",
                                                              RedirectUrl = "http://localhost/WebApplication1/oauthclient/callback",
                                                              Username = "kwaku.dev@sidekick.com",
                                                              IsActive=true,
                                                              IsTrusted = true,
                                                              
                                                              AccessTokenExpiryTicks = TimeSpan.FromHours(2).Ticks,
                                                              RefreshTokenExpiryTicks = TimeSpan.FromDays(30).Ticks,
                                                              AppUrl ="http://localhost/ssoclient/",
                                                              SsoEncryptionKey = "b4f5aa91cc5110ae69eda952a4ab5a024c1dd764",
                                                              Meta = "{\"rateLimit\":2000,\"allowSso\":true}", //API rate limit,
                                                              IsOAuth = true,AllowedIp = "*",
                                                              SsoUrl = "http://localhost/ssoclient/",
                                                              

                                                          }
                                                      });

            context.AppScopes.AddOrUpdate(s=>s.Id,new []
                                                  {
                                                      new AppScope
                                                      {
                                                          Id = 1,
                                                          AppId = 1,
                                                          OAuthScopeId=1, 
                                                      },
                                                       new AppScope
                                                      {
                                                          Id = 2,
                                                          AppId = 1,
                                                          OAuthScopeId=2
                                                      }
                                                  });

            if (!context.Users.Any(u => u.UserName == "kwame.user@sidekick.com"))
            {
                var store = new UserStore<ApplicationUser>(context);
                var manager = new UserManager<ApplicationUser>(store);
                var user = new ApplicationUser
                {
                    UserName = "kwame.user@sidekick.com",
                    Email = "kwame.user@sidekick.com",

                    EmailConfirmed = true,
                    //DateCreated = DateTime.Now,
                    Fullname = "Kwame Test User",
                    PhoneNumber = "0244123456",
                  
                    PhoneNumberConfirmed = true,
                    IsActive =true,
                };

                manager.Create(user, "sidekick_user_2015");

            }


            if (!context.Users.Any(u => u.UserName == "kwaku.dev@sidekick.com"))
            {
                var store = new UserStore<ApplicationUser>(context);
                var manager = new UserManager<ApplicationUser>(store);
                var user = new ApplicationUser
                {
                    UserName = "kwaku.dev@sidekick.com",
                    Email = "kwaku.dev@sidekick.com",

                    EmailConfirmed = true,
                    //DateCreated = DateTime.Now,
                    Fullname = "Kwaku Great Developer",
                    PhoneNumber = "0277243123",

                    PhoneNumberConfirmed = true,
                    IsActive = true,
                };

                manager.Create(user, "sidekick_dev_2015");

            }
        }
    }
}
