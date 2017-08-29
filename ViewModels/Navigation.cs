using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mars.PerfectFit.Presentation.Web.ViewModels
{
    using Mars.PerfectFit.Core.Domain.Models;
    using Mars.PerfectFit.Core.Domain.Models.Cms;

    public sealed class Navigation
    {
        public IEnumerable<MenuItem> MenuItems { get; set; }

        public Image SecondaryLogo { get; set; }

        public string ProfileUrl { get; set; }

        public Image SecondaryLogoMobile { get; set; }

        public string RegistrationUrl { get; set; }
    }
}
