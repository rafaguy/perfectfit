using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Web;

namespace Mars.PerfectFit.Presentation.Web.Helpers
{
    public class LocalizationHelper
    {
        private static readonly UmbracoHelper umbracoHelper = new UmbracoHelper(UmbracoContext.Current);

        public static string GetDictionaryItem(string dictionaryKey)
        {
            var error = umbracoHelper.GetDictionaryValue(dictionaryKey);
            

            if (string.IsNullOrEmpty(error))
            {
                //throw new Exception(string.Format("The dictionary key '{0}' for the required error message is empty or does not exist", dictionaryKey));
                error = "";
            }

            return error;
        }
    }
}