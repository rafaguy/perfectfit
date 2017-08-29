namespace Mars.PerfectFit.Presentation.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Web.Mvc;
    using Mars.PerfectFit.Core.Domain.Models.Profile;
    using Mars.PerfectFit.Core.Domain.Repositories;
    using Mars.PerfectFit.Core.Domain.Services;
    using Mars.PerfectFit.Core.Utilities;
    using Mars.PerfectFit.Infrastructure.CrmService.ServiceCRMReference;
    using Mars.PerfectFit.Infrastructure.CrmService.Services;
    using Mars.PerfectFit.Presentation.Web.Security;
    using Mars.PerfectFit.Presentation.Web.ViewModels;
    using Umbraco.Web;
    using Constents;
    using System.Dynamic;
    using Helpers;
    using Core.Domain.Models.Emailing;

    public class RegistrationPetsController : BaseController
    {
        #region Fields

        private readonly IRegistrationPetsRepository registrationPetsRepository;
        private readonly UmbracoHelper helperUmbraco;
        private readonly OodsServices oodsService;
        private readonly IRegistrationProfileRepository registrationProfileRepository;
        private readonly IRegistrationConfirmationRepository registrationConfirmationRepository;
        private readonly ILoginRepository loginRepository;
        private readonly IEmailRepository emailRepository;
        private readonly ISettingRepository settingRepository;
        private readonly IProfileSettingsRepository profileSettingsRepository;
        #endregion

        #region Constructors & Destructors

        public RegistrationPetsController(IConfigService configService, ICommonService commonService, IEmailRepository emailRepositoryParam,
            OodsServices oodsService, IRegistrationPetsRepository registrationPetsRepository, ISettingRepository settingRepositoryParam,
            IRegistrationProfileRepository registrationProfileRepository, IProfileSettingsRepository profileSettingsRepositoryParam,
            IRegistrationConfirmationRepository registrationConfirmationRepository, ILoginRepository loginRepository) : base(configService, commonService)
        {
            this.oodsService = oodsService;
            emailRepository = emailRepositoryParam;
            this.registrationPetsRepository = registrationPetsRepository;
            helperUmbraco = new UmbracoHelper(UmbracoContext.Current);
            this.registrationProfileRepository = registrationProfileRepository;
            this.registrationConfirmationRepository = registrationConfirmationRepository;
            this.loginRepository = loginRepository;
            this.settingRepository = settingRepositoryParam;
            this.profileSettingsRepository = profileSettingsRepositoryParam;
        }

        #endregion
        #region Methods and variables utiles
        string cultureInf = "fr-FR";
        string EmailId = "157643";


        private string GetFavouritLang()
        {
            string favLang = "EN";
            string culture = Thread.CurrentThread.CurrentUICulture.ToString();

            if (culture.StartsWith("en"))
            {
                favLang = "EN";
            }
            else if (culture.StartsWith("de"))
            {
                favLang = "DE";
            }
            else if (culture.StartsWith("fr"))
            {
                favLang = "FR";
            }
            return favLang;
        }
        private string GetClientIP()
        {
            return (Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ??
                    Request.ServerVariables["REMOTE_ADDR"]).Split(',')[0].Trim();
        }

        #endregion

        #region Methods

        public ActionResult RegistrationPets()
        {
            var registrationPets = registrationPetsRepository.GetRegistrationPets(CurrentPage.Id);


            if (TempData["isStep1"] == null)
            {
                var urlAliasRegistrationProfile = registrationProfileRepository.GetRegistrationProfileUrlAlias(System.Threading.Thread.CurrentThread.CurrentUICulture.ToString());
                return Redirect("/" + urlAliasRegistrationProfile);
            }

            var model = new RegistrationPetsViewModel
            {
                Banner = registrationPets.Banner,
                Title = registrationPets.Title,
                Description = registrationPets.Description,
                ReceiveMarketingEmail = profileSettingsRepository.GetReceiveMarketingEmail(System.Threading.Thread.CurrentThread.CurrentUICulture.ToString())
            };

            dynamic datalayer = new ExpandoObject();
            datalayer.MetaDataSiteMap = "Registration";
            ViewBag.DataLayer = this.GenerateCustomVariable(datalayer);



            return View(model);
        }


        public ActionResult PetsValidation()
        {

            var urlCult = Request.Url.ToString();
            var protocolCult = urlCult.Split('/').First();
            var domainCult = urlCult.Split('/')[2];
            var cultureBydom = loginRepository.GetCultureByDomain(string.Concat(protocolCult, "//", domainCult));

            var optPerfectFitBrandId = 278; //PerfectFit Brand id

            var photoId = new Dictionary<int, int>();

            var forms = Request.Form[0];

            var dataroot = Newtonsoft.Json.JsonConvert.DeserializeObject<DataRoot>(forms);

            var data = dataroot.data;

            var hearPerfectFit = data.hearPerfectFit;
            
            var petsModel = data.petsModel;

            if (Request.Files.Count > 0)
            {
                try
                {
                    //Get all files from Request object
                    var files = Request.Files;
                    for (var i = 0; i < files.Count; i++)
                    {
                        var file = files[i];

                        string fname;
                        //checking for Internet Explorer
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            var testfiles = file.FileName.Split('\\');
                            fname = testfiles[testfiles.Length - 1];
                        }
                        else
                        {
                            fname = file.FileName;
                        }

                        var userFolder = (User)Session["userRegistration"];
                        fname = Path.Combine(Server.MapPath("~/App_Data/TEMP/"), fname);
                        file.SaveAs(fname);
                        var ms = Services.MediaService;


                       
                        var folder = MediaHelper.GetOrCreateMediaFolders(ms, cultureBydom.ToString(), "Account-Pet Images");
                        var mediaMap = ms.CreateMedia(fname, folder, "Image");
                        mediaMap.SetValue("umbracoFile", file);
                        mediaMap.SetValue("altText", file.FileName);
                        mediaMap.Name = file.FileName;

                        var photoNameIndex = files.AllKeys[i];
                        var photoIndex = int.Parse(files.AllKeys[i].Substring(photoNameIndex.Length - 1));
                        ms.Save(mediaMap);
                        photoId.Add(photoIndex, mediaMap.Id);
                        System.IO.File.Delete(fname);
                    }
                }
                catch (Exception ex)
                {
                    return Json(ex.Message);
                }
            }

            var model = (RegisterModel)Session["userModel"];

            var user = (User)Session["userRegistration"];            

            if (hearPerfectFit)
            {
                if (user.marketingPermissions != null && user.marketingPermissions.Length > 0)
                {
                    List<MarketingPermission> userMP = new List<MarketingPermission>();
                    MarketingPermission newUserMP = new MarketingPermission
                    {
                        OPT_IN_SOURCE_CODE = "OODS",
                        OPT_IN_CHANNEL_CODE = 0,
                        OPT_IN_TYPE_CODE = 1,
                        OPT_IN_METHOD_CODE = 1,
                        OPT_IN_BRAND_ID = optPerfectFitBrandId
                    };
                    userMP.Add(user.marketingPermissions[0]);
                    userMP.Add(newUserMP);
                    user.marketingPermissions = userMP.ToArray();
                }
                else
                {
                    user.marketingPermissions = new MarketingPermission[]
                    {
                        new MarketingPermission
                        {
                            OPT_IN_SOURCE_CODE = "OODS",
                            OPT_IN_CHANNEL_CODE = 0,
                            OPT_IN_TYPE_CODE = 1,
                            OPT_IN_METHOD_CODE = 1,
                            OPT_IN_BRAND_ID = optPerfectFitBrandId
                        }
                    };
                }
            }

            Pet[] userPets = new Pet[petsModel.Count];

            var countCat = 0;
            var countDog = 0;

            for (var i = 0; i < userPets.Length; i++)
            {
                #region loop userPets
                userPets[i] = new Pet();

                userPets[i].PET_TYPE_CODE = petsModel[i].petType.Equals("cat") ? 2 : 1;

                userPets[i].PET_NAME = petsModel[i].petName;
                var photoMediaId = 0;

                if (photoId.TryGetValue(i, out photoMediaId))
                {
                    userPets[i].PET_PHOTO_URL = photoMediaId.ToString();
                }
                userPets[i].PET_GENDER_CODE = int.Parse(petsModel[i].petGender);

                userPets[i].PET_BIRTHDATE = DateTime.Parse(petsModel[i].petBirthDate, new CultureInfo(cultureInf));
                if (petsModel[i].PetBreedId != 0)
                {
                    userPets[i].PET_BREED_CODE = petsModel[i].PetBreedId;
                }
                else
                {
                    userPets[i].PET_BREED_CODE = 4259;
                }

                userPets[i].PET_NEUTERING_STATUS_CODE = petsModel[i].isPetSterilized.Equals("Yes") ? 1 : 2;

                userPets[i].PET_ALONE_TIME_CODE = petsModel[i].petAloneSitter;

                if (petsModel[i].isProductTriedBefore != null)
                {
                    userPets[i].PET_TRIED_LIGHT_FOOD = petsModel[i].isProductTriedBefore.Equals("Yes") ? 1 : 2;
                }
                else
                {
                    userPets[i].PET_TRIED_LIGHT_FOOD = 0;
                }
                userPets[i].PET_FOOD_TYPE_CODE = petsModel[i].PetFood;

                if (string.Compare(petsModel[i].petType, "cat", StringComparison.Ordinal) == 0)
                {
                    countCat++;
                    int accomodationCode = 0;
                    string Outdoors = LocalizationHelper.GetDictionaryItem("Registration - Pet - Form - Outdoors - Text");
                    string Indoors = LocalizationHelper.GetDictionaryItem("Registration - Pet - Form - Indoors - Text");
                    string Both= LocalizationHelper.GetDictionaryItem("Registration - Pet - Form - Both - Text");
                    if (petsModel[i].petSpendTime== "1")
                    {
                        accomodationCode = 1;
                    }
                    else if(petsModel[i].petSpendTime== "3")
                    {
                        accomodationCode = 3;
                    }
                    else if(petsModel[i].petSpendTime == "2")
                    {
                        accomodationCode = 2;
                    }

                    userPets[i].PET_ACCOMODATION_CODE = accomodationCode;

                    userPets[i].PET_COAT_LENGTH = SiteParameters.getPetCoatFormToCrm(petsModel[i].petCoat);
                }
                else
                {
                    countDog++;
                    userPets[i].PET_SIZE_CODE = SiteParameters.getPetWeightFormToCrm(petsModel[i].petWeigh);
                    userPets[i].PET_ACTIVITY_LEVEL = petsModel[i].petActive;
                }
                #endregion
            }
            user.profile = new Profile
            {
                NUMBER_OF_CATS = countCat,
                NUMBER_OF_DOGS = countDog
            };
            user.pets = userPets;
            var clientIP = GetClientIP();
            var userBack = oodsService.Register(user, profileSettingsRepository.GetCellId(cultureBydom.ToString()), model.Email, model.Password, false, clientIP);

            if (userBack.mtsError != null)
            {
                //if Login is exist
                if (userBack.mtsError.Code == 1006)
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(Session["culture"].ToString());

                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(Session["culture"].ToString());

                    UmbracoHelper helper = new UmbracoHelper(UmbracoContext.Current);

                    TempData["ErrorDuplicate"] = LocalizationHelper.GetDictionaryItem("Registration - Profile - Error - Email - Duplicate");

                    var cultureRender = Session["culture"].ToString();

                    var urlRedirect = registrationProfileRepository.GetRegistrationProfileUrlAlias(cultureRender);

                    return Json(new { url = "/" + urlRedirect });
                }
            }


            //send email confirmation
            var returnUrl = (string)Session["returnUrl"];
            string returnUrlToken = "";
            if (!string.IsNullOrEmpty(returnUrl))
            {
                returnUrlToken = ";" + returnUrl + "&param=return";
            }
            string token = EncryptionHelper.CreateToken(userBack.consumer.CONSUMER_ID.ToString() + returnUrlToken, model.Email, DateTime.UtcNow.AddDays(3));

            string scenario = "regular-registration";

            if (Session["scenario_name"] != null)
            {
                if (Session["scenario_name"].Equals("couponing"))
                {

                    scenario = "couponing";

                }
                else if (Session["scenario_name"].Equals("mbg"))
                {
                    scenario = "mbg";

                }
                else if (Session["scenario_name"].Equals("ratings_reviews"))
                {
                    scenario = "ratingsNreviews";
                }
                else if (Session["scenario_name"].Equals("sample"))
                {
                    scenario = "sample";
                }
                Session["scenario_name"] = null;
            }

            var utmParam = "utm_medium=crm-email&utm_source=crm&utm_term=confirm-cta&utm_content=pending-registration&utm_campaign=registration-process";


            EmailTemplate confirmationTemplate = emailRepository.GetEmailTemplate("Confirmation", Session["culture"].ToString());

            oodsService.SendMSEmail(model.Email, Convert.ToString(confirmationTemplate.EmailId), userBack.consumer.CONSUMER_ID, new string[] { string.Concat("Link_URL=",Helpers.UrlHelper.GetBaseUrl() + "/umbraco/Surface/RegistrationPets/Validate", "?token=", token,"&scenario=",scenario,"&",utmParam), string.Concat("Confirm_CTA=", Helpers.UrlHelper.GetBaseUrl() + "/umbraco/Surface/RegistrationPets/Validate", "?token=", token,"&scenario=",scenario,"&",utmParam),
            string.Concat("Subject=",confirmationTemplate.Subject),string.Concat("Name_URL=",Helpers.UrlHelper.GetBaseUrl()) });
            var culture = Session["culture"].ToString();
            var url = registrationConfirmationRepository.GetRegistrationConfirmationUrlAlias(culture);

            //var utmWelcomParam = "utm_medium=crm-email&utm_source=crm&utm_term=find-your-perfect-fit-cta&utm_content=welcome-email&utm_campaign=registration-process";

            //pending tracking
            if (Session["coupon_event_label"] != null)
            {
                Session["registrationpendingLabel"] = Session["coupon_event_label"];
            }
            Session["registrationpendingLabel"] = "regular-registration";

            Session["coupon_event_for_confirmation"] = Session["coupon_event_label"];
            Session["registrationpendingLabel_for_confirmation"] = Session["registrationpendingLabel"];

            //sample tracking
            if (Session["sample_event_label"] != null)
            {
                Session["registrationpendingLabel"] = Session["sample_event_label"];
            }

            if (Session["mbg_event_label"] != null)
            {
                Session["registrationpendingLabel"] = Session["mbg_event_label"];
            }

            //
            return Json(new { url = "/" + url });
        }

        public ActionResult CreateNewValidationEmail(int userId)
        {
            User user = oodsService.GetUser(userId);
            string token = EncryptionHelper.CreateToken(user.consumer.CONSUMER_ID.ToString(), user.consumer.EMAIL_ADDRESS, DateTime.UtcNow.AddDays(3));

            //scenario for tracking information



            oodsService.SendMSEmail(user.consumer.EMAIL_ADDRESS, EmailId, user.consumer.CONSUMER_ID, new string[] { string.Concat("Confirm_CTA=", Helpers.UrlHelper.GetBaseUrl() + "/umbraco/Surface/RegistrationPets/Validate", "?token=", token),
                string.Concat("Subject=",LocalizationHelper.GetDictionaryItem("Registration-profile-email-confirmation-subject")) });
            var culture = Session["culture"].ToString();
            var url = registrationConfirmationRepository.GetRegistrationConfirmationUrlAlias(culture);

            return Redirect("/" + url);
        }
        public ActionResult Validate(string token, string scenario)
        {

            var urlCult = Request.Url.ToString();
            var protocolCult = urlCult.Split('/').First();
            var domainCult = urlCult.Split('/')[2];
            var cultureBydom = loginRepository.GetCultureByDomain(string.Concat(protocolCult, "//", domainCult));

            var cultureFromUrl = cultureBydom;

            EmailTemplate welcomeTemplate = emailRepository.GetEmailTemplate("Welcome", cultureFromUrl.ToString());
            var emailId = "157646 ";
            token = token.Replace(" ", "+");
            string decrypted = null;
            try
            {
                decrypted = EncryptionHelper.Decrypt(token);
            }
            catch (Exception ex)
            {
                TempData["success"] = LocalizationHelper.GetDictionaryItem("Registration - profile - error");
                TempData["Confirmsuccess"] = LocalizationHelper.GetDictionaryItem("Registration - profile - validation - token - wrong");
                //var loginUrl = loginRepository.GetLoginUrlAlias(Thread.CurrentThread.CurrentCulture.ToString());
                var url = Request.Url.ToString();
                var protocol = url.Split('/').First();
                var domain = url.Split('/')[2];
                var loginUrl = loginRepository.GetLoginUrlAliasbyDomain(string.Concat(protocol, "//", domain));


                return Redirect(string.Concat("/", loginUrl));
            }

            if ((EncryptionHelper.IsTokenExpired(Convert.ToDateTime(EncryptionHelper.Decrypt(token).Split(';').Last())) && int.Parse(EncryptionHelper.Decrypt(token).Split(';').First()) != 0))
            {
                TempData["success"] = LocalizationHelper.GetDictionaryItem("Registration - profile - error");
                TempData["Confirmsuccess"] = LocalizationHelper.GetDictionaryItem("Registration - profile - validation-token - Expired") + "<a href=\"/umbraco/Surface/RegistrationPets/CreateNewValidationEmail?userId=" + int.Parse(EncryptionHelper.Decrypt(token).Split(';').First()) + "\">" + LocalizationHelper.GetDictionaryItem("Registration - profile - validation-token - Expired-newValidation-Email") + "</a>";
                var url = Request.Url.ToString();
                var protocol = url.Split('/').First();
                var domain = url.Split('/')[2];
                var loginUrl = loginRepository.GetLoginUrlAliasbyDomain(string.Concat(protocol, "//", domain));
                return Redirect(string.Concat("/", loginUrl));
            }


            // var decrypted = EncryptionHelper.Decrypt(token);
            var userId = int.Parse(decrypted.Split(';').First());
            if (!userId.Equals(0))
            {
                User user = oodsService.GetUser(userId);
                if (!user.login.Active)
                {
                    oodsService.SetActivation(profileSettingsRepository.GetCellId(cultureBydom.ToString()), user.consumer.EMAIL_ADDRESS, true, GetClientIP());
                    TempData["success"] = LocalizationHelper.GetDictionaryItem("Registration - profile - Success");
                    TempData["Confirmsuccess"] = LocalizationHelper.GetDictionaryItem("Registration-profile-email-welcome-subject");

                    var utmParam = "?utm_medium=crm-email&utm_source=crm&utm_term=philosophy-cta&utm_content=welcome-email&utm_campaign=registration-process#ourphilosophy";
                    var utmParamCtaProduct = "?utm_medium=crm-email&utm_source=crm&utm_term=philosophy-cta&utm_content=welcome-email&utm_campaign=registration-process";

                    var utmParamUpdate = "?utm_medium=crm-email&utm_source=crm&utm_term=update-your-profile-cta&utm_content=welcome-email&utm_campaign=registration-process";
                    var WelcomeEmailSubj = LocalizationHelper.GetDictionaryItem("Registration - Profile - Email - welcome");

                    //call Login repository
                    var loginUrlAlias = loginRepository.GetLoginUrlAlias(cultureFromUrl.ToString());
                    var ctaDogParamUrl = "&filter=on&category=dog";
                    var ctaCatParamUrl = "&filter=on&category=cat";

                    oodsService.SendMSEmail(user.consumer.EMAIL_ADDRESS, Convert.ToString(welcomeTemplate.EmailId), user.consumer.CONSUMER_ID, new string[] { string.Concat("Cat_CTA=", Helpers.UrlHelper.GetBaseUrl()+utmParamCtaProduct+ctaCatParamUrl),
                    string.Concat("Subject=",welcomeTemplate.Subject),string.Concat("Unsubscribe_URL=",""),string.Concat("Profile_URL=",Helpers.UrlHelper.GetBaseUrl(),"/"+loginUrlAlias.ToString()+utmParamUpdate),string.Concat("Dog_CTA=", Helpers.UrlHelper.GetBaseUrl()+utmParamCtaProduct+ctaDogParamUrl),string.Concat("Feed_CTA=", Helpers.UrlHelper.GetBaseUrl()+utmParam),string.Concat("Play_CTA=", Helpers.UrlHelper.GetBaseUrl()+utmParam),string.Concat("Move_CTA=", Helpers.UrlHelper.GetBaseUrl()+utmParam) });
                    var url = Request.Url.ToString();
                    var protocol = url.Split('/').First();
                    var domain = url.Split('/')[2];

                    var loginUrl = loginRepository.GetLoginUrlAliasbyDomain(string.Concat(protocol, "//", domain));

                    var returnUrl = decrypted.Split(';')[1];
                    if (returnUrl.Contains("return"))
                    {
                        string utmPending = "?utm_medium=crm-email&utm_source=crm&utm_term=confirm-cta&utm_content=pending-registration&utm_campaign=registration-process";

                        Session["registration_event_completed"] = scenario;
                        return Redirect(string.Concat("/", loginUrl + "?returnUrl=" + returnUrl + "&scenario=" + scenario + utmPending));
                        //return Redirect(string.Concat(string.Concat(protocol, "//", domain), "/", returnUrl));
                    }


                    //Data tracking : event login
                    string utm = "?utm_medium=crm-email&utm_source=crm&utm_term=confirm-cta&utm_content=pending-registration&utm_campaign=registration-process";

                    if (string.IsNullOrEmpty(scenario))
                    {
                        return Redirect(string.Concat("/", loginUrl + utm));
                    }
                    string utm2 = "&utm_medium=crm-email&utm_source=crm&utm_term=confirm-cta&utm_content=pending-registration&utm_campaign=registration-process";

                    Session["registration_event_completed"] = scenario;
                    return Redirect(string.Concat("/", loginUrl + "?scenario=" + scenario + utm2));
                }
                else
                {

                    TempData["success"] = LocalizationHelper.GetDictionaryItem("Registration - profile - error");
                    TempData["Confirmsuccess"] = LocalizationHelper.GetDictionaryItem("Registration-profile-email-confirmation-login-already-activated");
                    var url = Request.Url.ToString();
                    var protocol = url.Split('/').First();
                    var domain = url.Split('/')[2];
                    var loginUrl = loginRepository.GetLoginUrlAliasbyDomain(string.Concat(protocol, "//", domain));
                    return Redirect(string.Concat("/", loginUrl));
                }

            }
            else
            {
                TempData["success"] = LocalizationHelper.GetDictionaryItem("Registration - profile - error");
                // TempData["Confirmsuccess"] = "WELCOME TO PERFECT FIT™";
                var url = Request.Url.ToString();
                var protocol = url.Split('/').First();
                var domain = url.Split('/')[2];
                var loginUrl = loginRepository.GetLoginUrlAliasbyDomain(string.Concat(protocol, "//", domain));
                return Redirect(string.Concat("/", loginUrl));
            }
        }

        public ActionResult DeactivateAccount()
        {
            if (Authentication.isLogin())
            {
                var user = oodsService.GetUser(int.Parse(Authentication.GetConsumerId()));

                var culture = Thread.CurrentThread.CurrentUICulture;
                if (Session["culture"] != null) culture = (CultureInfo)Session["culture"];

                MtsError response = oodsService.SetActivation(SiteParameters.GetCellId(culture.ToString()), user.consumer.EMAIL_ADDRESS, false, GetClientIP());

                Authentication.logOut();

                return Redirect("/");
            }

            return Redirect("/");
        }

        public ActionResult GetBreeds(string specie, string pageId)
        {
            var currentPage = helperUmbraco.TypedContent(pageId);

            Thread.CurrentThread.CurrentCulture = currentPage.GetCulture();
            Thread.CurrentThread.CurrentUICulture = currentPage.GetCulture();

            IEnumerable<PetBreed> breeds = null;

            if (specie.Equals("cat", StringComparison.InvariantCultureIgnoreCase))
            {
                breeds = registrationPetsRepository.GetRegistrationPetsBreeds(currentPage.GetCulture().Name, "catBreedContainer");
            }
            else if (specie.Equals("dog", StringComparison.InvariantCultureIgnoreCase))
            {
                breeds = registrationPetsRepository.GetRegistrationPetsBreeds(currentPage.GetCulture().Name, "dogBreedContainer");
            }

            return Json(breeds, JsonRequestBehavior.AllowGet);
        }



        public ActionResult UpdatePetProfile()
        {
            var urlCult = Request.Url.ToString();
            var protocolCult = urlCult.Split('/').First();
            var domainCult = urlCult.Split('/')[2];
            var cultureBydom = loginRepository.GetCultureByDomain(string.Concat(protocolCult, "//", domainCult));

            if (!Authentication.isLogin())
            {
                return Redirect("/registration");
            }

            var cultureDateInput = "fr-FR";

            var forms = Request.Form[0];

            var dataroot = Newtonsoft.Json.JsonConvert.DeserializeObject<DataRoot>(forms);

            var data = dataroot.data;

            var petsModel = data.petsModel;

            var photoMediaId = 0;

            if (Request.Files.Count > 0)
            {
                try
                {
                    //Get all files from Request object
                    var files = Request.Files;
                    var file = files[0];

                    string fname;
                    //checking for Internet Explorer
                    if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                    {
                        var testfiles = file.FileName.Split('\\');
                        fname = testfiles[testfiles.Length - 1];
                    }
                    else
                    {
                        fname = file.FileName;
                    }
                    fname = Path.Combine(Server.MapPath("~/App_Data/TEMP/"), fname);
                    file.SaveAs(fname);
                    var ms = Services.MediaService;
                    var folder = MediaHelper.GetOrCreateMediaFolders(ms, cultureBydom.ToString(), "Account-Pet Images");
                    var mediaMap = ms.CreateMedia(fname, folder, "Image");
                    mediaMap.SetValue("umbracoFile", file);
                    mediaMap.SetValue("altText", file.FileName);
                    mediaMap.Name = file.FileName;

                    var photoNameIndex = files.AllKeys[0];
                    var photoIndex = int.Parse(files.AllKeys[0].Substring(photoNameIndex.Length - 1));
                    ms.Save(mediaMap);
                    photoMediaId = mediaMap.Id;
                    System.IO.File.Delete(fname);
                }
                catch (Exception ex)
                {
                    return Json(ex.Message);
                }
            }

            Pet petToUpdate = oodsService.GetPet(petsModel[0].petId);

            petToUpdate.PET_TYPE_CODE = petsModel[0].petType.Equals("cat") ? 2 : 1;
            petToUpdate.PET_NAME = petsModel[0].petName;

            if (Request.Files.Count > 0)
            {
                petToUpdate.PET_PHOTO_URL = photoMediaId.ToString();
            }

            petToUpdate.PET_GENDER_CODE = int.Parse(petsModel[0].petGender);

            petToUpdate.PET_BIRTHDATE = DateTime.Parse(petsModel[0].petBirthDate, new CultureInfo(cultureDateInput));
            if (petsModel[0].PetBreedId != 0)
            {
                petToUpdate.PET_BREED_CODE = petsModel[0].PetBreedId;
            }
            else
            {
                petToUpdate.PET_BREED_CODE = 4259;
            }
            petToUpdate.PET_NEUTERING_STATUS_CODE = petsModel[0].isPetSterilized.Equals("Yes") ? 1 : 2;
            petToUpdate.PET_ALONE_TIME_CODE = petsModel[0].petAloneSitter;
            if (petsModel[0].isProductTriedBefore != null)
            {
                petToUpdate.PET_TRIED_LIGHT_FOOD = petsModel[0].isProductTriedBefore.Equals("Yes") ? 1 : 2;
            }
            else
            {
                petToUpdate.PET_TRIED_LIGHT_FOOD = 0;
            }
            petToUpdate.PET_FOOD_TYPE_CODE = petsModel[0].PetFood;

            if (string.Compare(petsModel[0].petType, "cat", StringComparison.Ordinal) == 0)
            {
                int accomodationCode = 0;
                string Outdoors = LocalizationHelper.GetDictionaryItem("Registration - Pet - Form - Outdoors - Text");
                string Indoors = LocalizationHelper.GetDictionaryItem("Registration - Pet - Form - Indoors - Text");
                string Both = LocalizationHelper.GetDictionaryItem("Registration - Pet - Form - Both - Text");
                if (petsModel[0].petSpendTime == "1")
                {
                    accomodationCode = 1;
                }
                else if (petsModel[0].petSpendTime == "3")
                {
                    accomodationCode = 3;
                }
                else if (petsModel[0].petSpendTime == "2")
                {
                    accomodationCode = 2;
                }
                petToUpdate.PET_ACCOMODATION_CODE = accomodationCode;

                petToUpdate.PET_COAT_LENGTH = SiteParameters.getPetCoatFormToCrm(petsModel[0].petCoat);
            }
            else
            {
                petToUpdate.PET_SIZE_CODE = SiteParameters.getPetWeightFormToCrm(petsModel[0].petWeigh);
                petToUpdate.PET_ACTIVITY_LEVEL = petsModel[0].petActive;
            }
            var clientIp = GetClientIP();
            var culture = Thread.CurrentThread.CurrentUICulture;
            if (Session["culture"] != null) culture = (CultureInfo)Session["culture"];
            MtsError response = oodsService.UpdatePet(petToUpdate, SiteParameters.GetCellId(culture.ToString()), clientIp);

            if (response != null && response.Code == 1000)
            {
                var user_profile = "";
                User user = oodsService.GetUser(int.Parse(Authentication.GetConsumerId()));
                var pets = user.pets;
                int catNumber = 0;
                int dogNumber = 0;

                foreach (var pet in pets)
                {
                    switch (pet.PET_TYPE_CODE)
                    {
                        case 1:
                            dogNumber++;
                            break;
                        case 2:
                            catNumber++;
                            break;
                    }
                }
                if (catNumber != 0 && dogNumber != 0)
                {
                    user_profile = "Both";
                }
                else if (catNumber != 0 && dogNumber == 0)
                {
                    user_profile = "Cat";
                }
                else if (catNumber == 0 && dogNumber != 0)
                {
                    user_profile = "Dog";
                }

                dynamic datalayer = new ExpandoObject();
                datalayer.MetaDataUserProfile = user_profile;
                var layer = this.GenerateCustomVariable(datalayer);
                Session["user_profile"] = layer;

                Session["user_profile_dl"] = user_profile;
                return Json(new { status = "updated" });
            };

            return Json(new { status = "error" });
        }

        public ActionResult AddPetProfile()
        {
            if (!Authentication.isLogin())
            {
                return Redirect("/registration");
            }

            var cultureDateInput = "fr-FR";

            var forms = Request.Form[0];

            var dataroot = Newtonsoft.Json.JsonConvert.DeserializeObject<DataRoot>(forms);

            var data = dataroot.data;

            var petsModel = data.petsModel;

            var photoMediaId = 0;

            if (Request.Files.Count > 0)
            {
                try
                {
                    //Get all files from Request object
                    var files = Request.Files;
                    var file = files[0];

                    string fname;
                    //checking for Internet Explorer
                    if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                    {
                        var testfiles = file.FileName.Split('\\');
                        fname = testfiles[testfiles.Length - 1];
                    }
                    else
                    {
                        fname = file.FileName;
                    }
                    fname = Path.Combine(Server.MapPath("~/App_Data/TEMP/"), fname);
                    file.SaveAs(fname);
                    var ms = Services.MediaService;
                    var mediaMap = ms.CreateMedia(fname, -1, "Image");
                    mediaMap.SetValue("umbracoFile", file);
                    mediaMap.SetValue("altText", file.FileName);
                    mediaMap.Name = file.FileName;

                    var photoNameIndex = files.AllKeys[0];
                    var photoIndex = int.Parse(files.AllKeys[0].Substring(photoNameIndex.Length - 1));
                    ms.Save(mediaMap);
                    photoMediaId = mediaMap.Id;
                    System.IO.File.Delete(fname);

                    //update DL for pet

                    //
                }
                catch (Exception ex)
                {
                    return Json(ex.Message);
                }
            }

            Pet petToAdd = new Pet();

            petToAdd.PET_TYPE_CODE = petsModel[0].petType.Equals("cat") ? 2 : 1;
            petToAdd.PET_NAME = petsModel[0].petName;
            petToAdd.PET_PHOTO_URL = photoMediaId.ToString();
            petToAdd.PET_GENDER_CODE = int.Parse(petsModel[0].petGender);

            petToAdd.PET_BIRTHDATE = DateTime.Parse(petsModel[0].petBirthDate, new CultureInfo(cultureDateInput));
            if (petsModel[0].PetBreedId != 0)
            {
                petToAdd.PET_BREED_CODE = petsModel[0].PetBreedId;
            }
            else
            {
                petToAdd.PET_BREED_CODE = 4259;
            }
            petToAdd.PET_NEUTERING_STATUS_CODE = petsModel[0].isPetSterilized.Equals("Yes") ? 1 : 2;
            petToAdd.PET_ALONE_TIME_CODE = petsModel[0].petAloneSitter;
            if (petsModel[0].isProductTriedBefore != null)
            {
                petToAdd.PET_TRIED_LIGHT_FOOD = petsModel[0].isProductTriedBefore.Equals("Yes") ? 1 : 2;
            }
            else
            {
                petToAdd.PET_TRIED_LIGHT_FOOD = 0;
            }
            petToAdd.PET_FOOD_TYPE_CODE = petsModel[0].PetFood;
            petToAdd.CONSUMER_ID = int.Parse(Authentication.GetConsumerId());

            if (string.Compare(petsModel[0].petType, "cat", StringComparison.Ordinal) == 0)
            {
                int accomodationCode = 0;
                string Outdoors = LocalizationHelper.GetDictionaryItem("Registration - Pet - Form - Outdoors - Text");
                string Indoors = LocalizationHelper.GetDictionaryItem("Registration - Pet - Form - Indoors - Text");
                string Both = LocalizationHelper.GetDictionaryItem("Registration - Pet - Form - Both - Text");
                if (petsModel[0].petSpendTime == "1")
                {
                    accomodationCode = 1;
                }
                else if (petsModel[0].petSpendTime == "3")
                {
                    accomodationCode = 3;
                }
                else if (petsModel[0].petSpendTime == "2")
                {
                    accomodationCode = 2;
                }
                petToAdd.PET_ACCOMODATION_CODE = accomodationCode;

                petToAdd.PET_COAT_LENGTH = SiteParameters.getPetCoatFormToCrm(petsModel[0].petCoat);
            }
            else
            {
                petToAdd.PET_SIZE_CODE = SiteParameters.getPetWeightFormToCrm(petsModel[0].petWeigh);
                petToAdd.PET_ACTIVITY_LEVEL = petsModel[0].petActive;
            }
            User userToUpdate = oodsService.GetUser(int.Parse(Authentication.GetConsumerId()));

            userToUpdate.consumer.LAST_MODIFICATION_DATE = DateTime.Now;

            var culture = Thread.CurrentThread.CurrentUICulture;
            if (Session["culture"] != null) culture = (CultureInfo)Session["culture"];
            userToUpdate.consumer.CAMPAIGN_RESPONSE_CODE = SiteParameters.GetCellId(culture.ToString());

            List<Pet> userPets = userToUpdate.pets.ToList();
            userPets.Add(petToAdd);
            userToUpdate.pets = userPets.ToArray();
            MtsError response = oodsService.UpdateUser(userToUpdate, GetClientIP());


            //update DL -
            var user_profile = "";
            User user = oodsService.GetUser(int.Parse(Authentication.GetConsumerId()));
            var pets = user.pets;
            int catNumber = 0;
            int dogNumber = 0;

            foreach (var pet in pets)
            {
                switch (pet.PET_TYPE_CODE)
                {
                    case 1:
                        dogNumber++;
                        break;
                    case 2:
                        catNumber++;
                        break;
                }
            }
            if (catNumber != 0 && dogNumber != 0)
            {
                user_profile = "Both";
            }
            else if (catNumber != 0 && dogNumber == 0)
            {
                user_profile = "Cat";
            }
            else if (catNumber == 0 && dogNumber != 0)
            {
                user_profile = "Dog";
            }

            dynamic datalayer = new ExpandoObject();
            datalayer.MetaDataUserProfile = user_profile;
            var layer = this.GenerateCustomVariable(datalayer);
            Session["user_profile"] = layer;

            Session["user_profile_dl"] = user_profile;

            //

            return Json(new { status = "updated" });
        }

        public ActionResult DeletePet(string id)
        {

            var urlCult = Request.Url.ToString();
            var protocolCult = urlCult.Split('/').First();
            var domainCult = urlCult.Split('/')[2];
            var cultureBydom = loginRepository.GetCultureByDomain(string.Concat(protocolCult, "//", domainCult));

            string urlRedirectProfile = registrationProfileRepository.GetEditProfileUrlAlias(cultureBydom.ToString());


            if (!Authentication.isLogin())
            {
                return Redirect("/registration");
            }
            if (string.IsNullOrEmpty(id))
            {
                return Redirect(urlRedirectProfile);
            }

            User user = oodsService.GetUser(int.Parse(Authentication.GetConsumerId()));
            if (user.pets.Length < 2)
            {
                return Redirect("/" + urlRedirectProfile);
            }


            var culture = Thread.CurrentThread.CurrentUICulture;

            if (Session["culture"] != null) culture = (CultureInfo)Session["culture"];

            Pet petToDelete = oodsService.GetPet(int.Parse(id));
            petToDelete.CONSUMER_ID = 0;
            var clientIp = GetClientIP();
            MtsError response = oodsService.UpdatePet(petToDelete, SiteParameters.GetCellId(culture.ToString()), clientIp);

            //update DL -
            var user_profile = "";
            User userDl = oodsService.GetUser(int.Parse(Authentication.GetConsumerId()));
            var pets = userDl.pets;
            int catNumber = 0;
            int dogNumber = 0;

            foreach (var pet in pets)
            {
                switch (pet.PET_TYPE_CODE)
                {
                    case 1:
                        dogNumber++;
                        break;
                    case 2:
                        catNumber++;
                        break;
                }
            }
            if (catNumber != 0 && dogNumber != 0)
            {
                user_profile = "Both";
            }
            else if (catNumber != 0 && dogNumber == 0)
            {
                user_profile = "Cat";
            }
            else if (catNumber == 0 && dogNumber != 0)
            {
                user_profile = "Dog";
            }

            dynamic datalayer = new ExpandoObject();
            datalayer.MetaDataUserProfile = user_profile;
            var layer = this.GenerateCustomVariable(datalayer);
            Session["user_profile"] = layer;

            Session["user_profile_dl"] = user_profile;
            //
            var urlReturn = registrationProfileRepository.GetEditProfileUrlAlias(culture.ToString());

            return Redirect("/" + urlReturn);
        }

        #endregion
    }
}
