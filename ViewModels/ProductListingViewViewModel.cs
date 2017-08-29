namespace Mars.PerfectFit.Presentation.Web.ViewModels
{
    using System.Collections.Generic;
    using Mars.PerfectFit.Core.Domain.Models.Products;

    public sealed class ProductListingViewViewModel : ViewModelBase
    {
        #region Properties

        public string BannerText { get; set; }

        public string BannerTitle { get; set; }

        public string ImageBanner { get; set; }

        public string PageDescription { get; set; }

        public string PageHeading { get; set; }

        public string PageSubHeading { get; set; }

        public ProductFormula ProductFormula { get; set; }

        public IEnumerable<Product> Products { get; set; }

        #endregion
    }
}
