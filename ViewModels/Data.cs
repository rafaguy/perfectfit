using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mars.PerfectFit.Presentation.Web.ViewModels
{
    public class Data
    {
        public bool hearPerfectFit { get; set; }

        public bool receiveMarketingEmail { get; set; }
        public IList<PetsModel> petsModel { get; set; }
    }
}