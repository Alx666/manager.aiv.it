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
    public class ClassesController : Controller
    {
        private AivEntities db = new AivEntities();

        // GET: Classes
        public ActionResult Index()
        {            
            return View(db.Classes.Include(x => x.Edition).ToList());
        }

        // GET: Classes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Class @class = db.Classes.Find(id);
            if (@class == null)
            {
                return HttpNotFound();
            }
            return View(@class);
        }

        // GET: Classes/Create
        public ActionResult Create()
        {
            ViewBag.EditionId = new SelectList(db.Editions.Where(e => e.DateEnd >= DateTime.Now), "Id", "DisplayName");
            return View();
        }

        // POST: Classes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,EditionId,Section")] Class @class)
        {
            if (ModelState.IsValid)
            {
                db.Classes.Add(@class);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.EditionId = new SelectList(db.Editions, "Id", "Id", @class.EditionId);
            return View(@class);
        }

        // GET: Classes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Class @class = db.Classes.Find(id);
            if (@class == null)
            {
                return HttpNotFound();
            }
            ViewBag.EditionId = new SelectList(db.Editions.Select(e => new { Id = e.Id, Name = e.Course.Name + " " + e.Course.Grade + " " + e.AcademicYear }), "Id", "Name");
            return View(@class);
        }

        // POST: Classes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,EditionId,Section")] Class @class)
        {
            if (ModelState.IsValid)
            {
                db.Entry(@class).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.EditionId = new SelectList(db.Editions, "Id", "Id", @class.EditionId);
            return View(@class);
        }

        // GET: Classes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Class @class = db.Classes.Find(id);
            if (@class == null)
            {
                return HttpNotFound();
            }
            return View(@class);
        }

        [CustomAuthorize(RoleType.Director, RoleType.Admin)]
        public ActionResult ResetCredentials(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var hClassUsers = from u in db.Users where u.ClassId == id select u;

            hClassUsers.ToList().ForEach(u => 
            {
                string sPassword = StringExtensions.Random(6);
                u.Password = sPassword;
            });

            db.SaveChanges();

            hClassUsers.ToList().ForEach(u => 
            {
                Emailer.Send(u.Email, "Credentials Change", $"Your login has been changed!{Environment.NewLine}Username: {u.Email}{Environment.NewLine}Password: {u.Password}{Environment.NewLine}");
            });

            return RedirectToAction("Index");
            
        }



        // POST: Classes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Class @class = db.Classes.Find(id);
            db.Classes.Remove(@class);
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
