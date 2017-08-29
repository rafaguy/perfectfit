namespace Mars.PerfectFit.Presentation.Web.Helpers
{
    using System;
    using System.Web;

    public static class UrlHelper
    {
        #region Methods

        public static string GetBaseUrl()
        {
            HttpContextBase context = new HttpContextWrapper(HttpContext.Current);

            var protocol = context.Request.IsSecureConnection ? "https" : "http";

            var url = context.Request.Url;

            var host = string.Empty;

            if (url != null)
            {
                host = url.Host;
            }

            return $"{protocol}://{host}";
        }

        #endregion
    }
}
