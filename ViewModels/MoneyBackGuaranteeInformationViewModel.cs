using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mars.PerfectFit.Presentation.Web.ViewModels
{
    public class MoneyBackGuaranteeInformationViewModel : ViewModelBase
    {
        public string ImageBanner { get; set; }

        public string TitleBold { get; set; }

        public string SimpleTitle { get; set; }

        public string ShortDescription { get; set; }

        public string LongDescription { get; set; }

        public string LegalText { get; set; }

        public MbgUpdateViewModel MbgUpdate { get; set; }
    }
}