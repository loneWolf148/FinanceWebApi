using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using FinanceWebApi.Model;

namespace FinanceWebApi.Controllers
{
    [EnableCors(origins:"*",headers:"*",methods:"*")]
    public class LoginController : ApiController
    {
        FinanceEntities db = new FinanceEntities();
            
        [HttpPost]
        public HttpResponseMessage GetUser(Login log)
        {
            var display = db.Consumers.Where(m => m.UserName == log.UserName && m.Password == log.Password).FirstOrDefault();
            if (display != null)
            {
                return Request.CreateResponse(HttpStatusCode.Accepted, "Login Successfully");
            }
            
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Username or Password");
            }
        }
        [HttpPost]
        public HttpResponseMessage GetAdmin(Login log)
        {
            var display = db.Admins.Where(m => m.AdminName == log.UserName && m.Password == log.Password).FirstOrDefault();
            if (display != null)
            {
                return Request.CreateResponse(HttpStatusCode.Accepted, "Admin Login Successfully");
            }

            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Name or Password");
            }
        }
    }
}
