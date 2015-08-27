using System;
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

        public string AppUrl { get; set; } //main point of entry for app

        public string RedirectUrl { get; set; }

        public string Meta { get; set; } //to contain stuff like ratelimit in json payload

        public virtual ICollection<AppScope> AppScopes  { get; set; }

        public string Username { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public bool IsOAuth { get; set; } //helps to determine whether to allow oauth

        public DateTime DateCreated { get; set; }

        public bool IsTrusted { get; set; } //if trusted, we give a refresh token
        public TimeSpan AccessTokenExpiry { get; set; }
        public string SsoEncryptionKey { get; set; }

//TTL for each access token
    }
}