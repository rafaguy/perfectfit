using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mars.PerfectFit.Core.Domain.Services;
using Mars.PerfectFit.Presentation.Web.ViewModels;
using Umbraco.Web;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Mars.PerfectFit.Core.Domain.Models.MoneyBackGuarantee;
using Mars.PerfectFit.Presentation.Web.Security;
using Mars.PerfectFit.Infrastructure.CrmService.ServiceCRMReference;
using Mars.PerfectFit.Infrastructure.CrmService.Services;
using Mars.PerfectFit.Presentation.Web.Helpers;
using Mars.PerfectFit.Presentation.Web.Constents;
using Mars.PerfectFit.Core.Domain.Repositories;
using Mars.PerfectFit.Core.Domain.Models.Emailing;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Dynamic;

namespace Mars.PerfectFit.Presentation.Web.Controllers
{
    using Umbraco.Core.Logging;
    using Umbraco.Core.Models;
    using static Application.MbgResponse;

    public class MoneyBackGuaranteeInformationController : BaseController
    {
        private readonly IMoneyBackGuaranteeService mbgService;
        private readonly IMbgRequestService mbgRequestService;
        private readonly IEmailRepository emailRepository;
        private readonly IRegistrationProfileRepository registrationProfileRepository;
        private readonly OodsServices oodsService;

        public MoneyBackGuaranteeInformationController(IRegistrationProfileRepository registrationProfileRepository, IEmailRepository emailRepository, IMbgRequestService mbgRequestService, OodsServices oodsService, IMoneyBackGuaranteeService mbgService, IConfigService configService, ICommonService commonService) : base(configService, commonService)
        {
            this.mbgService = mbgService;
            this.oodsService = oodsService;
            this.mbgRequestService = mbgRequestService;
            this.emailRepository = emailRepository;
            this.registrationProfileRepository = registrationProfileRepository;
        }

        public ActionResult MoneyBackGuaranteeInformation(string modif = "")
        {
            //Get Culture by Url
            var urlCult = Request.Url.ToString();
            var protocolCult = urlCult.Split('/').First();
            var domainCult = urlCult.Split('/')[2];
            var domain = string.Concat(protocolCult, "//", domainCult);

            var mbgCulture = Umbraco.TypedContentAtXPath("//moneyBackGuaranteeInformation").
                  FirstOrDefault(x => string.Concat(x.UrlWithDomain().Split('/')[0], "//", x.UrlWithDomain().Split('/')[2]) == domain);

            var culture = mbgCulture != null ? mbgCulture.GetCulture() : null;

            TempData["culture"] = culture;

            dynamic datalayer = new ExpandoObject();
            datalayer.MetaDataSiteMap = "Money Back Guarantee";
            if (Session["user_profile_dl"] != null)
            {
                datalayer.MetaDataUserProfile = Session["user_profile_dl"];
            }
            ViewBag.DataLayer = this.GenerateCustomVariable(datalayer);

            if (!Authentication.isLogin())
            {
                Session["mbg_event_label"] = "Money Back Guarantee";
                Session["scenario_name"] = "mbg";

                ViewBag.DataLayer = this.GenerateCustomVariable(datalayer);
                var mbgPage =
                Umbraco.TypedContentAtXPath("//moneyBackGuarantee")
                    .FirstOrDefault(x => x.GetCulture().Equals(culture));

                var mbgUrlAlias = mbgPage.GetPropertyValue<string>("umbracoUrlAlias");

                if (!string.IsNullOrEmpty(modif))
                {
                    return Redirect("/" + mbgUrlAlias + "?returnModif=?modif=" + modif);
                }

                return Redirect("/" + mbgUrlAlias);
            }

            var checkMbgUser = mbgRequestService.GetMbgRequestCount(Authentication.GetConsumerId());

            MbgUpdateViewModel mbgUpdate = new MbgUpdateViewModel();

            if (checkMbgUser > 0)
            {
                var localUserParticipation = mbgRequestService.Get(Authentication.GetConsumerId());

                if (localUserParticipation.RequestStat.CompareTo("valid") == 0 || localUserParticipation.RequestStat.CompareTo("rejected") == 0 || modif.Equals(localUserParticipation.SogecParticipationId) == false)
                {
                    TempData["errorNumero"] = "Merci";
                    TempData["errorMessate"] = "Vous avez déjà fait une demande de remboursement";

                    var mbgConfirmationPage =
                        Umbraco.TypedContentAtXPath("//moneyBackGuaranteeConfirmation")
                            .FirstOrDefault(x => x.GetCulture().Equals(culture));

                    var confirmationUrlAlias = mbgConfirmationPage.GetPropertyValue<string>("umbracoUrlAlias");


                    return Redirect("/" + confirmationUrlAlias);
                }
                else if (localUserParticipation.RequestStat.CompareTo("pending") == 0 && !string.IsNullOrEmpty(modif))
                {
                    mbgUpdate.Iban = localUserParticipation.Iban;
                    mbgUpdate.Gencode = localUserParticipation.Gencode;
                    mbgUpdate.GencodePhotoId = localUserParticipation.GencodePhotoId;
                    mbgUpdate.ReceiptPhotoId = localUserParticipation.ReceiptPhotoId;
                    mbgUpdate.ApiParticipation = localUserParticipation.SogecParticipationId;
                }
            }

            var mbgInformationPage =
                Umbraco.TypedContentAtXPath("//moneyBackGuaranteeInformation")
                    .FirstOrDefault(x => x.GetCulture().Equals(culture));

            return View(new MoneyBackGuaranteeInformationViewModel
            {
                ImageBanner = mbgInformationPage.GetPropertyValue<string>("banner"),
                TitleBold = mbgInformationPage.GetPropertyValue<string>("title"),
                SimpleTitle = mbgInformationPage.GetPropertyValue<string>("subtitle"),
                ShortDescription = mbgInformationPage.GetPropertyValue<string>("shortDescription"),
                LongDescription = mbgInformationPage.GetPropertyValue<string>("longDescription"),
                LegalText = mbgInformationPage.GetPropertyValue<string>("legalText"),
                MbgUpdate = mbgUpdate
            });
        }

