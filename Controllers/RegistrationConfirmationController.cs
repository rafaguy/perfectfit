using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mars.PerfectFit.Core.Domain.Services;
using Mars.PerfectFit.Core.Domain.Repositories;
using Mars.PerfectFit.Presentation.Web.ViewModels;
using System.Dynamic;

namespace Mars.PerfectFit.Presentation.Web.Controllers
{
    public class RegistrationConfirmationController : BaseController
    {
        private readonly IRegistrationConfirmationRepository registrationConfirmationRepository;

        public RegistrationConfirmationController(IRegistrationConfirmationRepository registrationConfirmationRepository, IConfigService configService, ICommonService commonService) : base(configService, commonService)
        {
            this.registrationConfirmationRepository = registrationConfirmationRepository;
        }

        public ActionResult RegistrationConfirmation()
        {
            var model = new RegistrationConfirmationViewModel
            {
                registrationConfirmation = registrationConfirmationRepository.GetRegistrationConfirmation(CurrentPage.Id)
            };

            var tag = string.Empty;

            if (Session["registrationpendingLabel_for_confirmation"] != null)
            {
                if (Session["coupon_event_label"] != null)
                {
                    tag = Session["coupon_event_label"].ToString();
                    dynamic datalayer = new ExpandoObject();
                    datalayer.@event = "event_crm_actions";
                    datalayer.event_category = "event_crm_actions";
                    datalayer.event_action = "event_registration_pending";
                    datalayer.event_label = tag;
                    string lowerDatalayer = this.GenerateCustomVariable(datalayer);
                    ViewBag.DataLayer = lowerDatalayer;
                } else if (Session["registrationpendingLabel"] != null)
                {
                    
                        tag = Session["registrationpendingLabel"].ToString();
                        dynamic datalayer = new ExpandoObject();
                        datalayer.@event = "event_crm_actions";
                        datalayer.event_category = "event_crm_actions";
                        datalayer.event_action = "event_registration_pending";
                        datalayer.event_label = tag;
                        string lowerDatalayer = this.GenerateCustomVariable(datalayer);
                        ViewBag.DataLayer = lowerDatalayer;
                    
                }else if(Session["sample_event_label"] != null)
                {
                    tag = Session["sample_event_label"].ToString();
                    dynamic datalayer = new ExpandoObject();
                    datalayer.@event = "event_crm_actions";
                    datalayer.event_category = "event_crm_actions";
                    datalayer.event_action = "event_registration_pending";
                    datalayer.event_label = tag;
                    string lowerDatalayer = this.GenerateCustomVariable(datalayer);
                    ViewBag.DataLayer = lowerDatalayer;
                }else if (Session["mbg_event_label"]!=null)
                {
                    tag = Session["mbg_event_label"].ToString();
                    dynamic datalayer = new ExpandoObject();
                    datalayer.@event = "event_crm_actions";
                    datalayer.event_category = "event_crm_actions";
                    datalayer.event_action = "event_registration_pending";
                    datalayer.event_label = tag;
                    string lowerDatalayer = this.GenerateCustomVariable(datalayer);
                    ViewBag.DataLayer = lowerDatalayer;
                }
               
            }


            dynamic datalayerSiteMap = new ExpandoObject();

            datalayerSiteMap.MetaDataSiteMap = "Registration";

            ViewBag.MetaDataSiteMap = this.GenerateCustomVariable(datalayerSiteMap);


            Session["registrationpendingLabel"] = null;
            Session["coupon_event_label"] = null;
            Session["sample_event_label"] = null;
            Session["mbg_event_label"] = null;



            return View(model);
        }
       

      
    }


   
}