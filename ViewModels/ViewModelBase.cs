namespace Mars.PerfectFit.Presentation.Web.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Mars.PerfectFit.Core.Domain.Models;
    using Mars.PerfectFit.Core.Domain.Models.Cms;

    public abstract class ViewModelBase
    {
        #region Properties

        public Uri CanonicalUrl { get; set; }

        public CultureInfo Culture { get; set; }

        public Image Favicon { get; set; }

        public string FooterScripts { get; set; }

        public string MetaDescription { get; set; }

        public string MetaKeywords { get; set; }

        public bool NoIndex { get; set; }

        public string OgDescription { get; set; }

        public Image OgImage { get; set; }

        public string OgSiteName { get; set; }

        public string OgTitle { get; set; }

        public string OgType { get; set; }

        public string OgUrl { get; set; }

        public string PageTitle { get; set; }

        public Image SiteLogo { get; set; }

        public int PageId { get; set; }

        public string UserProfileUrl { get; set; }

        public string RegistrationProfileUrl { get; set; }

        public Navigation Navigation { get; set; }

        public Image FooterLogo { get; set; }

        public string FooterDescritpion { get; set; }

        #endregion
    }
}
