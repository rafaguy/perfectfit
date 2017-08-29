namespace Mars.PerfectFit.Presentation.Web.Helpers
{
    using System.IO;
    using System.Web;
    using System.Web.Hosting;
    using Umbraco.Core;
    using Umbraco.Core.Configuration;
    using Umbraco.Web;
    using Umbraco.Web.Routing;
    using Umbraco.Web.Security;

    public static class ContextHelper
    {
        // use to fake an HTTP context when outside
        // of the request pipeline

        public static HttpContext FakeHttpContext =>
            HttpContext.Current ??
            new HttpContext(new SimpleWorkerRequest("http://tempuri.org", string.Empty, new StringWriter()));

        // Use to ensure that the singleton instance
        // of the Umbraco context is created

        public static UmbracoContext GetUmbracoContext()
        {
            var umbracoContext = UmbracoContext.Current;

            if (umbracoContext != null)
            {
                return umbracoContext;
            }

            HttpContextBase httpContext = new HttpContextWrapper(FakeHttpContext);

            var applicationContext = ApplicationContext.Current;

            umbracoContext = UmbracoContext.EnsureContext(
                httpContext,
                applicationContext,
                new WebSecurity(httpContext, applicationContext),
                UmbracoConfig.For.UmbracoSettings(),
                UrlProviderResolver.Current.Providers,
                false);

            return umbracoContext;
        }
    }
}
