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
        readonly FinanceEntities financeEntities = new FinanceEntities();

        [HttpGet]
        public HttpResponseMessage GetCardDetails(string id)
        {
            try
            {
                var cardDetails = from x in financeEntities.CompanyCards
                                  where x.UserName == id
                                  join y in financeEntities.Cards on x.CardTypeNo equals y.CardTypeNo
                                  join z in financeEntities.Consumers on x.UserName equals z.UserName
                                  select
                                  new
                                  { CardNumber=x.CardNumber, Name=z.Name, Validity=x.Validity, CardType=y.CardType, Activated = DateTime.Compare(x.Validity, DateTime.Now) >= 0 };
                var user = cardDetails.FirstOrDefault();

                if (cardDetails == null || cardDetails.Count() == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Consumer Card Details Could Not Be Fetched");
                }
                return Request.CreateResponse(HttpStatusCode.OK, user);
            } catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Something Went Wrong");
            }
        }

        [HttpGet]
        public HttpResponseMessage GetCreditDetails(string id)
        {
            try
            {
                CompanyCard consumerCard = financeEntities.CompanyCards.Include("Card").Where(c => c.UserName == id).FirstOrDefault();
                if (consumerCard == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Consumer Credit Details Could Not Be Found");
                }

                var creditDetails = new
                {
                    TotalCredit = consumerCard.Card.CardLimit,
                    CreditUsed = consumerCard.Card.CardLimit - consumerCard.Balance,
                    RemainingCredit = consumerCard.Balance
                };

                return Request.CreateResponse(HttpStatusCode.OK, creditDetails);
            } catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Something Went Wrong");
            }
        }

        [HttpGet]
        public HttpResponseMessage GetPurchasedProducts(string id)
        {
            try
            {
                var purchasedProducts = from a in financeEntities.Transactions
                                        where a.UserName == id
                                        join c in financeEntities.Products
                                        on a.ProductID equals c.ProductID
                                        select new
                                        {
                                            ProductName=c.ProductName,
                                            PurchaseDate=a.PurchaseDate,
                                            ProductCost = a.EMIAmount * a.EMI.Months,
                                            AmountPaid = (a.EMIAmount * a.EMI.Months) - a.RemainingAmount
                                        };
                if (purchasedProducts == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Purchased Products Could Not Be Fetched");
                }
                return Request.CreateResponse(HttpStatusCode.OK, purchasedProducts);
            } 
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Something Went Wrong");
            }
        }

        [HttpGet]
        public HttpResponseMessage GetRecentTransactions(string id)
        {
            try
            {
                var transaction = from a in financeEntities.Deductions
                                  where a.UserName == id
                                  join b in financeEntities.Products on a.ProductID equals b.ProductID
                                  select new
                                  {
                                      ProductID=a.ProductID,
                                      ProductName=b.ProductName,
                                      DeductionDate=a.DeductionDate,
                                      EMIAmount = a.EMIAmount
                                    };
            
            if (transaction == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Transaction History Could Not Be Fetched");
            }
            return Request.CreateResponse(HttpStatusCode.OK, transaction);

            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Something Went Wrong");

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
