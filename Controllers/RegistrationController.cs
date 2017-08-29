namespace Mars.PerfectFit.Presentation.Web.Controllers
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Web.Mvc;
    using Mars.PerfectFit.Core.Domain.Repositories;
    using Mars.PerfectFit.Core.Domain.Services;
    using Mars.PerfectFit.Infrastructure.CrmService.ServiceCRMReference;
    using Mars.PerfectFit.Infrastructure.CrmService.Services;
    using Mars.PerfectFit.Presentation.Web.Security;
    using Mars.PerfectFit.Presentation.Web.ViewModels;
    using Umbraco.Web;
    using Constents;
    using System.Dynamic;
    using System.Collections.Generic;
    using System.Linq;

    public class RegistrationController : BaseController
    {
        #region Fields

        private readonly IRegistrationPetsRepository registrationPetsRepository;
        private readonly IRegistrationProfileRepository registrationProfileRepository;
        private readonly UmbracoHelper umbracoHelper;
        private readonly ILoginRepository loginRepository;
        private readonly OodsServices oodsService;
       

        #endregion

        #region Constructors & Destructors

        public RegistrationController(ILoginRepository loginRepositoryParam, IConfigService configService, ICommonService commonService, OodsServices oodsService, IRegistrationProfileRepository registrationProfileRepository, IRegistrationPetsRepository registrationPetsRepository) : base(configService, commonService)
        {
            this.oodsService = oodsService;
            this.registrationProfileRepository = registrationProfileRepository;
            loginRepository = loginRepositoryParam;
            this.registrationPetsRepository = registrationPetsRepository;
            umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
        }

        #endregion

        #region Methods

        public ActionResult Registration(string returnUrl="")
        {

            var urlCult = Request.Url.ToString();
            var protocolCult = urlCult.Split('/').First();
            var domainCult = urlCult.Split('/')[2];
            var cultureBydom = loginRepository.GetCultureByDomain(string.Concat(protocolCult, "//", domainCult));
            var cultureFromUrl = cultureBydom;

            var isRegistered = umbracoHelper.GetDictionaryValue("Form - Registration - Registered");

            var culture = cultureFromUrl;

            if (Session["culture"] != null)
            {
                culture = cultureFromUrl.ToString();
            }
            else
            {
                Session["culture"] = CultureInfo.CreateSpecificCulture(cultureFromUrl);

            }
            var registrationProfile = registrationProfileRepository.GetRegistrationProfileXpath(culture.ToString());

            var model = new RegistrationProfileViewModel
            {
                Banner = registrationProfile.Banner,
                Title = registrationProfile.Title,
                Description = registrationProfile.Description,
               
            };

            ViewBag.isRegistered = isRegistered;

            if (!string.IsNullOrEmpty(returnUrl))
            {
                Session["returnUrl"] = returnUrl;
                ViewBag.ReturnUrl = returnUrl;
            }

            dynamic datalayer = new ExpandoObject();
            datalayer.MetaDataSiteMap = "Registration";
            ViewBag.DataLayer = this.GenerateCustomVariable(datalayer);

            return View(model);
        }

        public ActionResult GetVersionCRM()
        {
            var version = oodsService.GetVersion();
            return Json(version, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ActionName("MemberRegistration")]
        public ActionResult ProfileValidation(RegisterModel model)
        {
            var culture = Thread.CurrentThread.CurrentUICulture.ToString();
            var optPerfectFitBrandId = 278; //TO DO : to place in umbraco parameter

            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            var registerUser = new User
            {
                consumer = new Consumer
                {
                    CAMPAIGN_RESPONSE_DATE = DateTime.Now,
                    FIRSTNAME = model.FirstName,
                    SURNAME = model.LastName,
                    STREET_NAME = model.Address1,
                    STREET_ADDRESS = model.Address2,
                    POSTAL_CODE = model.PostalCode,
                    CITY = model.City,
                    EMAIL_ADDRESS = model.Email,
                    CREATION_DATE = DateTime.Now,
                    LAST_MODIFICATION_DATE = DateTime.Now,
                    FAVOURITE_LANGUAGE_CODE = culture.Split('-').Last(),
                    COUNTRY_CODE = culture.Split('-').Last()
                },
                login = new Login
                {
                    username = model.Email,
                    password = model.Password
                }
            };

            if (!string.IsNullOrEmpty(model.MyProfileQuestion))
            {
                registerUser.brandPreferences = new BrandPreference[]
                {
                    new BrandPreference
                    {
                        BP_BRAND_ID = optPerfectFitBrandId,
                        CELL_ID = SiteParameters.GetCellId(culture.ToString())
                    }
                };
            }

            //if (!culture.ToString().Equals("de_DE"))
            //{
            //    registerUser.marketingPermissions = new MarketingPermission[]
            //    {
            //        new MarketingPermission
            //        {
            //            OPT_IN_SOURCE_CODE = "OODS",
            //            OPT_IN_CHANNEL_CODE = 0,
            //            OPT_IN_TYPE_CODE = 1,
            //            OPT_IN_METHOD_CODE = 1,
            //            OPT_IN_BRAND_ID = optPerfectFitBrandId
            //        }
            //    };
            //}

            if (!string.IsNullOrEmpty(model.BirthDate))
            {
                registerUser.consumer.BIRTHDATE = DateTime.Parse(model.BirthDate);
            }

            Session["userModel"] = model;
            Session["userRegistration"] = registerUser;


            var urlAliasRegistrationPets = registrationPetsRepository.GetRegistrationPetsUrlAlias(System.Threading.Thread.CurrentThread.CurrentUICulture.ToString());

            TempData["isStep1"] = "step1";
            return Redirect("/"+urlAliasRegistrationPets);
        }

        [HttpPost]
        [ActionName("MemberUpdate")]
        public ActionResult EditProfile(RegisterModel model)
        {
            if (!Authentication.isLogin())
            {
                var registrationUrl = registrationProfileRepository.GetRegistrationProfileUrlAlias(System.Threading.Thread.CurrentThread.CurrentUICulture.ToString());
                return Redirect("/" + registrationUrl);
            }

            var culture = Thread.CurrentThread.CurrentUICulture;

            var optPerfectFitBrandId = 278; //TO DO : to place in umbraco parameter

            User usertoModifInit = oodsService.GetUser(int.Parse(Authentication.GetConsumerId()));

            var clientIp = (Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ??
                    Request.ServerVariables["REMOTE_ADDR"]).Split(',')[0].Trim();

            var cellId = SiteParameters.GetCellId(Thread.CurrentThread.CurrentUICulture.ToString());

            if (usertoModifInit.marketingPermissions != null && usertoModifInit.marketingPermissions.Length > 0)
            {
                for (int i = 0; i < usertoModifInit.marketingPermissions.Length; i++)
                {
                    usertoModifInit.marketingPermissions[i].CONSUMER_ID = 0;
                    oodsService.UpdateMarketingPermission(usertoModifInit.marketingPermissions[i], cellId, clientIp);
                }
            }

            if (usertoModifInit.brandPreferences != null && usertoModifInit.brandPreferences.Length > 0)
            {
                for (int i = 0; i < usertoModifInit.brandPreferences.Length; i++)
                {
                    usertoModifInit.brandPreferences[i].CONSUMER_ID = 0;
                    oodsService.UpdateBrandPreference(usertoModifInit.brandPreferences[i], cellId, clientIp);
                }
            }

            User usertoModif = oodsService.GetUser(int.Parse(Authentication.GetConsumerId()));

            usertoModif.consumer.FIRSTNAME = model.FirstName;
            usertoModif.consumer.SURNAME = model.LastName;
            usertoModif.consumer.STREET_NAME = model.Address1;
            usertoModif.consumer.STREET_ADDRESS = model.Address2;
            usertoModif.consumer.POSTAL_CODE = model.PostalCode;
            usertoModif.consumer.CITY = model.City;
            usertoModif.consumer.EMAIL_ADDRESS = model.Email;
            usertoModif.consumer.LAST_MODIFICATION_DATE = DateTime.Now;
            usertoModif.consumer.CAMPAIGN_RESPONSE_CODE = SiteParameters.GetCellId(Thread.CurrentThread.CurrentUICulture.ToString());

            if (!string.IsNullOrEmpty(model.BirthDate))
            {
                usertoModif.consumer.BIRTHDATE = DateTime.Parse(model.BirthDate);
            }



            if (!string.IsNullOrEmpty(model.MyProfileQuestion))
            {
                usertoModif.brandPreferences = new BrandPreference[]
                {
                    new BrandPreference
                    {
                        BP_BRAND_ID = optPerfectFitBrandId,
                        CELL_ID = SiteParameters.GetCellId(culture.ToString())
                    }
                };
            }

            if (!string.IsNullOrEmpty(model.UpdateMarsQuestion))
            {
                if (usertoModif.marketingPermissions != null && usertoModif.marketingPermissions.Length > 0)
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
                    userMP.Add(usertoModif.marketingPermissions[0]);
                    userMP.Add(newUserMP);
                    usertoModif.marketingPermissions = userMP.ToArray();
                }
                else
                {
                    usertoModif.marketingPermissions = new MarketingPermission[]
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


            

            MtsError response = oodsService.UpdateUser(usertoModif, clientIp);

            Session["NewFirstName"] = string.Concat(usertoModif.consumer.FIRSTNAME, " ", usertoModif.consumer.SURNAME);
            return CurrentUmbracoPage();
        }

        #endregion
    }
}
