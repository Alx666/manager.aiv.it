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
            var hClasses = from hC in db.Classes
                           select new ClassViewModels
                           {
                               Id           = hC.Id,
                               Name         = hC.Edition.Course.Name + " " + hC.Edition.Course.Grade + hC.Section,
                               Students     = hC.Students.Count(),
                               Lessons      = hC.Lessons.Count()
                           };

            var res = from l in db.Lessons
                      where l.ClassSize.HasValue
                      group l by l.ClassId into g
                      select new { Id = g.Key, Frequency = g.Average(x => x.Frequency) };

            var final = hClasses.ToList();
            final.ForEach(x => x.Frequency = res.Where(k => k.Id == x.Id).Select(y => y.Frequency).FirstOrDefault());

            return View(final);
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
            ViewBag.EditionId = new SelectList(db.Editions.Select(e => new { Id = e.Id, Name = e.Course.Name + " " + e.Course.Grade + " " + e.AcademicYear }), "Id", "Name");
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
