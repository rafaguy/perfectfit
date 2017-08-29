using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mars.PerfectFit.Presentation.Web.Constents
{
    public class SiteParameters
    {
        public static int getPetWeightFormToCrm(int weightIndex)
        {
            int index = 0;
            switch (weightIndex)
            {
                case 17:
                    index = 1;
                    break;
                case 18:
                    index = 2;
                    break;
                case 19:
                    index = 17;
                    break;
                case 20:
                    index = 18;
                    break;
                case 21:
                    index = 4;
                    break;
            }
            return index;
        }

        public static int getPetWeightCrmToForm(int weightIndex)
        {
            int index = 0;
            switch (weightIndex)
            {
                case 1:
                    index = 17;
                    break;
                case 2:
                    index = 18;
                    break;
                case 17:
                    index = 19;
                    break;
                case 18:
                    index = 20;
                    break;
                case 4:
                    index = 21;
                    break;
            }
            return index;
        }

        public static int getPetCoatFormToCrm(int coatIndex)
        {
            int index = 0;
            switch (coatIndex)
            {
                case 1:
                    index = 2;
                    break;
                case 2:
                    index = 3;
                    break;
                case 3:
                    index = 1;
                    break;
            }
            return index;
        }

        public static int getPetCoatCrmToForm(int coatIndex)
        {
            int index = 0;
            switch (coatIndex)
            {
                case 2:
                    index = 1;
                    break;
                case 3:
                    index = 2;
                    break;
                case 1:
                    index = 3;
                    break;
            }
            return index;
        }
        public static string GetCellId(string culture)
        {
            string cell_ids = string.Empty;
            switch (culture)
            {
                case "en-GB":
                    cell_ids = "GB_2660_0";
                    break;
                case "fr-FR":
                    cell_ids = "FR_2658_0";
                    break;
                case "de-DE":
                    cell_ids = "DE_2659_0";
                    break;
                case "pl-PL":
                    cell_ids = "PL_2661_0";
                    break;
                case "de-AT":
                    cell_ids = "AT_2672_0";
                    break;
                case "nl-NL":
                    cell_ids = "NL_2673_0";
                    break;

            }
            return cell_ids;
        }

        public static int getDiffDateToMonth(DateTime birthDate)
        {
            DateTime current = DateTime.Now;
            int monthDiff = ((current.Year * 12) + current.Month) - ((birthDate.Year * 12) + birthDate.Month);
            return monthDiff;
        }

        public static int getSliderUpdateProfileIndex(DateTime birthDate)
        {
            bool year = false;
            int month = getDiffDateToMonth(birthDate);
            if (month >= 12)
            {
                double monthToYear = month / 12;
                month = (int)Math.Round(monthToYear);
                year = true;
            }
            if (year)
            {
                return month + 5;
            }
            else
            {
                if (month > 6)
                {
                    return month - 6;
                }
                else
                {
                    return 0;
                }
            }
            return 0;
        }
    }
}