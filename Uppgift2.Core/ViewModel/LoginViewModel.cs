﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uppgift2.Core.ViewModel
{
    public class LoginViewModel
    {
        [Display(Name = "Username")]
        [Required]
        public string Username { get; set; }
        [Display(Name = "Password")]
        [Required]
        public string Password { get; set; }
        public string RedirectUrl { get; set; }
    }
}
