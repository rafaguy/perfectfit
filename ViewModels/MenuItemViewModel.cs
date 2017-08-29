namespace Mars.PerfectFit.Presentation.Web.ViewModels
{
    using System.Collections.Generic;
    using Mars.PerfectFit.Core.Domain.Models;
    using Mars.PerfectFit.Core.Domain.Models.Cms;

    public class MenuItemViewModel
    {
        #region Constructors & Destructors

        public MenuItemViewModel()
        {
            SubMenuItems = new List<MenuItem>();
        }

        #endregion

        #region Properties

        public string Alignment { get; set; }

        public string CssClassName { get; set; }

        public int Id { get; set; }

        public int? ParentId { get; set; }

        public int RelatedPage { get; set; }

        public string RelativeLink { get; set; }

        public ICollection<MenuItem> SubMenuItems { get; }

        public string Title { get; set; }

        #endregion
    }
}
