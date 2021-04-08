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
    public class RegisterViewModel
    {
        [DisplayName("First Name")]
        [Required(ErrorMessage = "Please make sure you entered a first name.")]
        public string FirstName { get; set; }
        [DisplayName("Last Name")]
        [Required(ErrorMessage = "Please make sure you entered a last name.")]
        public string LastName { get; set; }
        [DisplayName("Username")]
        [Required(ErrorMessage = "Please pick a username.")]
        [MinLength(6)]
        public string Username { get; set; }
        [DisplayName("Email")]
        [Required(ErrorMessage = "Please make sure you entered a valid email name.")]
        public string EmailAddress { get; set; }
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
