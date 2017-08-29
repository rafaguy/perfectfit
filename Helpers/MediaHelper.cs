using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mars.PerfectFit.Presentation.Web.Helpers
{
    using Umbraco.Core.Models;
    using Umbraco.Core.Services;

    public static class MediaHelper
    {
        public static IMedia GetOrCreateMediaFolders(IMediaService service, string culture, string directory)
        {
            const string mediaTypeAlias = Umbraco.Core.Constants.Conventions.MediaTypes.Folder;

            var root = service.GetRootMedia().FirstOrDefault(d => d.Name == culture);

            if (root == null)
            {
                root = service.CreateMedia(culture, -1, mediaTypeAlias);
                service.Save(root);
            }

            var child = root.Children().FirstOrDefault(x => x.Name == directory);

            if (child == null)
            {
                child = service.CreateMedia(directory, root, mediaTypeAlias);
                service.Save(child);
            }


            return child;
        }
    }
}
