using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Umbraco.Web.Mvc;
using Uppgift2.Core.Interfaces;
using Uppgift2.Core.ViewModel;
using Umbraco.Core.Logging;

namespace Uppgift2.Core.Controllers
{
    public class LoginController : SurfaceController
    {
        public const string PARTIAL_VIEW_FOLDER = "~/Views/Partials/Login/";
        private IEmailService _emailService;

        public LoginController(IEmailService emailService)
        {
            _emailService = emailService;
        }
        #region Login
        public ActionResult RenderLogin()
        {
            var vm = new LoginViewModel();
            vm.RedirectUrl = HttpContext.Request.Url.AbsolutePath;
            return PartialView(PARTIAL_VIEW_FOLDER + "Login.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandleLogin(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            var member = Services.MemberService.GetByUsername(vm.Username);
            if (member == null)
            {
                ModelState.AddModelError("Login", "User does not exist");
                return CurrentUmbracoPage();
            }

            if (member.IsLockedOut)
            {
                ModelState.AddModelError("Login", "The account is locked, please reset your password");
                return CurrentUmbracoPage();
            }

            var emailVerified = member.GetValue<bool>("emailVerified");
            if (!emailVerified)
            {
                ModelState.AddModelError("Login", "Please verify your email.");
                return CurrentUmbracoPage();
            }

            if (!Members.Login(vm.Username, vm.Password))
            {
                ModelState.AddModelError("Login", "The username or password is not correct.");
                return CurrentUmbracoPage();
            }

            if (!string.IsNullOrEmpty(vm.RedirectUrl))
            {
                return Redirect(vm.RedirectUrl);
            }

            return RedirectToCurrentUmbracoPage();
        }
        #endregion

        #region Forgotten Password

        public ActionResult RenderForgottenPAssword()
        {
            var vm = new ForgottenPasswordViewModel();

            return PartialView(PARTIAL_VIEW_FOLDER + "ForgottenPassword.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult HandleForgottenPassword(ForgottenPasswordViewModel vm)
        {
            var member = Services.MemberService.GetByEmail(vm.EmailAddress);
            if (member == null)
            {
                ModelState.AddModelError("Error", "Sorry we can't find that email address");

                return CurrentUmbracoPage();
            }

            var resetToken = Guid.NewGuid().ToString();
            var expiryDate = DateTime.Now.AddHours(12);

            member.SetValue("resetExpiryDate", expiryDate);
            member.SetValue("resetLinkToken", resetToken);
            Services.MemberService.Save(member);

            _emailService.SendResetPasswordNotification(vm.EmailAddress, resetToken);

            Logger.Info<LoginController>($"Sent a password reset to {vm.EmailAddress}");

            TempData["status"] = "OK";

            return RedirectToCurrentUmbracoPage();

        }
        #endregion

        #region Reset Password

        public ActionResult RenderResetPassword()
        {
            var vm = new ResetPasswordViewModel();
            return PartialView(PARTIAL_VIEW_FOLDER + "ResetPassword.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandleResetPassword(ResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            var token = Request.QueryString["token"];
            if (string.IsNullOrEmpty(token))
            {
                Logger.Warn<LoginController>("Reset Password - no token found");
                ModelState.AddModelError("Error", "Invalid token");
                return CurrentUmbracoPage();
            }

            var member = Services.MemberService.GetMembersByPropertyValue("resetLinkToken", token).SingleOrDefault();
            if (member == null)
            {
                ModelState.AddModelError("Error", "That link is no longer valid");
                return CurrentUmbracoPage();
            }

            var membersTokenExpiryDate = member.GetValue<DateTime>("resetExpiryDate");
            var currentTime = DateTime.Now;
            if (currentTime.CompareTo(membersTokenExpiryDate) >= 0)
            {
                ModelState.AddModelError("Error", "The link has expired, please request a new one from forgotten password-page");
                return CurrentUmbracoPage();
            }

            Services.MemberService.SavePassword(member, vm.Password);

            member.SetValue("resetLinkToken", string.Empty);
            member.SetValue("resetExpiryDate", null);
            member.IsLockedOut = false;
            Services.MemberService.Save(member);


            _emailService.SendPasswordChangedNotification(member.Email);

            TempData["status"] = "OK";
            Logger.Info<LoginController>($"User {member.Username} has changed their password.");
            return RedirectToCurrentUmbracoPage();
        }

        #endregion
    }
}
