using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mars.PerfectFit.Core.Domain.Services;
using Umbraco.Web;
using System.Threading;
using Mars.PerfectFit.Presentation.Web.ViewModels;
using System.Dynamic;

namespace Mars.PerfectFit.Presentation.Web.Controllers
{
    public class MoneyBackGuaranteeConfirmationController : BaseController
    {
        public MoneyBackGuaranteeConfirmationController(IConfigService configService, ICommonService commonService) : base(configService, commonService)
        {
        }

        public ActionResult MoneyBackGuaranteeConfirmation()
        {
            var mbgConfirmationPage =
                Umbraco.TypedContentAtXPath("//moneyBackGuaranteeConfirmation")
                    .FirstOrDefault(x => x.GetCulture().Equals(Thread.CurrentThread.CurrentUICulture));


            if(TempData["errorNumero"] != null)
            {
                ViewBag.ErrMessageNum = TempData["errorNumero"].ToString();
                ViewBag.ErrMessage = TempData["errorMessate"].ToString();
            }

           

            string tag = string.Empty;
            if (Session["MBGEventLabel"] != null)
            {
                tag = Session["MBGEventLabel"].ToString();
                dynamic datalayer = new ExpandoObject();
                datalayer.@event = "event_mbg_actions";
                datalayer.event_category = "event_crm_actions";
                datalayer.event_action = "event_mbg_complete";
                datalayer.event_label = tag;
                string lowerDatalayer = this.GenerateCustomVariable(datalayer);
                ViewBag.DataLayer = lowerDatalayer;
            }
            Session["MBGEventLabel"] = null;


            dynamic datalayerMbg = new ExpandoObject();
            datalayerMbg.MetaDataSiteMap = "Money Back Guarantee";
            if (Session["user_profile_dl"] != null)
            {
                datalayerMbg.MetaDataUserProfile = Session["user_profile_dl"];
            }
            ViewBag.MetaDataSiteMap = this.GenerateCustomVariable(datalayerMbg);

            return View(new MoneyBackGuaranteeConfirmationViewModel
            {
                ImageBanner = mbgConfirmationPage.GetPropertyValue<string>("banner"),
                TitleBold = mbgConfirmationPage.GetPropertyValue<string>("title"),
                SimpleTitle = mbgConfirmationPage.GetPropertyValue<string>("subtitle"),
                SuccessMessage = mbgConfirmationPage.GetPropertyValue<string>("successMessage"),
                SuccessMessageDescription = mbgConfirmationPage.GetPropertyValue<string>("successMessageDescription")
            });
        }
    }
}