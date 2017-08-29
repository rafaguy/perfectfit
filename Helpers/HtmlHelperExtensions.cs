using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web;

namespace Mars.PerfectFit.Presentation.Web.Helpers
{
    public static class HtmlHelperExtensions
    {
        private static readonly UmbracoHelper umbracoHelper = new UmbracoHelper(UmbracoContext.Current);

        public static string MediaUrlWithDomain(this HtmlHelper html, string source)
        {
            return $"{UrlHelper.GetBaseUrl()}/{source}";
        }

        public static string GetMediaItem(this HtmlHelper helper, string mediaId)
        {
            return umbracoHelper.TypedMedia(mediaId)?.GetCropUrl() ?? string.Empty;
        }
    }
}