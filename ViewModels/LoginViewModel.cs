using Mars.PerfectFit.Infrastructure.Data.Umbraco.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mars.PerfectFit.Presentation.Web.ViewModels
{
    using Mars.PerfectFit.Core.Domain.Models;

    public class LoginViewModel : ViewModelBase
    {
        public bool Active { get; set; }

        public int ConsumerID { get; set; }

        [RequiredValidation("Login - Username - Error - Mandatory")]
        public string Password { get; set; }

        public int SiteID { get; set; }

        public bool SingleLogin { get; set; }

        [RequiredValidation("Login - Password - Error - Mandatory")]
        [EmailValidation(ErrorMessageDictionaryKey = "Login - Username - Error - Format")]
        public string Username { get; set; }

        public Image Banner { get; set; }

        public bool RememberMe { get; set; }

        public string UrlPageReturn { get; set; }
    }
}