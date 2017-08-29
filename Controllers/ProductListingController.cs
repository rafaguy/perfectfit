namespace Mars.PerfectFit.Presentation.Web.Controllers
{
    using System.Linq;
    using System.Web.Mvc;
    using Mars.PerfectFit.Core.Domain.Models;
    using Mars.PerfectFit.Core.Domain.Models.Products;
    using Mars.PerfectFit.Core.Domain.Repositories;
    using Mars.PerfectFit.Core.Domain.Services;
    using Mars.PerfectFit.Presentation.Web.ViewModels;
    using Umbraco.Web;

    public class ProductListingController : BaseController
    {
        #region Fields

        private readonly IProductListingRepository productListingRepository;
        private readonly IProductService productService;

        #endregion

        #region Constructors & Destructors

        public ProductListingController(IConfigService configService, ICommonService commonService,
            IProductListingRepository productListingRepository, IProductService productService) : base(configService, commonService)
        {
            this.productListingRepository = productListingRepository;
            this.productService = productService;
        }

        #endregion

        #region Methods

        public ActionResult Index()
        {
            var productListing = productListingRepository.GetProductListing(CurrentPage.Id);

            var model = new ProductListingViewViewModel
            {
                BannerTitle = productListing.BannerTitle,
                BannerText = productListing.BannerText,
                ImageBanner = productListing.Banner,
                PageDescription = productListing.PageDescription,
                PageHeading = productListing.PageHeading,
                PageSubHeading = productListing.PageSubHeading,
                ProductFormula = productListing.ProductFormula
            };

            var products = productService.GetProducts(CurrentPage.GetCulture()).ToList();

            var speciesName = string.Empty;

            if (products.Any())
            {
                model.Products = products;

                speciesName = model.Products.ElementAt(0).Species.Title;
            }

            var tracking = new CustomVariable
            {
                MetaDataSiteMap = "Product Landing Page",
                MetaDataPetsCategorization = speciesName
            };

            ViewBag.Tracking = GenerateTracking(tracking);

            return View(model);
        }

        #endregion
    }
}
