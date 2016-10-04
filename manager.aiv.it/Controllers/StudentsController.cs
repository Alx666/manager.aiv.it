using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using manager.aiv.it;
using manager.aiv.it.Models;

namespace manager.aiv.it.Controllers
{
    public class StudentsController : Controller
    {
        private AivEntities db = new AivEntities();

        // GET: Students
        [CustomAuthorize(RoleType.Secretary)]
        public ActionResult Index()
        {
            var model = from r in db.Roles
                        from s in r.Users
                        where r.Id == (int)RoleType.Student
                        select s;

            return View(model);
        }

        // GET: Students/Details/5        
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

            return View(user);
        }

        // GET: Students/Create
        [CustomAuthorize(RoleType.Secretary)]
        public ActionResult Create()
        {
            ViewBag.ClassId = new SelectList(db.Classes, "Id", "DisplayName");
            ViewBag.RoleId  = new SelectList(db.Roles, "Id", "Name");
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Secretary)]
        public ActionResult Create([Bind(Include = "Id,Name,Surname,Email,Password,Mobile,RegistrationDate,ClassId")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ClassId = new SelectList(db.Classes, "Id", "Section", user.ClassId);
            ViewBag.RoleId  = new SelectList(db.Roles, "Id", "Name");
            return View(user);
        }

        // GET: Students/Edit/5
        [CustomAuthorize(RoleType.Secretary)]
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

            ViewBag.ClassId = new SelectList(db.Classes.Include(c => c.Edition), "Id", "DisplayName", user.ClassId);
            ViewBag.RoleId  = new SelectList(db.Roles, "Id", "Name");

            return View(user);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Secretary)]
        public ActionResult Edit([Bind(Include = "Id,Name,Surname,Email,Password,Mobile,RegistrationDate,ClassId")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ClassId = new SelectList(db.Classes, "Id", "Section", user.ClassId);
            ViewBag.RoleId = new SelectList(db.Roles, "Id", "Name");
            return View(user);
        }

        // GET: Students/Delete/5
        [CustomAuthorize(RoleType.Secretary)]
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

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Secretary)]
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
