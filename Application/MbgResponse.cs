using Mars.PerfectFit.Core.Domain.Models.MoneyBackGuarantee;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Umbraco.Web;

namespace Mars.PerfectFit.Presentation.Web.Application
{
    public class MbgResponse
    {

        public class MbgRootobject
        {
            [JsonProperty("AE18")]
            public Campagne[] campagne { get; set; }
        }

        public class Campagne
        {
            public Global global { get; set; }
            public object[] products { get; set; }
            public object[] proofs { get; set; }
        }

        public class Global
        {
            public string participation_id { get; set; }
            public object is_conform { get; set; }
            public object non_conformity_type { get; set; }
            public Reason_Not_Conformity[] reason_not_conformity { get; set; }
        }

        public class Reason_Not_Conformity
        {
            public string code { get; set; }
            public string libelle_court { get; set; }
            public string libelle_long { get; set; }
        }

    }
}