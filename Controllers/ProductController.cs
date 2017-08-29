namespace Mars.PerfectFit.Presentation.Web.Controllers
{
    using System.Web.Mvc;
    using Mars.PerfectFit.Core.Domain.Models;
    using Mars.PerfectFit.Core.Domain.Repositories;
    using Mars.PerfectFit.Core.Domain.Services;
    using Mars.PerfectFit.Presentation.Web.ViewModels;

    public class ProductController : BaseController
    {
        #region Fields

        private readonly IProductService productService;

        #endregion

        #region Constructors & Destructors

        public ProductController(IConfigService configService, ICommonService commonService, IProductService productService) : base(configService, commonService)
        {
            this.productService = productService;
        }

        #endregion

        #region Methods

        public ActionResult Index()
        {
            var product = productService.GetProduct(CurrentPage.Id);

            var viewModel = new ProductViewModel
            {
                Benefits = product.Benefits,
                Description = product.Description,
                Feedings = product.Feedings,
                FlavorIcon = product.Flavor.Icon,
                Nutrition = product.Nutrition,
                Packshots = product.Packshots,
                ProductFormula = product.ProductFormula,
                Title = product.Title
            };

            var tracking = new CustomVariable
            {
                MetaDataSiteMap = "Product Detail Page",
                MetaDataPetsCategorization = product.Species.Title,
                MetaDataProductFlavour = product.Flavor.Title,
                MetaDataProductName = product.Title
            };

            ViewBag.Tracking = GenerateTracking(tracking);

            return View(viewModel);
        }

        #endregion
    }
}
