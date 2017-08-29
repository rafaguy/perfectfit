using Mars.PerfectFit.Infrastructure.Data.Umbraco.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mars.PerfectFit.Presentation.Web.ViewModels
{
    using Mars.PerfectFit.Core.Domain.Models;

    public class ResetPasswordViewModel : ViewModelBase
    {
        [RequiredValidation("Reset - Password - Error - Mandatory")]
        public string Password { get; set; }

        [RequiredValidation("Reset - Password - Error - Mandatory")]
        public string ConfirmPassword { get; set; }

        public Image Banner { get; set; }
    }
}