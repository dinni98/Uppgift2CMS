using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAnnotationsExtensions;

namespace Uppgift2.Core.ViewModel
{
    public class AccountViewModel
    {
        [DisplayName("Full Name")]
        [Required(ErrorMessage = "Please enter your name")]
        public string Name { get; set; }
        [DisplayName("Email")]
        [Required(ErrorMessage = "Please enter your email address")]
        [Email(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; }
        public string Username { get; set; }

        [UIHint("Password")]
        [DisplayName("Password")]
        [Required(ErrorMessage = "Please make sure you entered a password.")]
        [MinLength(10, ErrorMessage = "Please make sure your password is at least 10 characters.")]
        public string Password { get; set; }
        [UIHint("Confirm Password")]
        [DisplayName("Confirm Password")]
        [Required(ErrorMessage = "Please enter your password again..")]
        [EqualTo("Password", ErrorMessage = "Make sure you entered the correct password.")]
        public string ConfirmPassword { get; set; }

    }
}
