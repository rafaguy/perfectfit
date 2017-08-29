using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mars.PerfectFit.Core.Domain.Services;
using Umbraco.Web;
using Mars.PerfectFit.Presentation.Web.ViewModels;
using Mars.PerfectFit.Presentation.Web.Helpers;

namespace Mars.PerfectFit.Presentation.Web.Controllers
{
    public class CookiePageController : BaseController
    {
        public CookiePageController(IConfigService configService, ICommonService commonService) : base(configService, commonService)
        {
        }

        // GET: CookiePage
        public ActionResult Index()
        {
            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            var contentPage = umbracoHelper.TypedContentAtXPath("//cookiePage").FirstOrDefault(x => x.GetCulture().ToString() == CurrentPage.GetCulture().ToString());
            var bannerId = CurrentPage.HasProperty("banner") ? CurrentPage.GetPropertyValue<int>("banner") : 0;
            var media = Umbraco.TypedMedia(bannerId);
            CookieViewModel cookie = new CookieViewModel
            {
                Body = contentPage.GetPropertyValue<string>("body"),
                Banner = ImageHelper.GetImage(media)
            };
            return View(cookie);
        }
    }
}