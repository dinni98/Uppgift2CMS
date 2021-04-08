using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Web.Mvc;
using Uppgift2.Core.ViewModel;

namespace Uppgift2.Core.Controllers
{
    public class AccountController : SurfaceController
    {
        public const string PARTIAL_VIEW_FOLDER = "~/Views/Partials/MyAccount/";

        public ActionResult RenderMyAccount()
        {
            var vm = new AccountViewModel();
            if (!Umbraco.MemberIsLoggedOn())
            {
                ModelState.AddModelError("Error", "You are not currently logged in.");
                return CurrentUmbracoPage();
            }

            var member = Services.MemberService.GetByUsername(Membership.GetUser().UserName);
            if (member == null)
            {
                ModelState.AddModelError("Error", "We could not find you in the system.");
                return CurrentUmbracoPage();
            }
            vm.Name = member.Name;
            vm.Email = member.Email;
            vm.Username = member.Username;

            return PartialView(PARTIAL_VIEW_FOLDER + "MyAccount.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandleUpdateDetails(AccountViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("Error", "There has been a problem.");
                return CurrentUmbracoPage();
            }
            if (!Umbraco.MemberIsLoggedOn() || Membership.GetUser() == null)
            {
                ModelState.AddModelError("Error", "You are not logged on.");
                return CurrentUmbracoPage();
            }

            var member = Services.MemberService.GetByUsername(Membership.GetUser().UserName);
            if (member == null)
            {
                ModelState.AddModelError("Error", "You are not logged on.");
                return CurrentUmbracoPage();
            }
            member.Name = vm.Name;
            member.Email = vm.Email;

            Services.MemberService.Save(member);
            TempData["status"] = "Updated Details";
            return RedirectToCurrentUmbracoPage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandlePasswordChange(AccountViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("Error", "There is a problem with the form");
                return CurrentUmbracoPage();
            }
            if (!Umbraco.MemberIsLoggedOn() || Membership.GetUser() == null)
            {
                ModelState.AddModelError("Error", "You are not logged in");
                return CurrentUmbracoPage();
            }
            var member = Services.MemberService.GetByUsername(Membership.GetUser().UserName);
            if (member == null)
            {
                ModelState.AddModelError("Error", "You are not logged in");
                return CurrentUmbracoPage();
            }
            try
            {
                Services.MemberService.SavePassword(member, vm.Password);
            }
            catch (MembershipPasswordException exc)
            {
                ModelState.AddModelError("Error", "There's a problem with your password " + exc.Message);
                return CurrentUmbracoPage();
            }

            TempData["status"] = "Updated Password";
            return RedirectToCurrentUmbracoPage();
        }
    }
}