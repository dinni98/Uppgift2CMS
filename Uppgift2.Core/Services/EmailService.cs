using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web;
using Uppgift2.Core.Interfaces;
using Uppgift2.Core.ViewModel;

namespace Uppgift2.Core.Services
{
    public class EmailService : IEmailService
    {
        private UmbracoHelper _umbraco;

        public EmailService(UmbracoHelper umbraco)
        {
            _umbraco = umbraco;
        }


        public void SendContactNotificationToAdmin(ContactFormViewModel vm)
        {
            var siteSettings = _umbraco.ContentAtRoot().DescendantsOrSelfOfType("siteSettings").FirstOrDefault();
            if (siteSettings == null)
            {
                throw new Exception("There are no site settings");
            }

            var fromAddress = siteSettings.Value<string>("emailSettingsFromAddress");
            var toAddresses = siteSettings.Value<string>("emailSettingsAdminAccounts");

            if (string.IsNullOrEmpty(fromAddress))
            {
                throw new Exception("There needs to be a from address in site settings.");
            }

            if (string.IsNullOrEmpty(toAddresses))
            {
                throw new Exception("There needs to be a to address in site settings");
            }

            var emailSubject = "There has been a contact form submitted";
            var emailBody = $"A new contact form has been received from {vm.Name}. Their comments were: {vm.Comment}";
            var smtpMessage = new MailMessage();
            smtpMessage.Subject = emailSubject;
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
