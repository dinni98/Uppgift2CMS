using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Umbraco.Web.Mvc;
using Uppgift2.Core.Interfaces;
using Uppgift2.Core.ViewModel;

namespace Uppgift2.Core.Controllers
{
    public class RegisterController : SurfaceController
    {
        private const string PARTIAL_VIEW_FOLDER = "~/Views/Partials/";
        private IEmailService _emailService;

        public RegisterController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        #region RegisterForm

        public ActionResult RenderRegister()
        {
            var vm = new RegisterViewModel();
            return PartialView(PARTIAL_VIEW_FOLDER + "Register.cshtml", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandleRegister(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            var existingMember = Services.MemberService.GetByEmail(vm.EmailAddress);

            if(existingMember != null)
            {
                ModelState.AddModelError("Account Error", "This email is already registered.");
                return CurrentUmbracoPage();
            }

            existingMember = Services.MemberService.GetByUsername(vm.Username);

            if (existingMember != null)
            {
                ModelState.AddModelError("Account Error", "This username is in use, pick another one.");
                return CurrentUmbracoPage();
            }

            var newMember = Services.MemberService
                                .CreateMember(vm.Username, vm.EmailAddress, $"{vm.FirstName} {vm.LastName}", "Member");
            newMember.PasswordQuestion = "";
            newMember.RawPasswordAnswerValue = "";
            Services.MemberService.Save((newMember));
            Services.MemberService.SavePassword(newMember, vm.Password);
            Services.MemberService.AssignRole(newMember.Id, "Normal User");

            var token = Guid.NewGuid().ToString();
            newMember.SetValue("emailVerifyToken", token);
            Services.MemberService.Save(newMember);

            _emailService.SendVerifyEmailAddressNotification(newMember.Email, token);



            TempData["status"] = "OK";

            return RedirectToCurrentUmbracoPage();
        }
        #endregion

        #region Verification

        public ActionResult RenderEmailVerification(string token)
        {
            var member = Services.MemberService.GetMembersByPropertyValue("emailVerifyToken", token).SingleOrDefault();
            if (member != null)
            {
                var alreadyVerified = member.GetValue<bool>("emailVerified");
                if (alreadyVerified)
                {
                    ModelState.AddModelError("Verified", "You have already verified your email");
                    return CurrentUmbracoPage();
                }
                member.SetValue("emailVerified", true);
                member.SetValue("emailVerifiedDate", DateTime.Now);
                Services.MemberService.Save(member);

                TempData["status"] = "OK";
                return PartialView(PARTIAL_VIEW_FOLDER + "EmailVerification.cshtml");
            }

            ModelState.AddModelError("Error", "There was a problem verifying your account.");
            return CurrentUmbracoPage();
        }

        #endregion
    }
}
