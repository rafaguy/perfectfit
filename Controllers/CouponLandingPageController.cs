namespace Mars.PerfectFit.Presentation.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using Mars.PerfectFit.Core.Domain;
    using Mars.PerfectFit.Core.Domain.Models.Coupons;
    using Mars.PerfectFit.Core.Domain.Repositories;
    using Mars.PerfectFit.Core.Domain.Services;
    using Mars.PerfectFit.Presentation.Web.Security;
    using Mars.PerfectFit.Presentation.Web.ViewModels;
    using System.Dynamic;

    public class CouponLandingPageController : BaseController
    {
        #region Fields

        private readonly ICouponLandingPageRepository couponLandingRepository;
        private readonly ICouponConfirmationPageRepository couponConfirmationRepository;
        private readonly ICouponRepository couponRepository;
        private readonly ICouponRequestService couponRequestService;

        #endregion

        #region Constructors & Destructors

        public CouponLandingPageController(IConfigService configService, ICommonService commonService,
            ICouponLandingPageRepository couponLandingRepository,
            ICouponRepository couponRepository,
            ICouponRequestService couponRequestService,
            ICouponConfirmationPageRepository couponConfirmationRepository)
            : base(configService, commonService)
        {
            this.couponLandingRepository = couponLandingRepository;
            this.couponRepository = couponRepository;
            this.couponRequestService = couponRequestService;
            this.couponConfirmationRepository = couponConfirmationRepository;
        }

        #endregion

        #region Methods

        public ActionResult CouponLandingPage(string print)
        {
            Session["culture"] = System.Threading.Thread.CurrentThread.CurrentUICulture;
            var coupons = couponRepository.GetCoupons(System.Threading.Thread.CurrentThread.CurrentUICulture.ToString()).ToList();

            var validCoupon = new List<Coupon>();

            for (var i = 0; i < coupons.Capacity; i++)
            {
                var couponEndDate = DateTime.Parse(coupons.ElementAt(i).EndDate);

                var compareDate = DateTime.Compare(couponEndDate.Date, DateTime.Now.Date);

                //Set Default button message
                coupons.ElementAt(i).UserCouponMessage = Umbraco.GetDictionaryValue("Coupon - Landing Page - Button Default");

                var consumerId = Authentication.GetConsumerId();

                if (!string.IsNullOrEmpty(consumerId))
                {
                    var couponId = coupons.ElementAt(i).ProviderCouponId;

                    var couponQuota = coupons.ElementAt(i).UserQuota;

                    var userQuota = couponRequestService.GetCouponRequestCount(couponId, consumerId);

                    if (userQuota >= couponQuota)
                    {
                        coupons.ElementAt(i).UserCouponMessage = Umbraco.GetDictionaryValue("Coupon - Landing Page - Button Printed");
                        coupons.ElementAt(i).CssAlredyPrinted = "btn--disabled btn--orange-fade";
                    }
                }else
                {
                    var couponLandingUrl = couponLandingRepository.GetCouponLandingPageUrlName(System.Threading.Thread.CurrentThread.CurrentUICulture.ToString());
                    Session["returnUrl"] = "/" + couponLandingUrl;
                }

                if (compareDate >= 0) validCoupon.Add(coupons.ElementAt(i));
            }
            var couponModel = new CouponLandingViewModel
            {
                CouponLanding = couponLandingRepository.GetCouponLanding(CurrentPage.Id),
                Coupons = validCoupon
            };

            if (!string.IsNullOrEmpty(print)) ViewBag.DirectPrint = print;

            if (validCoupon.Capacity == 0) ViewBag.MessageCoupon = Umbraco.GetDictionaryValue("Coupon - Landing Page - No - Coupon -Available");

            ViewBag.ReturnUrl = couponConfirmationRepository.GetCouponConfirmationPageUrlAlias(System.Threading.Thread.CurrentThread.CurrentUICulture.ToString());
            ViewBag.BaseUrl = Helpers.UrlHelper.GetBaseUrl();

            //tracking login completed
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


            dynamic datalayer = new ExpandoObject();
            datalayer.MetaDataSiteMap = "Coupons";
            if (Session["user_profile_dl"] != null)
            {
                datalayer.MetaDataUserProfile = Session["user_profile_dl"];
            }
            var layer = this.GenerateCustomVariable(datalayer);
            ViewBag.MetaDataSiteMap = layer;

            return View(couponModel);
        }

        #endregion
    }
}
