namespace Mars.PerfectFit.Presentation.Web.Controllers
{
    using System;
    using System.Web.Mvc;
    using Mars.PerfectFit.Core.Domain.Models.Registration;
    using Mars.PerfectFit.Core.Domain.Services;
    using Mars.PerfectFit.Infrastructure.CrmService.Services;
    using Mars.PerfectFit.Presentation.Web.Helpers;
    using Mars.PerfectFit.Presentation.Web.Security;
    using Mars.PerfectFit.Presentation.Web.ViewModels;
    using Umbraco.Web;
    using System.Threading;
    using System.Dynamic;
    using System.Globalization;
    using Core.Utilities;
    using Core.Domain.Repositories;
    using System.Linq;

    public class LoginController : BaseController
    {
        #region Fields

        private readonly OodsServices accountService;
        private readonly IProfileSettingsRepository profileSettingsRepository;
        private readonly IRegistrationProfileRepository profileRepository;

        #endregion

        #region Constructors & Destructors

        public LoginController(IRegistrationProfileRepository profileRepositoryParam,IProfileSettingsRepository profileSettingsRepositorParam,IConfigService configService, ICommonService commonService, OodsServices serv) : base(configService, commonService)
        {
            accountService = serv;
            profileSettingsRepository = profileSettingsRepositorParam;
            profileRepository = profileRepositoryParam;


        }

        #endregion

        
        // GET: Login
        public ActionResult Login(string returnUrl="")
        {

            var culture = CurrentPage.GetCulture();

            if(Authentication.isLogin())
            {
               return Redirect (profileRepository.GetEditProfileUrlAlias(culture.ToString()));
            }

            
            var bannerId = CurrentPage.HasProperty("banner") ? CurrentPage.GetPropertyValue<int>("banner") : 0;

            var media = Umbraco.TypedMedia(bannerId);

            var model = new LoginViewModel
            {
                Banner = ImageHelper.GetImage(media)
            };

            if (!string.IsNullOrEmpty(returnUrl))
            {
                ViewBag.ReturnUrl = returnUrl;
            }
            
            var tag = string.Empty;

           
                if (Session["registration_event_completed"] != null)
                {

                    dynamic datalayer = new ExpandoObject();
                    datalayer.@event = "event_crm_actions";
                    datalayer.event_category = "event_crm_actions";
                    datalayer.event_action = "event_registration_complete";
                    datalayer.event_label = Session["registration_event_completed"];

                    string lowerDatalayer = this.GenerateCustomVariable(datalayer);
                    ViewBag.DataLayer = lowerDatalayer;
                Session["registration_event_completed"] = null;
                }
            

            dynamic datalayerLogin = new ExpandoObject();

                datalayerLogin.@event = "event_crm_actions";
                datalayerLogin.event_category = "event_crm_actions";
                datalayerLogin.event_action = "event_login_view_form";
                datalayerLogin.event_label = "login_page";
               
                string lowerDatalayerLogin = this.GenerateCustomVariable(datalayerLogin);
                ViewBag.DataLayerLogin = lowerDatalayerLogin;


                dynamic datalayerSiteMap = new ExpandoObject();

                datalayerSiteMap.MetaDataSiteMap = "Login";

                ViewBag.MetaDataSiteMap = this.GenerateCustomVariable(datalayerSiteMap);
                
            Session["registrationpendingLabel_for_confirmation"] = null;
            Session["coupon_event_for_confirmation"] = null;
            ViewBag.RegsitrationUrl = profileRepository.GetRegistrationProfileUrlAlias(culture.ToString());

            return View(model);
        }

        private string GetClientIP()
        {
            return (Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ??
                    Request.ServerVariables["REMOTE_ADDR"]).Split(',')[0].Trim();
        }

