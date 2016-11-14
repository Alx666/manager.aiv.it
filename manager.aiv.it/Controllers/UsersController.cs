using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using manager.aiv.it;
using System.Data.Entity.Validation;
using System.Diagnostics;

namespace manager.aiv.it.Controllers
{
    public class UsersController : Controller
    {
        private AivEntities db = new AivEntities();


        [CustomAuthorize(RoleType.Admin)]
        public ActionResult Index()
        {
            //var users = db.Roles.Include(hR => hR.Users).SelectMany(hU => hU.Users);
            var users = (from r in db.Roles
                         from u in r.Users
                         where r.Id > (int)RoleType.Student
                         select u).Distinct();

            var roleless = from u in db.Users where u.Roles.Count() == 0 select u;

            users = users.Union(roleless);

            
            return View(users.ToList());
        }

        // GET: Users/Details/5
        [CustomAuthorize(RoleType.Admin, RoleType.Student)]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            if(Session.GetUser().Id == user.Id)
            {
                return View(user);
            }

            return Redirect(Request.UrlReferrer.ToString());
        }

        // GET: Users/Create
        [CustomAuthorize(RoleType.Admin)]
        public ActionResult Create()
        {
            ViewBag.ClassId = new SelectList(db.Classes, "Id", "Section");
            ViewBag.roles = new SelectList(db.Roles, "Id", "Name");
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Admin)]
        public ActionResult Create([Bind(Include = "Id,Name,Surname,Email,Password,Mobile")] User user, List<int> roles)
        {
            if (ModelState.IsValid)
            {
                if (roles != null)
                {
                    var hRoles = roles.Select(r => db.Roles.Find(r)).ToList();
                    hRoles.ForEach(r => user.Roles.Add(r));
                }

                user.RegistrationDate = DateTime.Now;
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ClassId = new SelectList(db.Classes, "Id", "Section", user.ClassId);
            ViewBag.RoleId = new SelectList(db.Roles, "Id", "Name");
            return View(user);
        }

        // GET: Users/Edit/5
        [CustomAuthorize(RoleType.Admin)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            User user = db.Users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            
            ViewBag.ClassId = new SelectList(db.Classes, "Id", "Section", user.ClassId);            
            ViewBag.roles   = new MultiSelectList(db.Roles, "Id", "Name", user.Roles.Select(x => x.Id));

            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Admin)]
        public ActionResult Edit([Bind(Include = "Id,Name,Surname,Email,Password,Mobile")] User user, List<int> roles)
        {
            if (ModelState.IsValid)
            {                
                User hUser = db.Users.Find(user.Id);
                hUser.Roles.Clear();

                if (roles != null)
                {
                    var hRoles = roles.Select(r => db.Roles.Find(r)).ToList();
                    hRoles.ForEach(r => hUser.Roles.Add(r));
                }

                bool bSendNewCredentials = hUser.Password != user.Password || hUser.Email != user.Email;

                hUser.Name = user.Name;
                hUser.Surname = user.Surname;
                hUser.Email = user.Email;
                hUser.Mobile = user.Mobile;
                hUser.Password = user.Password;


                Binary hPicture = db.Binaries.Find(hUser.BinaryId);
                if (hPicture == null)
                    hUser.BinaryId = null;

                db.Entry(hUser).State = EntityState.Modified;
                db.SaveChanges();

                if (bSendNewCredentials)
                    Emailer.Send(hUser.Email, "Credentials Change", $"Your login has been changed!{Environment.NewLine}Username: {hUser.Email}{Environment.NewLine}Password: {hUser.Password}{Environment.NewLine}");

                return RedirectToAction("Index");
            }
            ViewBag.ClassId = new SelectList(db.Classes, "Id", "Section", user.ClassId);
            ViewBag.RoleId = new SelectList(db.Roles, "Id", "Name");
            return View(user);
        }

        // GET: Users/Delete/5
        [CustomAuthorize(RoleType.Admin)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePicture(int userId, HttpPostedFileBase picture)
        {
            User user = db.Users.Find(userId);
            User hLogged = Session.GetUser();

            if (hLogged.Id != user.Id)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (user != null && picture != null)
            {
                Binary hBin = Binary.CreateFrom(picture);

                //user.Picture
                if (user.Picture != null)
                    db.Binaries.Remove(user.Picture);

                user.Picture = hBin;
            }

            db.SaveChanges();
            Session.LoadUser(user);

            return Redirect(Request.UrlReferrer.ToString());
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Admin)]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
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
