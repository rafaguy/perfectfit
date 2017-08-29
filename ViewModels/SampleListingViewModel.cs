namespace Mars.PerfectFit.Presentation.Web.ViewModels
{
    using System.Collections.Generic;
    using Mars.PerfectFit.Core.Domain.Models;
    using Mars.PerfectFit.Core.Domain.Models.Sampling;

    public sealed class SampleListingViewModel : ViewModelBase
    {
        public SampleListingViewModel()
        {
            Samples = new List<Sample>();
        }

        public Image Banner { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public IEnumerable<Sample> Samples { get; set; }

        public string SubTitle { get; set; }
    }
}
