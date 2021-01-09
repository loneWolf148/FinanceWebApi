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
                List<Product> allProducts = financeEntities.Products.ToList();
                return Request.CreateResponse(HttpStatusCode.OK, allProducts);
            } catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Products Could Not Be Fetched");
            }
        }

        [HttpGet]
        public HttpResponseMessage GetProduct(int id)
        {
            try
            {
                Product selectedProduct = financeEntities.Products.Find(id);
                if (selectedProduct == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Product Could Not Be Found");
                }
                return Request.CreateResponse(HttpStatusCode.OK, selectedProduct);
            } catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Product Could Not Be Selected For Ordering");
            }
        }

        [HttpGet]
        public HttpResponseMessage GetEmiSchemes()
        {
            try
            {
                List<EMI> schemes = financeEntities.EMIs.ToList();
                return Request.CreateResponse(HttpStatusCode.OK, schemes);
            } catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "EMI Schemes Could Not Be Fetched");
            }
        }

        [HttpGet]
        public HttpResponseMessage GetMonthlyEMI(int id, int schemeNo)
        {
            try
            {
                Product selectedProduct = financeEntities.Products.Find(id);
                if (selectedProduct == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No Prouduct Found");
                }
                EMI scheme = financeEntities.EMIs.Find(schemeNo);
                if (scheme == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "EMI Scheme Not Found");
                }
                int emiCost = CalculateMonthlyEMI(selectedProduct, scheme.Months);
                return Request.CreateResponse(HttpStatusCode.OK, emiCost);
            } catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage PlaceOrder([FromBody] Order newOrder)
        {
            try
            {
                Consumer consumer = financeEntities.Consumers.Find(newOrder.UserName);
                if (consumer == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Username Is Invalid");
                }

                Product selectedProduct = financeEntities.Products.Find(newOrder.ProductID);
                if (selectedProduct == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Product Could Not Be Found");
                }

                EMI scheme = financeEntities.EMIs.Find(newOrder.SchemeNo);
                if (scheme == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "EMI Scheme Could Not Be Found");
                }

                CompanyCard companyCard = financeEntities.CompanyCards.Where(c => c.UserName == newOrder.UserName).FirstOrDefault();
                if (companyCard == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Conumser Card Balance Could Not Be Fetched");
                }

                int emiCost = CalculateMonthlyEMI(selectedProduct, scheme.Months);
                int orderAmount = emiCost * scheme.Months;

                if (orderAmount > companyCard.Balance)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Insufficient Balance To Place Order");
                }

                if (DateTime.Compare(companyCard.Validity, DateTime.Now) < 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Your Card Is Deactivated");
                }

                Transaction newTransaction = new Transaction()
                {
                    UserName = consumer.UserName,
                    ProductID = selectedProduct.ProductID,
                    SchemeNo = scheme.SchemeNo,
                    PurchaseDate = DateTime.Now,
                    EMIAmount = emiCost,
                    RemainingAmount = orderAmount - emiCost,
                    LastChecked = DateTime.Now
                };
                financeEntities.Entry(newTransaction).State = System.Data.Entity.EntityState.Added;

                var currentBalance = companyCard.Balance - emiCost;
                companyCard.Balance = currentBalance;

                financeEntities.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.Created, "Order Placed Successfully");
            } catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Order Could Not Be Placed");
            }
        }

        private int CalculateMonthlyEMI(Product product, int months)
        {
            return Convert.ToInt32(product.ProductCost / months + product.ProductCost % months);
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
