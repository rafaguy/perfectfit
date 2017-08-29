using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mars.PerfectFit.Presentation.Web.ViewModels
{
    public class ForgotPasswordViewModel : ViewModelBase
    {
        [Required]
        public string Email { get; set; }
    }
}