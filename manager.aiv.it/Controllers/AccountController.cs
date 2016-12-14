using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using manager.aiv.it.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Net;

namespace manager.aiv.it.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private AivEntities db = new AivEntities();
     

        public AccountController()
        {
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetPasswordToken(string sEmail, string sPassword, string sNewPassword)
        {

            User hUser = db.Users.Find(Session.GetUser().Id);

            if (sPassword == sNewPassword)
            {
                hUser.Password = sNewPassword;
                db.SaveChanges();
                return RedirectToAction("Details", "Students", new { Id = hUser.Id });
            }
            else
            {
                //TODO: 
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }



            //List<User> hQuery = db.Users.Where((u) => (u.Email == sEmail && u.Password == sPassword)).ToList();

            //if (hQuery != null)
            //{
            //    User hAuth = hQuery.First();

            //    if (hAuth != null)
            //    {
            //        sEmail = sEmail.ToLower();

            //        string sToken = $"{sEmail} {sNewPassword}".Encrypt();

            //        Emailer.Send(sEmail, "Password Change Request", "http://37.187.154.24:28080/Account/PasswordReset?sEncodedData=" + sToken);

            //        Login("");
            //    }
            //}            
        }


        // GET: /Account/Login (for now)
        //[AllowAnonymous]
        //public void PasswordReset(string sEncodedData)
        //{
        //    string[] sChangeData = sEncodedData.Decrypt().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        //    string sEmail = sChangeData[0];
        //    string sPassword = sChangeData[1];

        //    User hUser = db.Users.Where(u => u.Email == sEmail).First();

        //    hUser.Password = sPassword;

        //    db.SaveChanges();

        //    Emailer.Send(hUser.Email, "Password Changed", "Password Change Successful");
        //}


        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

//#if DEBUG
//            User hDeveloper = db.Roles.Find((int)RoleType.Developer).Users.First();
//            Session.LoadUser(hDeveloper);
//            hDeveloper.Picture = (hDeveloper != null && hDeveloper.Picture != null) ? new Binary(hDeveloper.Picture) : null; //Hax

//            return RedirectToLocal(returnUrl);
//#else
            return View();
//#endif
        }


        //POST: /Account/Login
       [HttpPost]
       [AllowAnonymous]
       [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User hLogin = (from u in db.Users where u.Email == model.Email && u.Password == model.Password select u).Include(u => u.Picture).FirstOrDefault();
            
            // C# usa la short-circuit evaluation, perciò se
            // hLogin è null, le altre espressioni non vengono valutate.
            if (hLogin != null && hLogin.BinaryId != null && hLogin.Picture == null)
            {
                hLogin.BinaryId = null;
                db.SaveChanges();
            }
            
            if (hLogin != null)
            {
                Session.LoadUser(hLogin);

                //hLogin.Picture = (hLogin != null && hLogin.Picture != null) ? new Binary(hLogin.Picture) : null; //Hax
                EventLog.Log(db, hLogin, EventLogType.LoginSuccess, "Logged in", true);

                if (hLogin.IsOnly(RoleType.Student))
                    return RedirectToLocal("/Students/Details/" + hLogin.Id);
                else
                    return RedirectToAction("Index", "Home");
            }
            else
            {
                
                ModelState.AddModelError("", "Invalid login attempt.");
                EventLog.Log(db, null, EventLogType.LoginFailed, $"Login Failed: \"{Request.UserHostAddress}\" on (\"{model.Email}\",\"{model.Password}\")", true);
                return View(model);
            }
        }

        [AllowAnonymous]
        public ActionResult LogOff(string returnUrl)
        {
            this.Session.Abandon();
            return RedirectToLocal(returnUrl);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }

#region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
#endregion
    }
}