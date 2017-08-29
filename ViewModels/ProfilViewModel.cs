using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mars.PerfectFit.Presentation.Web.ViewModels
{
    using Mars.PerfectFit.Core.Domain.Models;

    public class ProfilViewModel : ViewModelBase
    {
        public RegisterModel Registration { get; set; }

        public Image Banner { get; set; }

        public string day { get; set; }

        public string month { get; set; }

        public string year { get; set; }
        public List<PetsModel> Pets { get; set; }

        public bool ReceiveMarketingEmail { get; set; }
    }
}