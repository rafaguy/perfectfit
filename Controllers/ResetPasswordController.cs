namespace Mars.PerfectFit.Presentation.Web.Controllers
{
    using System;
    using System.Web.Mvc;
    using Mars.PerfectFit.Core.Domain;
    using Mars.PerfectFit.Core.Domain.Models.Registration;
    using Mars.PerfectFit.Core.Domain.Services;
    using Mars.PerfectFit.Core.Utilities;
    using Mars.PerfectFit.Presentation.Web.Helpers;
    using Mars.PerfectFit.Presentation.Web.ViewModels;
    using Umbraco.Web;
    using System.Dynamic;
    using Core.Domain.Repositories;
    using System.Linq;

    public class ResetPasswordController : BaseController
    {
        #region Fields

        private readonly IRepository repository;
        private readonly ILoginRepository loginRepository;

        #endregion

        #region Constructors & Destructors

        public ResetPasswordController(ILoginRepository loginRepositoryParam,IConfigService configService, ICommonService commonService, IRepository repository) : base(configService, commonService)
        {
            this.repository = repository;
            loginRepository = loginRepositoryParam;
        }

        #endregion

        #region Methods

        // GET: ResetPassword
        public ActionResult ResetPassword(string token)
        {
            var bannerId = CurrentPage.HasProperty("banner") ? CurrentPage.GetPropertyValue<int>("banner") : 0;

            try
            {
                string dataToken = token.Replace(" ","+");
                var tokenInfo = EncryptionHelper.Decrypt(dataToken).Split(';');

                var userToken = repository.GetSingle<UserToken>(x => x.Action == UserAction.ForgotPassword && x.Token == dataToken);//DateTime.Parse(tokenInfo[2], CurrentPage.GetCulture());

                if (EncryptionHelper.IsTokenExpired(userToken.ExpirationDate))
                {
                    var tokenToDelete = repository.GetSingle<UserToken>(x => x.Action == UserAction.ForgotPassword && x.Token == token);

                    repository.Delete(tokenToDelete);

                    repository.UnitOfWork.Commit();

                    var urlCult = Request.Url.ToString();
                    var protocolCult = urlCult.Split('/').First();
                    var domainCult = urlCult.Split('/')[2];
                    var cultureBydom = loginRepository.GetCultureByDomain(string.Concat(protocolCult, "//", domainCult));

                   

                    return Redirect("/"+ loginRepository.GetLoginUrlAlias(cultureBydom.ToString()));
                }
            }
            catch (Exception)
            {
                var urlCult = Request.Url.ToString();
                var protocolCult = urlCult.Split('/').First();
                var domainCult = urlCult.Split('/')[2];
                var cultureBydom = loginRepository.GetCultureByDomain(string.Concat(protocolCult, "//", domainCult));
                return Redirect("/"+ loginRepository.GetLoginUrlAlias(cultureBydom.ToString()));
            }

            var media = Umbraco.TypedMedia(bannerId);

            var model = new ResetPasswordViewModel
            {
                Banner = ImageHelper.GetImage(media)
            };

            //MetaDataSiteMa Data Layer

            dynamic datalayer = new ExpandoObject();
            datalayer.MetaDataSiteMap = "System Page";
            var layer = this.GenerateCustomVariable(datalayer);
            ViewBag.MetaDataSiteMap = layer;

            return View(model);
        }

        #endregion
    }
}