        [HttpPost]
        public async Task<ActionResult> ValidationInformation()
        {
            var culture = TempData["culture"] as CultureInfo;

            var iban = Request.Form[0];
            var gencode = Request.Form[1];
            var participation = Request.Form[2];

            User user = oodsService.GetUser(int.Parse(Authentication.GetConsumerId()));

            

            string gencodePhotoId = "";
            string receiptPhotoId = "";
            var md5FileName = "";

            MD5 md5 = MD5.Create();
            var userEmail = user.login.username;
            var currentDate = DateTime.Now.ToString("dd/MM/yyyy");
            var userToMd5 = userEmail + currentDate;
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(userToMd5);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();

            foreach (byte t in hash)
            {
                sb.Append(t.ToString("x2"));
            }

            md5FileName = sb.ToString();

            if (Request.Files.Count > 0)
            {
                try
                {
                    var files = Request.Files;

                    for (var i = 0; i < files.Count; i++)
                    {
                        var file = files[i];

                        string fname;

                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            var testfiles = file.FileName.Split('\\');
                            fname = testfiles[testfiles.Length - 1];
                        }
                        else
                        {

                            fname = file.FileName;
                        }

                        if (!string.IsNullOrEmpty(fname))
                        {
                            var ext = Path.GetExtension(file.FileName)?.ToLower() ?? string.Empty;

                            var ms = Services.MediaService;

                            //var parent = MediaHelper.GetOrCreateMediaFolders(Services.MediaService, CurrentPage.GetCulture().ToString(), "MBG");

                            var mediaMap = ms.CreateMedia(fname, 1868, "Image");

                            mediaMap.SetValue("umbracoFile", sb + "_" + files.AllKeys[i] + ext, file.InputStream);

                            mediaMap.Name = sb + "_" + files.AllKeys[i] + ext;

                            ms.Save(mediaMap);

                            if (files.AllKeys[i].Contains("gencode")) gencodePhotoId = mediaMap.Id.ToString();
                            if (files.AllKeys[i].Contains("receipt")) receiptPhotoId = mediaMap.Id.ToString();

                        }
                    }
                }
                catch (Exception ex)
                {
                    return Json(ex.Message, JsonRequestBehavior.AllowGet);
                }
            }

            string paddedConsumerId = Authentication.GetConsumerId().PadRight(10, '0');

