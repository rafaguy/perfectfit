using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mars.PerfectFit.Presentation.Web.ViewModels;
using Mars.PerfectFit.Core.Domain.Services;

namespace Mars.PerfectFit.Presentation.Web.Controllers
{
    public class ForgotPasswordController : BaseController
    {
        public ForgotPasswordController(IConfigService configService, ICommonService commonService) : base(configService, commonService)
        {
        }

        // GET: ForgotPassword
        public ActionResult ForgotPassword()
        {
            var model = new ForgotPasswordViewModel
            {

            };
            return View(model);
           
        }
    }
}