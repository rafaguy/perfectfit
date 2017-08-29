﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mars.PerfectFit.Presentation.Web.ViewModels
{
    public sealed class RegistrationProfileViewModel : ViewModelBase
    {
        public RegisterModel Registration { get; set; }

        public string Banner { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
    }
}