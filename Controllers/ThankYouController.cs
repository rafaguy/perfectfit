namespace Mars.PerfectFit.Presentation.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using Mars.PerfectFit.Core.Domain.Services;
    using Mars.PerfectFit.Presentation.Web.Helpers;
    using Mars.PerfectFit.Presentation.Web.ViewModels;
    using Umbraco.Web;
    using System.Dynamic;

    public class ThankYouController : BaseController
    {
        public ThankYouController(IConfigService configService, ICommonService commonService) : base(configService, commonService)
        {
        }
        //TODO: Sample Tracking
        public ActionResult SampleThankYou()
        {
            var offersUrl = Umbraco
                .TypedContentAtXPath("//sampleListing")
                .FirstOrDefault(x => x.GetCulture().Equals(CurrentPage.GetCulture()));

            if (TempData["step"] == null)
            {
                return Redirect($"/{offersUrl?.UrlName}");
            }

            var model = new SampleThankYouViewModel
            {
                Banner = ImageHelper.GetImage(Umbraco.TypedMedia(CurrentPage.GetPropertyValue<int>("banner"))),
                Title = CurrentPage.GetPropertyValue<string>("title") ?? string.Empty,
                SubTitle = CurrentPage.GetPropertyValue<string>("subTitle") ?? string.Empty,
                SuccessMessage = CurrentPage.GetPropertyValue<string>("successMessage") ?? string.Empty,
                ErrorMessage = CurrentPage.GetPropertyValue<string>("errorMessage") ?? string.Empty
            };

            if (TempData["Error"] != null)
            {
                ViewBag.Message = model.ErrorMessage;
                TempData["Error"] = 1;
            }
            else
            {
                ViewBag.Message = model.SuccessMessage;
            }

            model.OffersUrl = offersUrl != null ? offersUrl.UrlName : string.Empty;

            dynamic datalayerSiteMap = new ExpandoObject();


            string tag = string.Empty;
            if (Session["sample_complete_label"] != null)
            {
                tag = Session["sample_complete_label"].ToString();
                dynamic datalayer = new ExpandoObject();
                datalayer.@event = "event_crm_actions";
                datalayer.event_category = "event_sample_actions";
                datalayer.event_action = Session["sample_complete_label"];
                datalayer.event_label = "sample_complete";
                string lowerDatalayer = this.GenerateCustomVariable(datalayer);
                ViewBag.DataLayer = lowerDatalayer;
            }

            Session["sample_complete_label"] = null;

            datalayerSiteMap.MetaDataSiteMap = "Sample";
            if (Session["user_profile_dl"] != null)
            {
                datalayerSiteMap.MetaDataUserProfile = Session["user_profile_dl"];
            }

            ViewBag.MetaDataSiteMap = this.GenerateCustomVariable(datalayerSiteMap);

            return View(model);
        }
    }
}
