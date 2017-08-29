namespace Mars.PerfectFit.Presentation.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Mars.PerfectFit.Core.Domain;
    using Mars.PerfectFit.Core.Domain.Models.Coupons;
    using Mars.PerfectFit.Core.Domain.Repositories;
    using Mars.PerfectFit.Core.Domain.Services;
    using Mars.PerfectFit.Infrastructure.CrmService.Services;
    using Mars.PerfectFit.Presentation.Web.Security;
    using Mars.PerfectFit.Presentation.Web.Helpers;
    using Newtonsoft.Json;
    using System.Globalization;
    using System.Linq;
    using Constents;
    using System.Web.Script.Serialization;
    using System.Net.Http.Headers;
    using Umbraco.Web;
    using Core.Domain.Models.Emailing;

    public class CouponController : BaseController
    {
        #region Fields

        private readonly OodsServices oodsService;
        private readonly IRegistrationProfileRepository registrationProfileRepository;
        private readonly ICouponRequestService couponRequestService;
        private readonly ICouponLandingPageRepository couponLandingPageRepository;
        private readonly ICouponService couponService;
        private readonly IEmailRepository emailRepository;

        #endregion

        #region Constructors & Destructors

        public CouponController(IConfigService configService, ICommonService commonService, IEmailRepository emailRepositoryParam,
            IRegistrationProfileRepository registrationProfileRepository,
            ICouponRequestService couponRequestService,
            ICouponLandingPageRepository couponLandingPageRepository,
            OodsServices oodsService, ICouponService couponService)
            : base(configService, commonService)
        {
            this.registrationProfileRepository = registrationProfileRepository;
            this.couponRequestService = couponRequestService;
            this.oodsService = oodsService;
            this.emailRepository = emailRepositoryParam;
            this.couponLandingPageRepository = couponLandingPageRepository;
            this.couponService = couponService;
        }

        #endregion

        #region Methods

        public async Task<ActionResult> GetPrintCouponToken(string couponId, string returnUrl, string device, string printLogin)
        {
            var culture = System.Threading.Thread.CurrentThread.CurrentUICulture;
            if (Session["culture"] != null)
            {
                culture = (CultureInfo)Session["culture"];
            }

            var urlOffers = couponLandingPageRepository.GetCouponLandingPageUrlName(culture.ToString());

            if (!Authentication.isLogin())
            {
                var registrationUrl = registrationProfileRepository.GetRegistrationProfileUrlAlias(culture.ToString());

                Session["coupon_event_label"] = "couponing";


                //tracking for welcome mail
                Session["scenario_name"] = "couponing";

                if (Request.IsAjaxRequest())
                {
                    if (device != null && device.Contains("mobile"))
                    {
                        return Json(new { urlMobile = "/" + registrationUrl, url = "/" + urlOffers + "?" + printLogin });
                    }
                    return Json(new { urlLogin = "/" + registrationUrl, url = "/"+ urlOffers + "?" + printLogin });
                }

                return Redirect("/" + registrationUrl);
            }

            var couponProviderSetting =
                Umbraco.TypedContentAtXPath("//couponProviderSettings")
                    .FirstOrDefault(x => x.GetCulture().Equals(culture));

            //var consumerId = Authentication.GetConsumerId();
            const string consumerId = "000000000000003";

            var response = await couponService.GetRequestToken(new CouponProviderSetting
            {
                ApiUri = new Uri(couponProviderSetting.GetPropertyValue<string>("apiUrl")),
                Username = couponProviderSetting.GetPropertyValue<string>("username"),
                Password = couponProviderSetting.GetPropertyValue<string>("password"),
                TwoLetterCountryCode = couponProviderSetting.GetPropertyValue<string>("twoLetterCountryCode"),
                SynSource = couponProviderSetting.GetPropertyValue<string>("syncSource"),
                ReturnUrl = returnUrl
            },
                couponId, consumerId);

            var token = new ContentResult {Content = response, ContentType = "application/json"};

            var couponRequest = new CouponRequest
            {
                CouponId = couponId,
                PrintDate = DateTime.Now,
                RequestDate = DateTime.Now,
                UserId = Authentication.GetConsumerId()
            };

            var tokenToPrint = JsonConvert.DeserializeObject<string>(token.Content);
           
            if (device != null && device.Contains("mobile"))
            {
                var user = oodsService.GetUser(int.Parse(Authentication.GetConsumerId()));
                var name = "";
                if (user != null)
                {
                    name = user.consumer.FIRSTNAME;
                }

                EmailTemplate couponTemplate = emailRepository.GetEmailTemplate("Coupon", culture.ToString());
                var profileUrl = registrationProfileRepository.GetEditProfileUrlAlias(culture.ToString());
                string couponUtm = "utm_medium=crm-email&utm_source=crm&utm_term=print-my-coupon-cta&utm_content=print-my-coupon&utm_campaign=mobile-printing-coupon";
                string utmUpdateProfile = "utm_medium=crm-email&utm_source=crm&utm_term=update-your-profile-cta&utm_content=print-my-coupon&utm_campaign=mobile-printing-coupon";
                string[] couponPrintLaterToken = { "Print_Coupon_CTA=" + Helpers.UrlHelper.GetBaseUrl()+"/" + urlOffers + "?" + printLogin + "&" + couponUtm, "Profile_URL="+ Helpers.UrlHelper.GetBaseUrl() + "/" + profileUrl + "?" + utmUpdateProfile, "Subject="+ couponTemplate.Subject, "First_Name=" + name }; //TODO : Localized Subject, localized URL


                var msEmail = oodsService.SendMSEmail(Authentication.GetUsername(), Convert.ToString(couponTemplate.EmailId), int.Parse(Authentication.GetConsumerId()), couponPrintLaterToken);

                switch (msEmail.Code)
                {
                    case 1000:
                        return Json(new { success = "Success" });
                    case 1003:
                        return Json(new { MessageError = "Service Unavailable" });
                    case 1005:
                        return Json(new { MessageError = "Account not active" });
                    case 1008:
                        return Json(new { MessageError = "Email not sent" });
                    case 1009:
                        return Json(new { MessageError = "Connection failed" });
                    case 1010:
                        return Json(new { MessageError = "Password expired" });
                }
                return Json(new { MessageError = "Server Not Found" });
            }

            couponRequestService.Save(couponRequest);

            if (Request.IsAjaxRequest())
            {
                return Json(new { url = tokenToPrint });
            }
            return Redirect(tokenToPrint);
        }

        #endregion
    }
}
