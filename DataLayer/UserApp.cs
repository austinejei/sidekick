using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataLayer
{
    public class UserApp
    {
        public UserApp()
        {
            UserAppScopes = new List<UserAppScope>();
        }
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }

        public DateTime DateInstalled { get; set; }
        public DateTime? DateUninstalled { get; set; }

        public bool IsInstalled { get; set; }

        public int AppId { get; set; }
        public virtual App App { get; set; }

        public string AccessToken { get; set; }
        public string HashedAccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string HashedRefreshToken { get; set; }

        //when this is reached.. caller can use refresh token, if granted, to get new access token
        public DateTime? AccessTokenExpiresOn { get; set; }

        //this is meant to tell us when the refresh token will expire..
        //meaning at the period at which the app will be required to go through the OAuth flow once more
        public DateTime? RefreshTokenExpiresOn { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<UserAppScope> UserAppScopes { get; set; }
    }
}