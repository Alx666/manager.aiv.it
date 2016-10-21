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

namespace manager.aiv.it.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private AivEntities db = new AivEntities();

        public AccountController()
        {
        }


        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            //this.Session["UserId"] = 0;
            //this.Session["Roles"] = new List<RoleType>() { RoleType.Student, RoleType.Teacher, RoleType.Secretary, RoleType.Manager, RoleType.Director, RoleType.Bursar, RoleType.Admin };
            //return RedirectToLocal(returnUrl);

            return View();
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

            User hLogin = (from u in db.Users where u.Email == model.Email && u.Password == model.Password select u).FirstOrDefault();

            if (hLogin != null)
            {

                if(hLogin.PictureId != null)
                {
                    hLogin.Picture = db.Binaries.Find(hLogin.PictureId);
                }

                //List<RoleType> hRoles = hLogin.Roles.Select(r => (RoleType)r.Id).ToList();

                Session.LoadUser(hLogin);
                
                //this.Session["UserId"]  = hLogin.Id;
                //this.Session["Roles"]   = hRoles;
                //this.Session["User"]    = hLogin;

                //if (hRoles.Contains(RoleType.Student) && !hRoles.Contains(RoleType.Teacher))
                if(hLogin.IsOnly(RoleType.Student))
                    return RedirectToLocal("/Students/Details/" + hLogin.Id);
                else
                    return RedirectToLocal(returnUrl);
            }
            else
            {
                ModelState.AddModelError("", "Invalid login attempt.");
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