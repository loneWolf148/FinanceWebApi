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
    public class LoginController : ApiController
    {
        readonly FinanceEntities db = new FinanceEntities();

        [HttpPost]
        public HttpResponseMessage LoginConsumer([FromBody] Login log)
        {
            try
            {
                Consumer consumer = db.Consumers.Where(c => c.UserName == log.UserName && c.Password == log.Password).FirstOrDefault();
                if (consumer == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Username Or Password Incorrect");
                }
                if (consumer.IsPending)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "User Not Yet Verified");
                }
                if (consumer.IsOpen == false)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Account Closed");
                }
                return Request.CreateResponse(HttpStatusCode.Accepted, new { consumer.UserName, consumer.Name, consumer.Password });
            } catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "User Could Not be Logged In");
            }
        }

        [HttpPost]
        public HttpResponseMessage LoginAdmin([FromBody] Login log)
        {
            try
            {
                Admin admin = db.Admins.Where(a => a.AdminName == log.UserName && a.Password == log.Password).FirstOrDefault();
                if (admin == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Admin Name Or Password Incorrect");
                }
                return Request.CreateResponse(HttpStatusCode.Accepted, new { UserName = admin.AdminName, Name = admin.AdminName, admin.Password });
            } catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Admin Could Not Be Logged In");
            }
        }

        [HttpPut]
        public HttpResponseMessage ChangePassword(string id, [FromBody] Password updatePassword)
        {
            try
            {
                Consumer consumer = db.Consumers.Find(id);
                if (consumer == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "User Does Not Exist");
                }
                if (consumer.Password != updatePassword.OldPassword)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Wrong Old Password");
                }
                consumer.Password = updatePassword.NewPassword;
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.Accepted, "Password Updated Successfully");
            } catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Password Could Not Be Changed");
            }
        }

        [HttpGet]
        public HttpResponseMessage ForgotPassword(string id, string email)
        {
            try
            {
                Consumer existingConsumer = db.Consumers.Where(c => c.UserName == id && c.Email == email).FirstOrDefault();
                if (existingConsumer == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Username or Email Incorrect");
                }
                if (existingConsumer.IsOpen == false)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Your Account Is Closed");
                }
                return Request.CreateResponse(HttpStatusCode.Accepted, $"Your Password is {existingConsumer.Password}");
            } catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Password Could Not Be Fetched");
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

