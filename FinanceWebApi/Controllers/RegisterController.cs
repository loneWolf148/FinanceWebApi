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
    public class RegisterController : ApiController
    {
        readonly FinanceEntities db = new FinanceEntities();

        [HttpPost]
        public HttpResponseMessage RegisterConsumer([FromBody] Consumer consumer)
        {
            try
            {
                Consumer existingConsumer = db.Consumers.Find(consumer.UserName);
                if (existingConsumer != null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Username Already Exists");
                }

                Bank newConsumerBank = db.Banks.Find(consumer.IFSC);
                if (newConsumerBank == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid IFSC Code");
                }

                Consumer newConsumer = new Consumer
                {
                    UserName = consumer.UserName,
                    Name = consumer.Name,
                    Email = consumer.Email,
                    PhoneNumber = consumer.PhoneNumber,
                    Password = consumer.Password,
                    AccountNo = consumer.AccountNo,
                    IFSC = consumer.IFSC,
                    Address = consumer.Address,
                    CardTypeNo = consumer.CardTypeNo,
                    ApplicationDate = DateTime.Now,
                    DateOfBirth = consumer.DateOfBirth,
                    IsPending = true,
                    IsOpen = false
                };

                Card companyCard = db.Cards.Find(consumer.CardTypeNo);
                if (companyCard == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid Card Scheme");
                }

                CompanyCard newConsumerCard = new CompanyCard()
                {
                    CardNumber = $"{consumer.UserName}{consumer.IFSC}",
                    CardTypeNo = consumer.CardTypeNo,
                    UserName = consumer.UserName,
                    Validity = DateTime.Now.AddMonths(24),
                    Balance = companyCard.CardLimit,
                    IsOpen = false
                };

                db.Entry(newConsumer).State = System.Data.Entity.EntityState.Added;
                db.Entry(newConsumerCard).State = System.Data.Entity.EntityState.Added;
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.Created, "User Registered Successfully And Awaiting Verification");
            } catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Consumer Could Not Be Registered");
            }
        }

        [HttpGet]
        public HttpResponseMessage GetBanks()
        {
            try
            {
                var banks = from bank in db.Banks
                            select new { bank.IFSC, bank.BankName };
                return Request.CreateResponse(HttpStatusCode.OK, banks);
            } catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Bank Details Could Not Be Fetched");
            }
        }

        [HttpGet]
        public HttpResponseMessage CheckUserName(string id)
        {
            try
            {
                var data = db.Consumers.Find(id);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Username Is Available");
                } else
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, "Username Already Present");
                }
            } catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Something Went Wrong");
            }
        }

        [HttpGet]
        public HttpResponseMessage GetCardTypes()
        {
            try
            {
                var data = db.Cards.ToList();
                return Request.CreateResponse(HttpStatusCode.OK, data);
            } catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Card Types Could Not Be Fetched");
            }
        }

        [HttpGet]
        public HttpResponseMessage CheckIfsc(string id)
        {
            try
            {
                using (FinanceEntities db = new FinanceEntities())
                {
                    var data = db.Banks.Find(id);
                    if (data != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, data);
                    } else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "IFSC Code Not Found");
                    }
                }
            } catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "IFSC Code Could Not Be Checked");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
