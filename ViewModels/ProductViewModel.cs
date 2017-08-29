namespace Mars.PerfectFit.Presentation.Web.ViewModels
{
    using System.Collections.Generic;
    using Mars.PerfectFit.Core.Domain.Models.Products;

    public sealed class ProductViewModel : ViewModelBase
    {
        #region Properties

        public IEnumerable<Benefit> Benefits { get; set; }

        public string Description { get; set; }

        public IEnumerable<Feeding> Feedings { get; set; }

        public string FlavorIcon { get; set; }

        public Nutrition Nutrition { get; set; }

        public string Packshots { get; set; }

        public ProductFormula ProductFormula { get; set; }

        public string Title { get; set; }

        #endregion
    }
}
