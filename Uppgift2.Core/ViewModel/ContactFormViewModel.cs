using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Uppgift2.Core.ViewModel
{
    public class ContactFormViewModel
    {
        [Required]
        [MaxLength(80, ErrorMessage = "Please try to limit to 80 characters")]
        public string Name { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Please use a valid email address.")]
        public string EmailAddress { get; set; }
        [Required]
        [MaxLength(255, ErrorMessage = "Please try and limit to 255 characters")]
        public string Comment { get; set; }
        [MaxLength()]
        public string Subject { get; set; }
    }
}
