using System;
using System.Collections.Generic;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using AuthServer.SsoInfrastructure.Factories;
using AuthServer.SsoInfrastructure.Services;
using AuthServer.SsoInfrastructure.ViewModels;

namespace AuthServer.Controllers
{
    [RoutePrefix("sso")]
    [Authorize]
    public class SsoController: Controller
    {
        private readonly IClaimsPrincipalFactory _claimsPrincipalFactory;

        private readonly HttpContextBase httpContextBase;

        private readonly IRealmTracker realmTracker;

        public SsoController(IClaimsPrincipalFactory claimsPrincipalFactory, HttpContextBase httpContextBase, IRealmTracker realmTracker)
        {
            this._claimsPrincipalFactory = claimsPrincipalFactory;
            this.httpContextBase = httpContextBase;
            this.realmTracker = realmTracker;
        }

#region Sigin SSO


        [Route("signin/{returnurl?}"),HttpGet]
        public ActionResult Signin(string returnUrl)
        {
            //return View(new SignInViewModel { ReturnUrl = returnUrl });

            return SignInSso(new SignInViewModel { ReturnUrl = returnUrl });
        }

        //[Route("signin"), HttpPost, ValidateAntiForgeryToken]
        //public ActionResult Signin(SignInViewModel signInViewModel)
        //{
        //    if (!ValidateLoginAgainstDatabase())
        //    {
        //        return View();
        //    }

        //    return SignInSso(signInViewModel);
        //}

        private RedirectResult SignInSso(SignInViewModel signInViewModel)
        {
            if (!ClaimsPrincipal.Current.Identity.IsAuthenticated)
            {
                CreateGeneralClaimsAndSerializeToStsCookie(User.Identity.Name);
            }

            return new RedirectResult(signInViewModel.ReturnUrl);
        }

        private void CreateGeneralClaimsAndSerializeToStsCookie(string userName)
        {
            var principal = _claimsPrincipalFactory.Create(userName);
            var outputprincipal = AuthenticatePrincipalAndSerialize(principal);
            Thread.CurrentPrincipal = outputprincipal;
        }

        private static ClaimsPrincipal AuthenticatePrincipalAndSerialize(ClaimsPrincipal principal)
        {
            var outputprincipal =
                FederatedAuthentication.FederationConfiguration.IdentityConfiguration.ClaimsAuthenticationManager
                    .Authenticate(string.Empty, principal);

            var sessionToken = new SessionSecurityToken(outputprincipal, TimeSpan.FromHours(1)) { IsPersistent = true };

            FederatedAuthentication.SessionAuthenticationModule.WriteSessionTokenToCookie(sessionToken);
            return outputprincipal;
        }

#endregion

#region SSO token

        [Route("token")]
        [HttpGet]
        public ContentResult Token()
        {
            var wsFederationMessage = ValidateRequestType();
            ValidateWsFederationMessage(wsFederationMessage);

            var samlTokenService = new SamlTokenService(
                new RealmTracker(HttpContext),
                new SecurityTokenServiceConfigurationFactory());
            var signInResponseMessage = samlTokenService.CreateResponseContainingToken(HttpContext.Request.Url);

            return new ContentResult { Content = signInResponseMessage.WriteFormPost() };
        }

        private static void ValidateWsFederationMessage(WSFederationMessage wsFederationMessage)
        {
            var signInRequestMessage = wsFederationMessage as SignInRequestMessage;
            if (signInRequestMessage == null)
            {
                throw new SecurityException("The WSFederationMessage is not a SignIn Message.");
            }
        }

        private static WSFederationMessage ValidateRequestType()
        {
            WSFederationMessage wsFederationMessage;
            if (!WSFederationMessage.TryCreateFromUri(System.Web.HttpContext.Current.Request.Url, out wsFederationMessage))
            {
                throw new SecurityException("This is not a WsFederation compliant request");
            }
            return wsFederationMessage;
        }

#endregion

        #region SignOut SSO
        [Route("signout"), HttpGet]
        public ActionResult SignOut()
        {
            if (!ClaimsPrincipal.Current.Identity.IsAuthenticated)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            var message = WSFederationMessage.CreateFromUri(httpContextBase.Request.Url);

            var signoutMessage = message as SignOutRequestMessage;
            if (signoutMessage != null)
            {
                return SignOutSso(signoutMessage);
            }

            throw new ApplicationException("We only deal with signouts here");
        }

        private ActionResult SignOutSso(SignOutRequestMessage signOutRequestMessage)
        {
            SignOutTheSts();

            var realmsToSignOut = realmTracker.ReadVisitedRealms();

            RemoveTheRpThatSignedOutFromRealmsToSignOut(signOutRequestMessage, realmsToSignOut);
            RemoveStsSessionCookie();

            return View(
                "Signout",
                new SignOutViewModel { ReturnUrl = signOutRequestMessage.Reply, RealmsToSignOut = realmsToSignOut });
        }

        private static void SignOutTheSts()
        {
            
                FederatedAuthentication.SessionAuthenticationModule.SignOut();
            
           
            
        }

        private static void RemoveTheRpThatSignedOutFromRealmsToSignOut(
            SignOutRequestMessage signOutRequestMessage,
            ICollection<string> realmsToSignOut)
        {
            if (string.IsNullOrWhiteSpace(signOutRequestMessage.Reply))
            {
                return;
            }

            realmsToSignOut.Remove(realmsToSignOut.FirstOrDefault(s => signOutRequestMessage.Reply.Contains(s)));
        }

        private void RemoveStsSessionCookie()
        {
            var sessionCookie = httpContextBase.Request.Cookies["ASP.Net_SessionId"];

            if (sessionCookie != null)
            {
                sessionCookie.Value = "";
                sessionCookie.Expires = new DateTime(2000, 1, 1);
                sessionCookie.Path = HttpRuntime.AppDomainAppVirtualPath;

                httpContextBase.Response.SetCookie(sessionCookie);
            }
        }
        #endregion

    }
}