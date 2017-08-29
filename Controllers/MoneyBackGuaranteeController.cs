using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mars.PerfectFit.Core.Domain.Services;
using Umbraco.Web;
using System.Threading;
using Mars.PerfectFit.Presentation.Web.ViewModels;
using Mars.PerfectFit.Core.Domain.Repositories;
using System.Globalization;
using System.Dynamic;

namespace Mars.PerfectFit.Presentation.Web.Controllers
{
    public class MoneyBackGuaranteeController : BaseController
    {
        private readonly IRegistrationProfileRepository registrationProfileRepository;

        public MoneyBackGuaranteeController(IRegistrationProfileRepository registrationProfileRepository, IConfigService configService, ICommonService commonService) : base(configService, commonService)
        {
            this.registrationProfileRepository = registrationProfileRepository;
        }

        public ActionResult MoneyBackGuarantee(string returnModif = "")
        {
            var culture = Thread.CurrentThread.CurrentUICulture;
            if (Session["culture"] != null)
            {
                culture = (CultureInfo)Session["culture"];
            }
            if (TempData["culture"] != null)
            {
                culture = (CultureInfo)TempData["culture"];
            }

            dynamic datalayer = new ExpandoObject();
            datalayer.MetaDataSiteMap = "Money Back Guarantee";
            if (Session["user_profile_dl"] != null)
            {
                datalayer.MetaDataUserProfile = Session["user_profile_dl"];
            }

            Session["mbg_event_label"] = "mbg";

            ViewBag.DataLayer = this.GenerateCustomVariable(datalayer);

            var mbgPage =
                Umbraco.TypedContentAtXPath("//moneyBackGuarantee")
                    .FirstOrDefault(x => x.GetCulture().Equals(Thread.CurrentThread.CurrentUICulture));

            var mbgInformationPage =
                Umbraco.TypedContentAtXPath("//moneyBackGuaranteeInformation")
                    .FirstOrDefault(x => x.GetCulture().Equals(Thread.CurrentThread.CurrentUICulture));

            var urlInformationPage = mbgInformationPage.GetPropertyValue<string>("umbracoUrlAlias");

            var registrationUrl = registrationProfileRepository.GetRegistrationProfileUrlAlias(culture.ToString());
            ViewBag.Register = registrationUrl + "?returnUrl=/" + urlInformationPage + returnModif;

            return View(new MoneyBackGuaranteeViewModel
            {
                ImageBanner = mbgPage.GetPropertyValue<string>("banner"),
                TitleBold = mbgPage.GetPropertyValue<string>("title"),
                SimpleTitle = mbgPage.GetPropertyValue<string>("subtitle"),
                Description = mbgPage.GetPropertyValue<string>("description"),
                LegalText = mbgPage.GetPropertyValue<string>("legalText"),
                LegalDescription = mbgPage.GetPropertyValue<string>("legalDescription")
            });
        }
    }
}