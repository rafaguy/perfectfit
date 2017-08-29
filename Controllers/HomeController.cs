namespace Mars.PerfectFit.Presentation.Web.Controllers
{
    using System.Web.Mvc;
    using Mars.PerfectFit.Core.Domain.Services;
    using Mars.PerfectFit.Presentation.Web.ViewModels;
    using Umbraco.Web;
    using System.Linq;
    using System.Dynamic;

    public class HomeController : BaseController
    {
        #region Constructors & Destructors

        public HomeController(IConfigService configService, ICommonService commonService) : base(configService, commonService)
        {
        }

        #endregion

        #region Methods

        public ActionResult Index()
        {
            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);

            var contentPage = umbracoHelper.TypedContentAtXPath("//home").FirstOrDefault(x => x.GetCulture().ToString() == CurrentPage.GetCulture().ToString());
            HomepageViewViewModel home = new HomepageViewViewModel
            {
                Body = contentPage.GetPropertyValue<string>("body")
            };

            ViewBag.DataLayer = GenerateHomeTracking();

            if (Session["Login_complete_event"] != null)
            {
                dynamic datalayerLoginComplete = new ExpandoObject();
                datalayerLoginComplete.@event = "event_crm_actions";
                datalayerLoginComplete.event_category = "event_crm_actions";
                datalayerLoginComplete.event_action = "event_login_complete";
                datalayerLoginComplete.event_label = "login_page";
                string lowerDatalayer = this.GenerateCustomVariable(datalayerLoginComplete);
                ViewBag.DataLayerLoginComplete = lowerDatalayer;
            }
            Session["Login_complete_event"] = null;

            return View(home);
        }


        private string GenerateHomeTracking()
        {
            dynamic datalayer = new ExpandoObject();
            
            datalayer.MetaDataSiteMap = "Homepage";
            if (Session["user_profile_dl"] != null)
            {
                datalayer.MetaDataUserProfile = Session["user_profile_dl"];
            }
            var layer = this.GenerateCustomVariable(datalayer);
            return layer;
        }


        #endregion
    }
}