            var mbgParticipationToSend = new MbgRequest
            {
                GeneratedPraticipationId = long.Parse(paddedConsumerId),
                userId = Authentication.GetConsumerId(),
                RequestDate = DateTime.Now,
                Md5FileName = md5FileName,
                Iban = iban,
                Gencode = gencode
            };

            if (!string.IsNullOrEmpty(gencodePhotoId)) mbgParticipationToSend.GencodePhotoId = gencodePhotoId;
            if (!string.IsNullOrEmpty(receiptPhotoId)) mbgParticipationToSend.ReceiptPhotoId = receiptPhotoId;

            var mbgProviderSetting =
                Umbraco.TypedContentAtXPath("//moneyBackGuaranteeSettings")
                    .FirstOrDefault(x => x.GetCulture().Equals(culture));

            var providerSetting = new MbgProviderSetting
            {
                ApiUrlAuthentication = new Uri(mbgProviderSetting.GetPropertyValue<string>("apiUrlAuthentication")),
                ApiUrlParticipation = new Uri(mbgProviderSetting.GetPropertyValue<string>("apiUrlParticipation")),
                Username = mbgProviderSetting.GetPropertyValue<string>("username"),
                Password = mbgProviderSetting.GetPropertyValue<string>("password"),
                OperationCode = mbgProviderSetting.GetPropertyValue<string>("operationCode")
            };

            var response = await mbgService.GetAuthenticationToken(providerSetting);
            var authenticateResult = new ContentResult { Content = response, ContentType = "application/json" };
            var contentResultJson = JsonConvert.DeserializeObject<dynamic>(authenticateResult.Content);
            string token = contentResultJson.token;

            var participationAction = "create";

            if (!string.IsNullOrEmpty(participation))
            {
                participationAction = "update";

                var mbgPhotoUpdate = mbgRequestService.Get(Authentication.GetConsumerId());
                if (string.IsNullOrEmpty(gencodePhotoId)) gencodePhotoId = mbgPhotoUpdate.GencodePhotoId;
                if (string.IsNullOrEmpty(receiptPhotoId)) receiptPhotoId = mbgPhotoUpdate.ReceiptPhotoId;

            }

            var image = new List<MbgImageParameter>();
            if (!string.IsNullOrEmpty(gencodePhotoId))
            {
                var media = Umbraco.TypedMedia(int.Parse(gencodePhotoId));
                var url = Helpers.UrlHelper.GetBaseUrl().Replace("https", "http") + media.Url;

                //var url = "http://i.imgur.com/ijQOg0R.jpg";
                image.Add(new MbgImageParameter { src = url });
            }
            if (!string.IsNullOrEmpty(receiptPhotoId))
            {
                var media = Umbraco.TypedMedia(int.Parse(receiptPhotoId));
                var url = Helpers.UrlHelper.GetBaseUrl().Replace("https", "http") + media.Url;
                //var url = "http://i.imgur.com/JsaSHB7.jpg";
                image.Add(new MbgImageParameter { src = url });
            }

            var userAddress1 = string.Empty;
            var userAddress2 = string.Empty;
            if (!string.IsNullOrEmpty(user.consumer.STREET_ADDRESS))
            {
                userAddress1 = user.consumer.STREET_ADDRESS;
                if (userAddress1.Length > 38) userAddress1 = userAddress1.Substring(0, 38);
            }
            if (!string.IsNullOrEmpty(user.consumer.STREET_ADDRESS2))
            {
                userAddress2 = user.consumer.STREET_ADDRESS2;
                if (userAddress2.Length > 38) userAddress2 = userAddress2.Substring(0, 38);
            }

            var data = new
            {
                participation = new
                {
                    global = new
                    {
                        participation_id = mbgParticipationToSend.GeneratedPraticipationId,
                        action = participationAction,
                        channel = "FW"
                    },
                    consumer = new
                    {
                        lastname = user.consumer.SURNAME,
                        firstname = user.consumer.FIRSTNAME,
                        address_3 = userAddress1,
                        address_1 = userAddress2,
                        city = user.consumer.CITY,
                        postal_code = user.consumer.POSTAL_CODE,
                        iban = iban
                    },
                    consumer2 = new { },
                    images = image.ToArray(),
                    product = new[]
                    {
                        new
                        {
                            product_carrier = gencode
                        }
                    },
                    proofs = new List<string>()
                }
            };

