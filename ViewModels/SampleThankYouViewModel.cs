namespace Mars.PerfectFit.Presentation.Web.ViewModels
{
    using Mars.PerfectFit.Core.Domain.Models;

    public sealed class SampleThankYouViewModel : ViewModelBase
    {
        public Image Banner { get; set; }

        public string ErrorMessage { get; set; }

        public string OffersUrl { get; set; }

        public string SubTitle { get; set; }

        public string SuccessMessage { get; set; }

        public string Title { get; set; }
    }
}
