using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

using FinanceWebApi.Models;

namespace FinanceWebApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ProductController : ApiController
    {
        readonly FinanceEntities financeEntities = new FinanceEntities();

        [HttpGet]
        public HttpResponseMessage GetProducts()
        {
            try
            {
                var allProducts = from product in financeEntities.Products
                                  select
                                  new { product.ProductID, product.ProductName, product.ProductCost, product.ProductAvailability };
                return Request.CreateResponse(HttpStatusCode.OK, allProducts);
            } catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Something Went Wrong");
            }
        }

        [HttpGet]
        public HttpResponseMessage GetProduct(int id)
        {
            try
            {
                var selectedProduct = (from prod in financeEntities.Products
                                       where prod.ProductID == id
                                       select new { prod.ProductID, prod.ProductName, prod.ProductDetails, prod.ProductCost })
                                       .Single();

                if (selectedProduct == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, $"No Such Product Exists");
                }
                return Request.CreateResponse(HttpStatusCode.OK, selectedProduct);
            } catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Something Went Wrong");
            }
        }

        [HttpGet]
        public HttpResponseMessage GetEmiSchemes()
        {
            try
            {
                var schemes = from scheme in financeEntities.EMIs
                              select new { scheme.SchemeNo, scheme.Months };
                return Request.CreateResponse(HttpStatusCode.OK, schemes);
            } catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Something Went Wrong");
            }
        }

        [HttpPost]
        public HttpResponseMessage PlaceOrder([FromBody] ConsumerNewOrder newOrder)
        {
            try
            {
                var purchasedProduct = (from prod in financeEntities.Products
                                        where prod.ProductID == newOrder.ProductID
                                        select new { prod.ProductCost })
                                        .Single();

                var selectedScheme = (from scheme in financeEntities.EMIs
                                      where scheme.SchemeNo == newOrder.SchemeNo
                                      select new { scheme.Months })
                                      .Single();

                if (purchasedProduct == null || selectedScheme == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Order could not be placed");
                }

                DateTime purchaseDate = DateTime.Now;
                decimal remainingAmount = newOrder.EMIAmount * selectedScheme.Months;

                Transaction newOrderTransaction = new Transaction()
                {
                    UserName = newOrder.UserName,
                    ProductID = newOrder.ProductID,
                    SchemeNo = newOrder.SchemeNo,
                    PurchaseDate = purchaseDate,
                    RemainingAmount = remainingAmount,
                    EMIAmount = newOrder.EMIAmount
                };
                financeEntities.Entry(newOrderTransaction).State = System.Data.Entity.EntityState.Added;
                financeEntities.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, new { Message = "Order Placed Successfully" });
            } catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Something Went Wrong");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                financeEntities.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
