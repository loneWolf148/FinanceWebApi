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
            try
            {
                var productlist = from product in db.Products
                                  select new { product.ProductID, product.ProductName, product.ProductCost, product.ProductAvailability };
                return Request.CreateResponse(HttpStatusCode.OK, productlist);
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Something Went Wrong");

            }

        }
        [HttpGet]
        public HttpResponseMessage GetCardDetails(string id)
        {
            try
            {
                var companyCard = from x in db.CompanyCards
                                  where x.UserName == id
                                  join y in db.Cards on x.CardTypeNo equals y.CardTypeNo
                                  join z in db.Consumers on x.UserName equals z.UserName
                                  select new { x.CardNumber, z.Name, x.Validity, y.CardType, Activated = DateTime.Compare(x.Validity, DateTime.Now) >= 0 };
                return Request.CreateResponse(HttpStatusCode.OK, companyCard);
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Something Went Wrong");

            }

        }

        [HttpGet]

        public HttpResponseMessage GetProductsPurchased(string id)
        {
            try
            {

                var productspurchased = from a in db.Transactions
                                        where a.UserName == id
                                        join c in db.Products on a.ProductID equals c.ProductID
                                        select new
                                        {
                                            c.ProductName,
                                            a.PurchaseDate,
                                            AmountPaid = c.ProductCost - a.RemainingAmount

                                        };
                return Request.CreateResponse(HttpStatusCode.OK, productspurchased);
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Something Went Wrong");

            }


        }
    }
}
