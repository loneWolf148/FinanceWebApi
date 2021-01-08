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
    [EnableCors(origins:"*",headers:"*",methods:"*")]
    public class RegisterController : ApiController
    {
        public HttpResponseMessage GetUsername(string id)
        {
            using(FinanceEntities db=new FinanceEntities())
            {
                var data = db.Consumers.Find(id);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Username alraedy present");
                }
            }
        }
        [HttpGet]
        public HttpResponseMessage GetCardType()
        {
            using(FinanceEntities db=new FinanceEntities())
            {
                var data = from card in db.Cards select new {CardType=card.CardType };
                return Request.CreateResponse(HttpStatusCode.OK, data);
            }
        }
        public HttpResponseMessage GetIfsc(string Ifsc)
        {
            using(FinanceEntities db=new FinanceEntities())
            {
                var data = db.Banks.Find(Ifsc);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,data);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "IFSC code not found");
                }
            }
        }
    }
}
