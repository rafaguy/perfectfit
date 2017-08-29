namespace Mars.PerfectFit.Presentation.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using Mars.PerfectFit.Core.Domain;
    using Mars.PerfectFit.Core.Domain.Models.Registration;
    using Mars.PerfectFit.Core.Domain.Services;
    using Mars.PerfectFit.Core.Utilities;
    using Mars.PerfectFit.Infrastructure.CrmService.Services;
    using Mars.PerfectFit.Presentation.Web.Security;
    using Mars.PerfectFit.Presentation.Web.ViewModels;
    using Umbraco.Web;
    using Helpers;
    using System.Threading;
    using System.Globalization;
    using Core.Domain.Repositories;
    using Core.Domain.Models.Emailing;
    using System.Text;

    public class AccountController : BaseController
    {
        #region Fields

        private readonly OodsServices accountService;
        private readonly IRepository repository;
        private readonly UmbracoHelper umbracoHelper;
        private readonly IEmailRepository emailRepository;
        private readonly IProfileSettingsRepository profileSettingsRepository;
        private readonly IRegistrationProfileRepository registrationProfileRepository;
        private readonly ILoginRepository loginRepository;


        #endregion

        #region Constructors & Destructors

        public AccountController(ILoginRepository loginRepositoryParam,IRegistrationProfileRepository profileRepositoryParam,IProfileSettingsRepository settingRepositoryParam,IEmailRepository emailRepositoryParam , OodsServices accountParam, IConfigService configService, ICommonService commonService, IRepository repository) : base(configService, commonService)
        {
            accountService = accountParam;
            this.repository = repository;
            emailRepository = emailRepositoryParam;
            profileSettingsRepository = settingRepositoryParam;
            registrationProfileRepository = profileRepositoryParam;
            loginRepository = loginRepositoryParam;
            umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
        }


        public ActionResult ResendEmail(ForgotPasswordViewModel model, string returnUrl, string culture)
        {
            var result = new MessageResult();
            var user = accountService.SearchContact(model.Email, profileSettingsRepository.GetCellId(culture));

            if (user == null)
            {

                EmailTemplate notRegisteredTemplate = emailRepository.GetEmailTemplate("Not registered", culture);

                string[] sSPParams = { string.Concat("Subject=", notRegisteredTemplate.Subject), "Home_CTA=" + Helpers.UrlHelper.GetBaseUrl(), "Link_URL=" + Helpers.UrlHelper.GetBaseUrl() };

                var msEmail = accountService.SendMSEmail(model.Email, Convert.ToString(notRegisteredTemplate.EmailId), 0, sSPParams);

                return Json(result);

            }
            else if (user != null && user.login.Active == false)
            {
                var userId = Convert.ToString(user.consumer.CONSUMER_ID);
                var now = DateTime.UtcNow.AddDays(3);

                var token = EncryptionHelper.CreateToken(userId, EncryptionHelper.Hash(user.login.username), now);

                EmailTemplate confirmationTemplate = emailRepository.GetEmailTemplate("Confirmation", culture);

                var utmParam = "utm_medium=crm-email&utm_source=crm&utm_term=confirm-cta&utm_content=pending-registration&utm_campaign=registration-process";

                var msEmail = accountService.SendMSEmail(model.Email, Convert.ToString(confirmationTemplate.EmailId), user.consumer.CONSUMER_ID, new string[] { string.Concat("Link_URL=",Helpers.UrlHelper.GetBaseUrl() + "/umbraco/Surface/RegistrationPets/Validate", "?token=", token,"&",utmParam), string.Concat("Confirm_CTA=", Helpers.UrlHelper.GetBaseUrl() + "/umbraco/Surface/RegistrationPets/Validate", "?token=", token,"&",utmParam,string.Concat("&lang=",Session["culture"].ToString())),

               string.Concat("Subject=",confirmationTemplate.Subject) });

                switch (msEmail.Code)
                {
                    case 1000:
                        return Json(result);
                }

            } else if((user != null && user.login.Active == true)){
                result.Message ="";
                return Json(result);
            }

            return View();

        }

        #endregion

        #region Methods

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl,string culture)
        {
            var result = new MessageResult();
            var decodedUrl = "/Home"; // TODO : change value
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
                var user = accountService.Connect(model.Username, model.Password, profileSettingsRepository.GetCellId(culture), GetClientIP());


                if (user.login.username != null && user.login.Active)
                {
                    var login = new Core.Domain.Models.Registration.Login
                    {
                        Username = user.login.username,
                        Password = user.login.password,
                        SiteID = user.login.Site_ID,
                        ConsumerID = user.consumer.CONSUMER_ID
                    };

                    var consumer = new Consumer
                    {
                        Firstname = user.consumer.FIRSTNAME,
                        Surname = user.consumer.SURNAME
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

                        if (Request.IsAjaxRequest())
                        {
                            return Json(new {url = decodedUrl});
                        }

                        return Redirect(decodedUrl);
                    }

                    if (Request.IsAjaxRequest())
                    {
                        return Json(new {url = "/Home"});
                    }
                }

                if (Request.IsAjaxRequest())
                {
                    return Json(new {error = ""});
                }
            }
            catch (Exception exc)
            {
                ViewBag.error = exc.Message;
            }

            return Redirect(decodedUrl);
        }

        [HttpPost]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model, string returnUrl,string culture)
        {
            var result = new MessageResult();
            string forgotPasswordUrlAlias = registrationProfileRepository.GetForgotPasswordUrlAlias(culture);

            if (!ModelState.IsValid)
            {
                if (!Request.IsAjaxRequest())
                {
                    return Redirect(forgotPasswordUrlAlias);
                }
                return Json(new {MessageError = "Error Localized"});
            }

            var user = accountService.SearchContact(model.Email, profileSettingsRepository.GetCellId(culture));

            if (user == null)
            {
                EmailTemplate notRegisteredTemplate = emailRepository.GetEmailTemplate("Not registered",culture);
                string[] sSPParams = {string.Concat("Subject=", notRegisteredTemplate.Subject), "Name_URL=" + Helpers.UrlHelper.GetBaseUrl(), "Home_CTA=" + Helpers.UrlHelper.GetBaseUrl(), "Link_URL="+ Helpers.UrlHelper.GetBaseUrl() };

                var msEmail = accountService.SendMSEmail(model.Email, Convert.ToString(notRegisteredTemplate.EmailId), 0, sSPParams);

                return Json(result);
            }

            try

            {
                var userId = Convert.ToString(user.consumer.CONSUMER_ID);

                var existingUserToken = repository.GetSingle<UserToken>(x => x.UserId == userId && x.Action == UserAction.ForgotPassword);

           
                if (existingUserToken != null)
                {
                    repository.Delete(existingUserToken);
                    repository.UnitOfWork.Commit();
                }

                var now = DateTime.UtcNow.AddDays(3);

                var token = EncryptionHelper.CreateToken(userId, EncryptionHelper.Hash(user.login.username), now);

                Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(culture);

                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);

                EmailTemplate resetPasswordTemplate = emailRepository.GetEmailTemplate("Reset password", culture);
                var resetStringAlias = registrationProfileRepository.GetResetPasswordUrlAlias(culture);



                string[] sSPParams = { "Unsubscribe_URL=" + Helpers.UrlHelper.GetBaseUrl(),"Name_URL=" + Helpers.UrlHelper.GetBaseUrl(), "Link_URL=" + Helpers.UrlHelper.GetBaseUrl() + "/"+ resetStringAlias + "/?token=" + token, "Subject=" + resetPasswordTemplate.Subject, "Reset_CTA=" + Helpers.UrlHelper.GetBaseUrl() + "/"+ resetStringAlias + "/?token=" + token };


                var msEmail = accountService.SendMSEmail(user.login.username, Convert.ToString(resetPasswordTemplate.EmailId), user.consumer.CONSUMER_ID, sSPParams);

                switch (msEmail.Code)
                {
                    case 1000:
                        var userToken = new UserToken
                        {
                            Action = UserAction.ForgotPassword,
                            ExpirationDate = now,
                            Token = token,
                            UserId = Convert.ToString(user.consumer.CONSUMER_ID)
                        };

                        repository.Add(userToken);
                        repository.UnitOfWork.Commit();

                        return Json(result);

                    default:
                        return Json(new { MessageError = msEmail.Message });

                }
            }
            catch (Exception exc)
            {
                return Json(new {MessageError = exc.Message});
            }

            return Redirect("/"+forgotPasswordUrlAlias);
        }

        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordViewModel model, string token)
        {
            var cultureInfo = CurrentPage.GetCulture();
            Authentication.logOut();

            string resetPasswordUrlAlias = registrationProfileRepository.GetResetPasswordUrlAlias(cultureInfo.ToString());

            string loginUrlAlias = loginRepository.GetLoginUrlAlias(cultureInfo.ToString());

            Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureInfo.ToString());

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureInfo.ToString());

            UmbracoHelper helper = new UmbracoHelper(UmbracoContext.Current);

            if (!ModelState.IsValid || string.IsNullOrEmpty(model.Password))
            {
                var tokenParam = resetPasswordUrlAlias;

                if (!string.IsNullOrEmpty(token))
                {
                    tokenParam = tokenParam + "?token=" + token;
                }

                return Redirect(tokenParam);
            }

            var dataToken = token.Replace(" ", "+");

            var tokenInfo = new string[] { };

            try {
                 tokenInfo = EncryptionHelper.Decrypt(dataToken).Split(';');
            }
            catch (Exception)
            {

                return Redirect("/"+ resetPasswordUrlAlias);
            }

            var user = accountService.GetUser(Convert.ToInt32(tokenInfo[0]));

            if (user != null)
            {
                user.login.password = model.Password;

                var tokenToDelete = repository.GetSingle<UserToken>(x => x.Action == UserAction.ForgotPassword && x.Token == dataToken);

                repository.Delete(tokenToDelete);

                repository.UnitOfWork.Commit();
            }

            TempData["success"] = LocalizationHelper.GetDictionaryItem("Reset - Password - Success - Message");

            TempData["Confirmsuccess"] = LocalizationHelper.GetDictionaryItem("Reset - Password - Confirm - Success - Message -");

            accountService.UpdateUser(user, GetClientIP());

            return Redirect("/"+ resetPasswordUrlAlias);
        }

        private string GetClientIP()
        {
            return (Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ??
                    Request.ServerVariables["REMOTE_ADDR"]).Split(',')[0].Trim();
        }

        public ActionResult LogOut()
        {
            if (Authentication.isLogin())
            {
                Authentication.logOut();
                Session["NewFirstName"] = null;
                Session["user_profile"] = null;
                Session["user_profile_dl"] = null;
                return Redirect("/");
            }
            return Redirect("/");
        }

        public ActionResult IsUserLogged()
        {
            var statusUser = false;
            if (Authentication.isLogin())
            {
                statusUser = true;
            }
            return Json(new { status = statusUser });
        }

        private static string GenerateUrl(string baseUrl, string urlAlias, string token)
        {
            return $"{baseUrl}/{urlAlias}?token={token}";
        }

        private void GetModelStateErrors()
        {
            for (var i = 0; i < ModelState.Keys.Count; i++)
            {
                var item = ModelState.Values.ElementAt(i);

                if (item.Errors.Count > 0)
                {
                    var key = ModelState.Keys.ElementAt(i);
                    var errorMessage = item.Errors.First().ErrorMessage;

                    ModelState[key].Errors.Clear();
                    ModelState[key].Errors.Add(Umbraco.GetDictionaryValue(errorMessage));
                }
            }
        }

        #endregion
    }
}
