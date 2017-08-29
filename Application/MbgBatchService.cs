using static Mars.PerfectFit.Presentation.Web.Application.MbgResponse;

namespace Mars.PerfectFit.Presentation.Web.Application
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Mars.PerfectFit.Core.Application;
    using Mars.PerfectFit.Core.Domain.Models.MoneyBackGuarantee;
    using Mars.PerfectFit.Core.Domain.Repositories;
    using Mars.PerfectFit.Core.Domain.Services;
    using Mars.PerfectFit.Infrastructure.CrmService.Services;
    using Mars.PerfectFit.Presentation.Web.Helpers;
    using Newtonsoft.Json;
    using Umbraco.Web;


    public sealed class MbgBatchService : IMbgBatchService
    {
        private readonly IEmailRepository emailRepository;
        private readonly IMbgRequestService mbgRequestService;
        private readonly IMoneyBackGuaranteeService mbgService;
        private readonly OodsServices oodsService;
        private readonly UmbracoHelper umbracoHelper;

        #region Constructors & Destructors

        public MbgBatchService(IEmailRepository emailRepository, IMbgRequestService mbgRequestService, IMoneyBackGuaranteeService mbgService)
        {
            this.mbgService = mbgService;
            this.mbgRequestService = mbgRequestService;
            this.emailRepository = emailRepository;
            oodsService = new OodsServices();
            umbracoHelper = new UmbracoHelper(ContextHelper.GetUmbracoContext());
        }

        #endregion

        #region Implementations

        public async Task ProcessPendingRequests()
        {
            var culture = CultureInfo.CreateSpecificCulture("fr-FR");

            var allRequest = mbgRequestService.GetAllPendingRequest();

            var mbgProviderSetting =
                umbracoHelper.TypedContentAtXPath("//moneyBackGuaranteeSettings")
                    .FirstOrDefault(x => x.GetCulture().Equals(culture));

            if (mbgProviderSetting == null)
            {
                throw new ArgumentNullException(nameof(mbgProviderSetting), "Provider Setting cannot be null");
            }

            var providerSetting = new MbgProviderSetting
            {
                ApiUrlAuthentication = new Uri(mbgProviderSetting.GetPropertyValue<string>("apiUrlAuthentication")),
                ApiUrlParticipation = new Uri(mbgProviderSetting.GetPropertyValue<string>("apiUrlParticipation")),
                Username = mbgProviderSetting.GetPropertyValue<string>("username"),
                Password = mbgProviderSetting.GetPropertyValue<string>("password"),
                OperationCode = mbgProviderSetting.GetPropertyValue<string>("operationCode")
            };

            var authenticationResponse = await mbgService.GetAuthenticationToken(providerSetting);
            var authenticateResult = new ContentResult {Content = authenticationResponse, ContentType = "application/json"};
            var contentResultJson = JsonConvert.DeserializeObject<dynamic>(authenticateResult.Content);
            string token = contentResultJson.token;

            foreach (var request in allRequest)
            {
                var participationResponse = await mbgService.GetParticipationStatus(providerSetting, token, request.SogecParticipationId);
                var participationResult = new ContentResult {Content = participationResponse, ContentType = "application/json"};
                var participationResultJson = JsonConvert.DeserializeObject<dynamic>(participationResult.Content);

                if (participationResultJson is bool)
                {
                }
                else
                {
                    var participationResultJsonObject = JsonConvert.DeserializeObject<MbgRootobject>(participationResult.Content);

                    if (participationResultJsonObject.campagne != null)
                    {
                        var campagne = participationResultJsonObject.campagne[0];
                        var participationId = campagne.global.participation_id;

                        if (campagne.global.is_conform != null)
                        {
                            var user = oodsService.GetUser(int.Parse(request.userId));

                            if (user == null)
                            {
                                throw new ArgumentNullException(nameof(user), "User cannot be null");
                            }

                            switch (campagne.global.is_conform.ToString())
                            {
                                case "0":

                                    var conformityType = campagne.global.non_conformity_type.ToString().ToLower();

                                    if (conformityType.Equals("def"))
                                    {
                                        var emailRejected = emailRepository.GetEmailTemplate("MBG Request Rejected", culture.ToString());
                                        string[] mbgEmailRejectedParam = { "Subject=" + emailRejected.Subject, "First_Name=" + user.consumer.FIRSTNAME };
                                        oodsService.SendMSEmail(user.login.username, Convert.ToString(emailRejected.EmailId), user.consumer.CONSUMER_ID, mbgEmailRejectedParam);

                                        request.RequestStat = "rejected";
                                        mbgRequestService.Update(request);
                                    }

                                    if (conformityType.Equals("temp"))
                                    {
                                        var mbgInformationPage =
                                            umbracoHelper.TypedContentAtXPath("//moneyBackGuaranteeInformation")
                                                .FirstOrDefault(x => x.GetCulture().Equals(culture));

                                        var mbgInformationUrlAlias = mbgInformationPage.GetPropertyValue<string>("umbracoUrlAlias");

                                        var emailIncomplete = emailRepository.GetEmailTemplate("MBG Request Incomplete", culture.ToString());
                                        string[] mbgEmailIncompleteParam = { "Subject=" + emailIncomplete.Subject, "Link_URL=" + Helpers.UrlHelper.GetBaseUrl() + "/" + mbgInformationUrlAlias + "?modif=" + participationId, "First_Name=" + user.consumer.FIRSTNAME };
                                        oodsService.SendMSEmail(user.login.username, Convert.ToString(emailIncomplete.EmailId), user.consumer.CONSUMER_ID, mbgEmailIncompleteParam);

                                        request.RequestStat = "Incomplete";
                                        mbgRequestService.Update(request);
                                    }

                                    break;

                                case "1":

                                    var emailValid = emailRepository.GetEmailTemplate("MBG Request Validated", culture.ToString());
                                    string[] mbgEmailValidParam = {"Subject=" + emailValid.Subject};
                                    oodsService.SendMSEmail(user.login.username, Convert.ToString(emailValid.EmailId), user.consumer.CONSUMER_ID, mbgEmailValidParam);

                                    request.RequestStat = "valid";
                                    mbgRequestService.Update(request);

                                    break;
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
