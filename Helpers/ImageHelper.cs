namespace Mars.PerfectFit.Presentation.Web.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using Mars.PerfectFit.Core.Domain.Models;
    using Umbraco.Core.Models;
    using Umbraco.Web;

    public static class ImageHelper
    {
        public static Image GetImage(IPublishedContent content)
        {
            if (content == null)
            {
                return new Image();
            }

            return new Image
            {
                Alt = content.HasProperty("altText") ? content.GetPropertyValue<string>("altText") : string.Empty,
                Extension = content.GetPropertyValue<string>("umbracoExtension"),
                Height = content.GetPropertyValue<double>("umrbacoHeight"),
                Path = content.GetCropUrl(),
                Size = content.GetPropertyValue<double>("umbracoBytes"),
                Width = content.GetPropertyValue<double>("umrbacoWidth")
            };
        }

        public static IEnumerable<Image> GetImages(IEnumerable<IPublishedContent> contents)
        {
            var publishedContents = contents as IList<IPublishedContent> ?? contents.ToList();

            if (contents != null && publishedContents.Any())
            {
                foreach (var content in publishedContents)
                {
                    yield return GetImage(content);
                }
            }
        }
    }
}
