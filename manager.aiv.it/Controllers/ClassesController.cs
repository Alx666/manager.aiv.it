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
        private AivManagementEntities db = new AivManagementEntities();

        [CustomAuthorize(RoleType.Director, RoleType.Secretary, RoleType.Teacher, RoleType.Manager, RoleType.Admin, RoleType.Bursar)]
        public ActionResult Index()
        {            
            
            return View(db.ViewClassIndexes.Where(x => !x.IsClosed).ToList());
        }

        [CustomAuthorize(RoleType.Director, RoleType.Secretary, RoleType.Teacher, RoleType.Manager, RoleType.Admin, RoleType.Bursar)]
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

        [CustomAuthorize(RoleType.Director)]
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
        [CustomAuthorize(RoleType.Director)]
        public ActionResult Create([Bind(Include = "Id,EditionId,Section")] Class @class)
        {
            if (ModelState.IsValid)
            {
                db.Classes.Add(@class);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.EditionId = new SelectList(db.Editions.Where(e => e.DateEnd >= DateTime.Now), "Id", "Id", @class.EditionId);
            return View(@class);
        }

        // GET: Classes/Edit/5
        [CustomAuthorize(RoleType.Director)]
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
        [CustomAuthorize(RoleType.Director)]
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
        [CustomAuthorize(RoleType.Director)]
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


        // POST: Classes/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Class hToClose = db.Classes.Find(id);

            if (hToClose.Lessons.Count > 0)
            {
                hToClose.IsClosed = true;

                var hStudents = hToClose.ActiveStudents;
                hStudents.ToList().ForEach(x => x.ClassId = null);
            }
            else
            {
                db.Classes.Remove(hToClose);
            }
            
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
