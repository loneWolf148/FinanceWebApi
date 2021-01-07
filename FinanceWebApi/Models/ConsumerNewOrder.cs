using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinanceWebApi.Models
{
    public class ConsumerNewOrder
    {
        public string UserName { get; set; }
        public int ProductID { get; set; }
        public int SchemeNo { get; set; }
        public decimal EMIAmount { get; set; }
    }
}