using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FinanceWebApi.Models;
using System.Web.Http.Cors;

namespace FinanceWebApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]

    public class DashboardController : ApiController
    {
        readonly FinanceEntities db = new FinanceEntities();

        [HttpGet]
        public HttpResponseMessage GetProducts()
        {
            
                var productlist = from product in db.Products 
                                  select new { product.ProductID, product.ProductName, product.ProductCost, product.ProductAvailability };
                return Request.CreateResponse(HttpStatusCode.OK, productlist);
            
        }
        [HttpGet]
        public HttpResponseMessage GetCardDetails(string cardNo)
        {
            var companyCard = from x in db.CompanyCards
                              where x.CardNumber == cardNo
                              join y in db.Cards on x.CardTypeNo equals y.CardTypeNo
                              join z in db.Consumers on x.UserName equals z.UserName
                              select new { x.CardNumber, z.Name, x.Validity, y.CardType };
            return Request.CreateResponse(HttpStatusCode.OK, companyCard);
            
        }
        [HttpGet]

        public HttpResponseMessage GetProductsPurchased(string cardNo)
        {
            
            var productspurchased = from a in db.Transactions
                                    join b in db.CompanyCards on a.UserName equals b.UserName where b.CardNumber==cardNo
                                    join c in db.Products on a.ProductID equals c.ProductID 
                                    select new
                                    {
                                        c.ProductName,
                                        c.ProductCost,
                                        a.RemainingAmount
                
                                    };
             return Request.CreateResponse(HttpStatusCode.OK, productspurchased);


        }
    }
}
