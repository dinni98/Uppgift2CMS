using Uppgift2.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Core.Logging;
using System.Net.Mail;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Uppgift2.Core.Interfaces;

namespace Uppgift2.Core.Controllers
{

    public class ContactController : SurfaceController
    {

        private IEmailService _emailService;

        public ContactController(IEmailService emailService)
        {
            _emailService = emailService;
        }
        public ActionResult RenderContactForm()
        {
            var vm = new ContactFormViewModel();

            var siteSettings = Umbraco.ContentAtRoot().DescendantsOrSelfOfType("siteSettings").FirstOrDefault();
            if (siteSettings != null)
            {
                var siteKey = siteSettings.Value<string>("recaptchaSiteKey");
                vm.RecaptchaSiteKey = siteKey;
            }
            return PartialView("~/Views/Partials/Contact Form.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandleContactForm(ContactFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("Error", "Please check the form.");
                return CurrentUmbracoPage();
            }
            var siteSettings = Umbraco.ContentAtRoot().DescendantsOrSelfOfType("siteSettings").FirstOrDefault();
            if (siteSettings != null)
            {
                var secretKey = siteSettings.Value<string>("recaptchaSecretKey");
                if (!string.IsNullOrEmpty(secretKey) && !string.IsNullOrEmpty(Request.Form["GoogleCaptchaToken"]))
                {
                    var isCaptchaValid = IsCaptchaValid(Request.Form["GoogleCaptchaToken"], secretKey);
                    if (!isCaptchaValid)
                    {
                        ModelState.AddModelError("Captcha", "Go away, robot.");
                        return CurrentUmbracoPage();
                    }
                }
            }

            try
            {
                var contactForms = Umbraco.ContentAtRoot().DescendantsOrSelfOfType("contactForms").FirstOrDefault();

                if (contactForms != null)
                {
                    var newContact = Services.ContentService.Create("Contact", contactForms.Id, "contactForm");
                    newContact.SetValue("contactName", vm.Name);
                    newContact.SetValue("contactEmail", vm.EmailAddress);
                    newContact.SetValue("contactSubject", vm.Subject);
                    newContact.SetValue("contactComments", vm.Comment);
                    Services.ContentService.SaveAndPublish(newContact);
                }
                _emailService.SendContactNotificationToAdmin(vm);

                TempData["status"] = "OK";

                return RedirectToCurrentUmbracoPage();
            }
            catch (Exception exc)
            {
                Logger.Error<ContactController>("There was an error in the contact form submission", exc.Message);
                ModelState.AddModelError("Error", "Sorry there was a problem noting your details. Would you please try again later?");
            }

            return CurrentUmbracoPage();
        }

        private bool IsCaptchaValid(string token, string secretKey)
        {

            HttpClient httpClient = new HttpClient();
            var res = httpClient
                        .GetAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={token}")
                        .Result;
            if (res.StatusCode != System.Net.HttpStatusCode.OK)
                return false;

            string jsonRes = res.Content.ReadAsStringAsync().Result;
            dynamic jsonData = JObject.Parse(jsonRes);
            if (jsonData.success != "true")
            {
                return false;
            }

            return true;
        }


        private void SendContactFormReceivedEmail(ContactFormViewModel vm)
        {

            var siteSettings = Umbraco.ContentAtRoot().DescendantsOrSelfOfType("siteSettings").FirstOrDefault();
            if (siteSettings == null)
            {
                throw new Exception("There are no site settings");
            }

            var fromAddress = siteSettings.Value<string>("emailSettingsFromAddress");
            var toAddresses = siteSettings.Value<string>("emailSettingsAdminAccounts");

            if (string.IsNullOrEmpty(fromAddress))
            {
                throw new Exception("There needs to be a from address in site settings");
            }
            if (string.IsNullOrEmpty(toAddresses))
            {
                throw new Exception("There needs to be a to address in site settings");
            }



            var emailSubject = "There has been a contact form submitted";
            var emailBody = $"A new contact form has been received from {vm.Name}. Their comments were: {vm.Comment}";
            var smtpMessage = new MailMessage();
            smtpMessage.Subject = emailSubject;
            smtpMessage.Body = emailBody;
            smtpMessage.From = new MailAddress(fromAddress);

            var toList = toAddresses.Split(',');
            foreach (var item in toList)
            {
                if (!string.IsNullOrEmpty(item))
                    smtpMessage.To.Add(item);
            }



            using (var smtp = new SmtpClient())
            {
                smtp.Send(smtpMessage);
            }
        }
    }
}
