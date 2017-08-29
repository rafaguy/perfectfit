namespace Mars.PerfectFit.Presentation.Web.Controllers
{
    using System.Linq;
    using System.Web.Mvc;
    using Mars.PerfectFit.Core.Domain;
    using Mars.PerfectFit.Core.Domain.Models.Sampling;
    using Mars.PerfectFit.Core.Domain.Repositories;
    using Mars.PerfectFit.Core.Domain.Services;
    using Mars.PerfectFit.Infrastructure.CrmService.Services;
    using Mars.PerfectFit.Presentation.Web.Security;
    using Umbraco.Web;

    public class SampleController : BaseController
    {
        private readonly IEmailRepository emailRepository;
        private readonly OodsServices oodsServices;
        private readonly IRegistrationProfileRepository registrationProfileRepository;
        private readonly IRepository repository;

        public SampleController(IConfigService configService, ICommonService commonService,
            IRegistrationProfileRepository registrationProfileRepository,
            IRepository repository,
            OodsServices oodsServices,
            IEmailRepository emailRepository) : base(configService, commonService)
        {
            this.registrationProfileRepository = registrationProfileRepository;
            this.repository = repository;
            this.oodsServices = oodsServices;
            this.emailRepository = emailRepository;
        }
        //TODO: Sample Tracking
        public ActionResult ProcessRequest(string culture, string sampleId)
        {
            if (!Authentication.isLogin())
            {
                //temp variable for DL
                Session["sample_event_label"] = "sample";

                Session["scenario_name"] = "sample";

                var registrationUrl = registrationProfileRepository.GetRegistrationProfileUrlAlias(culture);

                var offersUrl = Umbraco
                    .TypedContentAtXPath("//sampleListing")
                    .FirstOrDefault(x => x.GetCulture().ToString().Equals(culture));

                var returnUrl = offersUrl != null ? offersUrl.GetPropertyValue<string>("umbracoUrlAlias") : string.Empty;

                return Redirect($"/{registrationUrl}?returnUrl=/{returnUrl}");
            }

            var sampleSettingContainer = Umbraco
                .TypedContentAtXPath("//sampleSettings")
                .Single(x => x.GetCulture().ToString().Equals(culture));

            var sampleSetting = new SampleSetting
            {
                GlobalRequestLimit = sampleSettingContainer.GetPropertyValue<int?>("globalRequestLimit") ?? 0,
                CellId = sampleSettingContainer.GetPropertyValue<string>("cellId") ?? string.Empty
            };

            var consumerId = int.Parse(Authentication.GetConsumerId());

            var user = oodsServices.GetUser(consumerId);

            user.consumer.CAMPAIGN_RESPONSE_CODE = sampleSetting.CellId;

            var response = oodsServices.UpdateUser(user, Request.UserHostAddress);

            if (response == null)
            {
                var sampleRequest = new SampleRequest
                {
                    SampleId = sampleId,
                    UserId = consumerId
                };

                repository.Add(sampleRequest);
                repository.UnitOfWork.Commit();

                var service = Services.ContentService;

                sampleSetting.GlobalRequestLimit -= 1;

                var content = service.GetById(sampleSettingContainer.Id);

                content.SetValue("globalRequestLimit", sampleSetting.GlobalRequestLimit);

                var attempt = service.SaveAndPublishWithStatus(content);

                if (!attempt.Success)
                {
                    return Json(attempt.Exception.Message);
                }

                var emailTemplate = emailRepository.GetEmailTemplate("Sample", culture);

                var parameters = new[] { string.Concat("Subject=", emailTemplate.Subject) };

                var emailResponse = oodsServices.SendMSEmail(Authentication.GetUsername(), emailTemplate.EmailId.ToString(), consumerId, parameters);

                if (emailResponse.Code != 1000)
                {
                    return Json(emailResponse.Message, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                TempData["Error"] = 1;
            }

            var thankYouPage = Umbraco
                .TypedContentAtXPath("//thankYou")
                .FirstOrDefault(x => x.GetCulture().ToString().Equals(culture) && x.GetPropertyValue<string>("relatedPage").Equals("Sample"));

            var redirect = string.Empty;

            if (thankYouPage != null)
            {
                redirect = thankYouPage.UrlName;
            }

            return Redirect($"/{redirect}");
        }
    }
}
