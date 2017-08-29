namespace Mars.PerfectFit.Presentation.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using Mars.PerfectFit.Core.Domain;
    using Mars.PerfectFit.Core.Domain.Models.Sampling;
    using Mars.PerfectFit.Core.Domain.Services;
    using Mars.PerfectFit.Presentation.Web.Helpers;
    using Mars.PerfectFit.Presentation.Web.Security;
    using Mars.PerfectFit.Presentation.Web.ViewModels;
    using Umbraco.Web;
    using System.Dynamic;

    public class SampleListingController : BaseController
    {
        private readonly IRepository repository;

        public SampleListingController(IConfigService configService, ICommonService commonService,
            IRepository repository) : base(configService, commonService)
        {
            this.repository = repository;
        }
        //TODO: Sample tracking
        public ActionResult Index()
        {
            var sampleSettingContainer = Umbraco
                .TypedContentAtXPath("//sampleSettings")
                .FirstOrDefault(x => x.GetCulture().Equals(CurrentPage.GetCulture()));

            var sampleSetting = new SampleSetting
            {
                GlobalRequestLimit = sampleSettingContainer.GetPropertyValue<int>("globalRequestLimit"),
                PerUserRequestLimit = sampleSettingContainer.GetPropertyValue<int>("perUserRequestLimit")
            };

            var model = new SampleListingViewModel
            {
                Banner = ImageHelper.GetImage(Umbraco.TypedMedia(CurrentPage.GetPropertyValue<int>("banner")))
            };

            if (Authentication.isLogin())
            {
                model.Title = CurrentPage.GetPropertyValue<string>("title") ?? string.Empty;
                model.Description = CurrentPage.GetPropertyValue<string>("description") ?? string.Empty;
                model.SubTitle = CurrentPage.GetPropertyValue<string>("subTitle") ?? string.Empty;
                ViewBag.ButtonText = LocalizationHelper.GetDictionaryItem("Sample - Sample Box - Button Text");
            }
            else
            {
                model.Title = CurrentPage.GetPropertyValue<string>("anonymousTitle") ?? string.Empty;
                model.Description = CurrentPage.GetPropertyValue<string>("anonymousDescription") ?? string.Empty;
                model.SubTitle = CurrentPage.GetPropertyValue<string>("anonymousSubTitle") ?? string.Empty;
                ViewBag.ButtonText = LocalizationHelper.GetDictionaryItem("Sample - Sample Box - Button Text - Anonymous");
            }

            if (sampleSetting.GlobalRequestLimit == 0)
            {
                ViewBag.NoOffersAvailable = LocalizationHelper.GetDictionaryItem("Sample - Landing Page - No Offers Available");
                return View(model);
            }

            var container = Umbraco
                .TypedContentAtXPath("//sampleContainer")
                .Single(x => x.GetCulture().Equals(CurrentPage.GetCulture()));

            var validSamples = new List<Sample>();

            foreach (var content in container.Children)
            {
                var sample = new Sample
                {
                    AbsoluteUri = new Uri(content.UrlAbsolute()),
                    Culture = content.GetCulture(),
                    Description = content.GetPropertyValue<string>("description") ?? string.Empty,
                    EndDate = content.GetPropertyValue<DateTime?>("endDate"),
                    Id = content.Id,
                    RelativeLink = content.UrlName,
                    Quantity = content.GetPropertyValue<int>("quantity"),
                    SampleId = content.GetPropertyValue<string>("sampleId"),
                    StartDate = content.GetPropertyValue<DateTime>("startDate"),
                    SubTitle = content.GetPropertyValue<string>("subTitle") ?? string.Empty,
                    Title = content.GetPropertyValue<string>("title") ?? string.Empty
                };

                var packshots = content.GetPropertyValue<string>("packshots")
                    .Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse);

                sample.Packshots = ImageHelper.GetImages(Umbraco.TypedMedia(packshots)).ToArray();

                var dateDiff = -1;

                if (sample.EndDate.HasValue)
                {
                    dateDiff = sample.EndDate.Value == DateTime.MinValue
                        ? 1
                        : DateTime.Compare(sample.EndDate.Value, DateTime.Now.Date);
                }

                if (dateDiff >= 0)
                {
                    validSamples.Add(sample);
                }
            }

            var samples = new List<Sample>();

            int consumerId;

            int.TryParse(Authentication.GetConsumerId(), out consumerId);

            foreach (var sample in validSamples)
            {
                if (consumerId > 0)
                {
                    var userQuota = repository.GetCount<SampleRequest>(x => x.SampleId == sample.SampleId && x.UserId == consumerId);

                    if (userQuota >= sampleSetting.PerUserRequestLimit)
                    {
                        ViewBag.NoOffersAvailable = LocalizationHelper.GetDictionaryItem("Sample - Landing Page - Max Limit Reached");
                        return View(model);
                    }
                }

                samples.Add(sample);
            }

            model.Samples = samples;

            if (!model.Samples.Any())
            {
                ViewBag.NoOffersAvailable = LocalizationHelper.GetDictionaryItem("Sample - Landing Page - No Offers Available");
            }

            TempData["step"] = 1;

            dynamic datalayer = new ExpandoObject();

            datalayer.MetaDataSiteMap = "Sample";

            if (Session["user_profile_dl"] != null)
            {
                datalayer.MetaDataUserProfile = Session["user_profile_dl"];
            }

            var layer = this.GenerateCustomVariable(datalayer);

            ViewBag.MetaDataSiteMap = layer;

            return View(model);
        }
    }
}
