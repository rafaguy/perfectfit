using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mars.PerfectFit.Infrastructure.Data.Umbraco.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace Mars.PerfectFit.Presentation.Web.ViewModels
{
    public class RegisterModel
    {
        [RequiredValidation("Forms - Validation - Required - FirstName")]
        public string FirstName { get; set; }

        public string LastName { get; set; }

        [RequiredValidation("Forms - Validation - Required - Email")]
        [EmailValidation(ErrorMessageDictionaryKey = "Forms - Validation - Email")]
        public string Email { get; set; }

        [RequiredValidation("Forms - Validation - Required - Passwd")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [RequiredValidation("Forms - Validation - Required - Address")]
        public string Address1 { get; set; }

        public string Address2 { get; set; }
        [RequiredValidation("Forms - Validation - Required - City")]
        public string City { get; set; }

        [RequiredValidation("Forms - Validation - Required - Postcode")]
        public string PostalCode { get; set; }

        public string BirthDate { get; set; }

        public string FriendlyUrl { get; set; }

        public string MyProfileQuestion { get; set; }

        public string UpdateMarsQuestion { get; set; }
    }
}