        [HttpPost]
        public ActionResult ValidationLogin(LoginViewModel model, string returnUrl,string culture)
        {
            //used for DataLayer : tracking

            var user_profile = "";

            var result = new MessageResult();
            var decodedUrl = "/";

            if (!ModelState.IsValid)
            {
                if (!Request.IsAjaxRequest())
                {
                    return CurrentUmbracoPage();
                }

                return Json(ModelState);
            }

            try
            {
                // Session["User"] = user;
                
                Session["culture"] = Thread.CurrentThread.CurrentCulture;
                if (string.IsNullOrEmpty(culture)){
                    var cultureInfo = CurrentPage.GetCulture();
                    culture = cultureInfo.ToString();
                }

                var user = accountService.Connect(model.Username,model.Password, profileSettingsRepository.GetCellId(culture), GetClientIP());

                if (user.login.username != null && user.login.Active)
                {
                    
                    var login = new Core.Domain.Models.Registration.Login
                    {
                        Username = user.login.username,
                        Password = user.login.password,
                        SiteID = user.login.Site_ID,
                        ConsumerID = user.consumer.CONSUMER_ID
                    };

                    //DL --- for login user
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
                    if(catNumber !=0 && dogNumber != 0){
                        user_profile = "Both";
                    } else if(catNumber!=0 && dogNumber == 0)
                    {
                        user_profile = "Cat";
                    }else if(catNumber==0 && dogNumber != 0)
                    {
                        user_profile = "Dog";
                    }

                    dynamic datalayer = new ExpandoObject();
                    datalayer.MetaDataUserProfile = user_profile;
                    var layer = this.GenerateCustomVariable(datalayer);
                    Session["user_profile"] = layer;
                    Session["user_profile_dl"] = user_profile;
                    // ---

                    var consumer = new Consumer
                    {
                        Firstname = user.consumer.FIRSTNAME,
                        Surname = user.consumer.SURNAME,
                        ConsumerId = user.consumer.CONSUMER_ID.ToString()
                    };

                    var usr = new User
                    {
                        ConsumerField = consumer,
                        LoginField = login
                    };

                    Authentication.SetCookie(usr, 24, model.RememberMe);

                    ViewBag.UserName = Authentication.GetUserFirstName();
                    
             
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        decodedUrl = Server.UrlDecode(returnUrl);
                        Session["Login_complete_event"] = "event_login_complete";
                        Session["sample_complete_label"] = "event_sample_complete";
                        if (Request.IsAjaxRequest())
                        {
                            return Json(new {url = decodedUrl});
                        }
                        

                        return Redirect(decodedUrl);
                    }

                    if (Request.IsAjaxRequest())
                    {
                        Session["Login_complete_event"] = "event_login_complete";
                        if (!string.IsNullOrEmpty(model.UrlPageReturn))
                        {
                            return Json(new { url = model.UrlPageReturn });
                        }
                        
                        return Json(new {url = "/"});
                    }
                } else
                {

                    //var cultureInfo = CurrentPage.GetCulture();
                   
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(culture.ToString());

                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture.ToString());

                    UmbracoHelper helper = new UmbracoHelper(UmbracoContext.Current);

                    var localizedErrorCredencial = LocalizationHelper.GetDictionaryItem("Login - Error - User - Wrong - Credential");

                    if (Request.IsAjaxRequest())
                    {
                        if (string.IsNullOrEmpty(localizedErrorCredencial))
                        {
                            return Json(new { MessageError = "Internal Server Error : Missing Dictionary" });
                        }
                        return Json(new { MessageError = localizedErrorCredencial });
                    }
                    
                    TempData["errorCredential"] = LocalizationHelper.GetDictionaryItem("Login - Error - User - Wrong - Credential");

                    //TempData["Confirmsuccess"] = LocalizationHelper.GetDictionaryItem("Reset - Password - Confirm - Success - Message -");

                    return CurrentUmbracoPage();
                }
            }
            catch (Exception exc)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(culture.ToString());

                Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture.ToString());

                UmbracoHelper helper = new UmbracoHelper(UmbracoContext.Current);
                var localizedErrorCredencial = LocalizationHelper.GetDictionaryItem("Login - Error - User - Wrong - Credential");
                return Json(new { MessageError = localizedErrorCredencial });
            }
            
            //create DL for user profile

            Session["Login_complete_event"] = "event_login_complete";

            Session["sample_complete_label"] = "event_sample_complete";

            //user profile

            //


            return Redirect("/");
        }

      
    }
}