            var dataParameter = JsonConvert.SerializeObject(data);

            var participationResponse = await mbgService.SendParticipation(providerSetting, token, dataParameter);
            var participationResult = new ContentResult { Content = participationResponse, ContentType = "application/json" };
            var participationResultJson = JsonConvert.DeserializeObject<dynamic>(participationResult.Content);

            var message = string.Empty;
            var participationId = string.Empty;
            string errorNumero;
            string errorMessate;

            if (participationResultJson is Boolean)
            {
                errorNumero = "Erreur du fournisseur";
                errorMessate = "Le fournisseur ne répond pas";
            }
            else
            {
                message = participationResultJson.message;
                participationId = participationResultJson.participation_id;
                errorNumero = participationResultJson.errno;
                errorMessate = participationResultJson.errmsg;
            }

            if (string.IsNullOrEmpty(errorNumero)){

                Session["MBGEventLabel"] = "mbg_complete";
            }

            TempData["errorNumero"] = errorNumero;
            TempData["errorMessate"] = errorMessate;

            if (!string.IsNullOrEmpty(participationId))
            {
                mbgParticipationToSend.SogecParticipationId = participationId;
                mbgParticipationToSend.RequestStat = "pending";

                if (!string.IsNullOrEmpty(participation))
                {
                    var mbgParticipationUpdate = mbgRequestService.Get(Authentication.GetConsumerId());

                    mbgParticipationUpdate.SogecParticipationId = participationId;
                    mbgParticipationUpdate.RequestDate = DateTime.Now;
                    mbgParticipationUpdate.Md5FileName = md5FileName;
                    mbgParticipationUpdate.Iban = iban;
                    mbgParticipationUpdate.Gencode = gencode;
                    if (!string.IsNullOrEmpty(gencodePhotoId)) mbgParticipationUpdate.GencodePhotoId = gencodePhotoId;
                    if (!string.IsNullOrEmpty(receiptPhotoId)) mbgParticipationUpdate.ReceiptPhotoId = receiptPhotoId;
                    mbgParticipationUpdate.RequestStat = "pending";

                    mbgRequestService.Update(mbgParticipationUpdate);
                }
                else
                {
                    mbgRequestService.Save(mbgParticipationToSend);
                }

                EmailTemplate email = emailRepository.GetEmailTemplate("MBG Confirmation", culture.ToString());
                string[] mbgEmailParam = { "Subject=" + email.Subject };
                var msEmail = oodsService.SendMSEmail(Authentication.GetUsername(), Convert.ToString(email.EmailId), int.Parse(Authentication.GetConsumerId()), mbgEmailParam);

                switch (msEmail.Code)
                {
                    case 1000:
                        break;
                    case 1003:
                        TempData["errorNumero"] = "Email API Exception";
                        TempData["errorMessate"] = "Service Unavailable";
                        break;
                    case 1005:
                        TempData["errorNumero"] = "Email API Exception";
                        TempData["errorMessate"] = "Account not active";
                        break;
                    case 1008:
                        TempData["errorNumero"] = "Email API Exception";
                        TempData["errorMessate"] = "Email not sent";
                        break;
                    case 1009:
                        TempData["errorNumero"] = "Email API Exception";
                        TempData["errorMessate"] = "Connection failed";
                        break;
                    case 1010:
                        TempData["errorNumero"] = "Email API Exception";
                        TempData["errorMessate"] = "Password expired";
                        break;
                }
            }
            else
            {
                var ms = Services.MediaService;

                if (!string.IsNullOrEmpty(gencodePhotoId))
                {
                    var media = ms.GetById(int.Parse(gencodePhotoId));
                    ms.Delete(media);
                }
                if (!string.IsNullOrEmpty(receiptPhotoId))
                {
                    var media = ms.GetById(int.Parse(receiptPhotoId));
                    ms.Delete(media);
                }
            }

            var mbgConfirmationPage =
                Umbraco.TypedContentAtXPath("//moneyBackGuaranteeConfirmation")
                    .FirstOrDefault(x => x.GetCulture().Equals(culture));

            var confirmationUrlAlias = mbgConfirmationPage.GetPropertyValue<string>("umbracoUrlAlias");


            return Redirect("/" + confirmationUrlAlias);
        }
        

    }
}
