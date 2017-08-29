namespace Mars.PerfectFit.Presentation.Web.Controllers
{
    using System.Text;
    using System.Web.Mvc;
    using Mars.PerfectFit.Core.Domain.Models;
    using Mars.PerfectFit.Core.Domain.Models.Configuration;
    using Mars.PerfectFit.Core.Domain.Services;
    using Mars.PerfectFit.Presentation.Web.ViewModels;
    using Umbraco.Web.Mvc;
    using System.Globalization;
    using Umbraco.Web;
    using System.Threading;
    using Helpers;
    using System.Dynamic;
    using System.Collections.Generic;
    using System;
    using System.Linq;

    public abstract class BaseController : SurfaceController, IRenderController
    {
        #region Fields

        private readonly ICommonService commonService;
        private readonly IConfigService configService;

        #endregion

        #region Constructors & Destructors

        protected BaseController(IConfigService configService, ICommonService commonService)
        {
            this.configService = configService;
            this.commonService = commonService;
        }

        #endregion

        #region Methods

        protected ViewResult View(ViewModelBase viewModel)
        {
            return View(null, viewModel);
        }

        protected ViewResult View(string viewName, ViewModelBase viewModel)
        {
            var page = commonService.GetCurrentPage(CurrentPage.Id);

            viewModel.PageId = CurrentPage.Id;
            viewModel.Culture = page.Culture;
            viewModel.CanonicalUrl = page.MetaTag.CanonicalUrl;
            viewModel.MetaDescription = page.MetaTag.MetaDescription;
            viewModel.MetaKeywords = page.MetaTag.MetaKeywords;
            viewModel.NoIndex = page.MetaTag.NoIndex;
            viewModel.PageTitle = page.MetaTag.Title;

            viewModel.OgDescription = page.OpenGraph.Description;
            viewModel.OgImage = !string.IsNullOrEmpty(page.OpenGraph.Image) ? ImageHelper.GetImage(Umbraco.TypedMedia(page.OpenGraph.Image)) : new Image();
            viewModel.OgSiteName = page.OpenGraph.SiteName;
            viewModel.OgTitle = page.OpenGraph.Title;
            viewModel.OgType = page.OpenGraph.Type;
            viewModel.OgUrl = page.OpenGraph.Url.AbsoluteUri;

          
            var siteSetting = configService.GetConfig<SiteSetting>();

            viewModel.Favicon = ImageHelper.GetImage(Umbraco.TypedMedia(siteSetting.Favicon));

            viewModel.FooterScripts = siteSetting.FooterScripts;
            viewModel.SiteLogo = ImageHelper.GetImage(Umbraco.TypedMedia(siteSetting.Logo));

            viewModel.FooterDescritpion = siteSetting.FooterDescription;

            viewModel.FooterLogo = ImageHelper.GetImage(Umbraco.TypedMedia(siteSetting.FooterLogo));

            var userProfileUrl = Umbraco.TypedContentAtXPath("//profile").FirstOrDefault(x => x.GetCulture().Equals(CurrentPage.GetCulture()));

            var registrationProfileUrl = Umbraco.TypedContentAtXPath("//registration").FirstOrDefault(x => x.GetCulture().Equals(CurrentPage.GetCulture()));

            viewModel.UserProfileUrl = userProfileUrl != null ? userProfileUrl.GetPropertyValue<string>("umbracoUrlAlias") : userProfileUrl.UrlName;

            viewModel.RegistrationProfileUrl = registrationProfileUrl != null ? registrationProfileUrl.GetPropertyValue<string>("umbracoUrlAlias") : registrationProfileUrl.UrlName;

            viewModel.Navigation = new Navigation
            {
                SecondaryLogo = ImageHelper.GetImage(Umbraco.TypedMedia(siteSetting.SecondaryLogo)),
                SecondaryLogoMobile = ImageHelper.GetImage(Umbraco.TypedMedia(siteSetting.SecondaryLogoMobile)),
                MenuItems = commonService.MenuItems,
                ProfileUrl = userProfileUrl != null ? userProfileUrl.GetPropertyValue<string>("umbracoUrlAlias") : userProfileUrl.UrlName,
                RegistrationUrl = registrationProfileUrl !=null ? registrationProfileUrl.GetPropertyValue<string>("umbracoUrlAlias") : registrationProfileUrl.UrlName
            };


            return base.View(viewName, viewModel);
        }

        protected string GenerateTracking(CustomVariable tracking)
        {
            var builder = new StringBuilder();

            builder.AppendLine("dataLayer.push({");
            builder.AppendLine($"MetaDataSiteMap: '{tracking.MetaDataSiteMap}',");
            builder.AppendLine($"MetaDataPetsCategorization: '{tracking.MetaDataPetsCategorization}',");
            builder.AppendLine($"MetaDataProductLifestage: '{tracking.MetaDataProductLifestage}',");
            builder.AppendLine($"MetaDataProductName: '{tracking.MetaDataProductName}',");
            builder.AppendLine($"MetaDataProductFlavour: '{tracking.MetaDataProductFlavour}'");
            builder.AppendLine("});");

            return builder.ToString();
        }

        public string GenerateCustomVariable(ExpandoObject obj)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("dataLayer.push({");
            foreach (var property in (IDictionary<String, Object>)obj)
            {

                builder.Append(string.Format("{0}:'{1}',", property.Key, property.Value));

            }
            builder.Length--;
            builder.AppendLine("});");

            return builder.ToString();
        }


        protected string getUnsubscribeUrl(string culture)
        {
            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            var unsubscribeUrl =
                umbracoHelper.TypedContentAtXPath("//profile")
                    .FirstOrDefault(x => x.GetCulture().ToString() == culture);

            return unsubscribeUrl != null ? unsubscribeUrl.GetPropertyValue<string>("umbracoUrlAlias") : null;

        }





        #endregion
    }
}
