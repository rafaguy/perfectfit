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
    public class CouponConfirmationPageController : BaseController
    {
        private readonly ICouponConfirmationPageRepository couponConfirmationRepository;

        public CouponConfirmationPageController(ICouponConfirmationPageRepository couponConfirmationRepository, IConfigService configService, ICommonService commonService) : base(configService, commonService)
        {
            this.couponConfirmationRepository = couponConfirmationRepository;
        }

        public ActionResult CouponConfirmationPage()
        {
            var model = new CouponConfirmationViewModel
            {
                couponConfirmation = couponConfirmationRepository.GetCouponConfirmation(CurrentPage.Id)
            };

            dynamic datalayerSiteMap = new ExpandoObject();

            if (Session["user_profile_dl"] != null)
            {
                datalayerSiteMap.MetaDataUserProfile = Session["user_profile_dl"];
            }

            datalayerSiteMap.MetaDataSiteMap = "Coupons";

            ViewBag.MetaDataSiteMap = this.GenerateCustomVariable(datalayerSiteMap);

            return View(model);
        }
    }
}