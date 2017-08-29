using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mars.PerfectFit.Presentation.Web.ViewModels
{
    using Mars.PerfectFit.Core.Domain.Models;
    using Mars.PerfectFit.Core.Domain.Models.Coupons;

    public class CouponViewModel : ViewModelBase
    {
        public Coupon Coupon { get; set; }
    }
}
