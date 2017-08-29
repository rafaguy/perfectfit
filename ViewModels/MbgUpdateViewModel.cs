using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mars.PerfectFit.Presentation.Web.ViewModels
{
    public class MbgUpdateViewModel
    {
        public string Iban { get; set; }

        public string Gencode { get; set; }

        public string GencodePhotoId { get; set; }

        public string ReceiptPhotoId { get; set; }

        public string ApiParticipation { get; set; }
    }
}