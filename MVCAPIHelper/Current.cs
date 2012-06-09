using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//Copyrigth Flumeware 2012.
namespace MVCAPIHelper
{
    public class Current
    {
        public static Authentication.IAuthenticationProvider AuthenticationProvider { get; set; }
        public static Quota.IQuotaStore QuotaStore { get; set; }

        public static double MaxRequestPerIpPerMin { get; set; }
        public static double MaxRequestPerTokenPerMin { get; set; }
        public static double MaxRequestPerUserPerMin { get; set; }
    }
}
