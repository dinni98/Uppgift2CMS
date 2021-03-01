using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Umbraco.Web.Mvc;
using Uppgift2.Core.ViewModel;

namespace Uppgift2.Core.Controllers
{
    public class ContactController : SurfaceController
    {
        public ActionResult RenderContactForm()
        {
            var vm = new ContactFormViewModel();
            return PartialView("~/Views/Partials/Contact Form.cshtml", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandleContactForm(ContactFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("Error", "Please check the form");
                return PartialView("~/Views/Partials/Contact Form.cshtml", vm);
            }

            return null;
        }
    }
}
