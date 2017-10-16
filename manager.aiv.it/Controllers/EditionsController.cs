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
    public class EditionsController : Controller
    {
        private AivManagementEntities db = new AivManagementEntities();

        // GET: Editions
        public ActionResult Index()
        {            
            return View(db.Editions.OrderByDescending(e => e.AcademicYear).ToList());
        }

        // GET: Editions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Edition edition = db.Editions.Find(id);
            if (edition == null)
            {
                return HttpNotFound();
            }
            return View(edition);
        }

        // GET: Editions/Create
        public ActionResult Create(int? courseid)
        {
            ViewBag.CourseId = new SelectList(db.Courses, "Id", "DisplayName");

            if (courseid.HasValue)
                ViewBag.topics = new MultiSelectList(db.Topics.Where(t => !t.DateDeprecated.HasValue && t.CourseId == courseid), "Id", "DisplayName");
            else
                ViewBag.topics = new MultiSelectList(Enumerable.Empty<Topic>());

            Edition hNew        = new Edition();
            hNew.AcademicYear   = (short)DateTime.Now.Year;
            hNew.DateStart      = DateTime.Now;
            hNew.DateEnd        = DateTime.Now.AddMonths(9);

            return View(hNew);
        }

        // POST: Editions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CourseId,AcademicYear,DateStart,DateEnd")] Edition edition, List<int> topics)
        {
            if (ModelState.IsValid)
            {                
                if (topics != null)
                {
                    var hTopics = topics.Select(t => db.Topics.Find(t)).ToList();
                    hTopics.ForEach(t => edition.Topics.Add(t));
                }

                edition.DateStart  = edition.DateStart;
                edition.DateEnd    = edition.DateEnd;
                edition.Course     = db.Courses.Find(edition.CourseId);

                db.Editions.Add(edition);
                db.SaveChanges();

                EventLog.Log(db, Session.GetUser(), EventLogType.EditionCreated, $"Created Edition{edition.DisplayName}", true);

                return RedirectToAction("Index");
            }

            ViewBag.CourseId = new SelectList(db.Courses, "Id", "Name", edition.CourseId);
            return View(edition);
        }

        // GET: Editions/Edit/5
        public ActionResult Edit(int? id, int? courseid)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Edition edition = db.Editions.Find(id);

            if (courseid == null)
                courseid = edition.CourseId;

            if (edition == null)
            {
                return HttpNotFound();
            }


            ViewBag.CourseId  = new SelectList(db.Courses.Select(c => new { Id = c.Id, Name = c.Name + " "  + c.Grade }), "Id", "Name", edition.Course.Id);
            ViewBag.topics    = new MultiSelectList(db.Topics.Where(t => !t.DateDeprecated.HasValue && t.CourseId == courseid), "Id", "DisplayName");

            return View(edition);
        }

        // POST: Editions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CourseId,AcademicYear,DateStart,DateEnd")] Edition edition, List<int> topics)
        {
            Edition hEdition = db.Editions.Find(edition.Id);
            hEdition.Topics.Clear();

            if (ModelState.IsValid)
            {
                if (topics != null)
                {
                    var hTopics = topics.Select(t => db.Topics.Find(t)).ToList();
                    hTopics.ForEach(t => hEdition.Topics.Add(t));
                }

                hEdition.DateStart  = edition.DateStart;
                hEdition.DateEnd    = edition.DateEnd;
                hEdition.Course     = db.Courses.Find(edition.CourseId);

                db.Entry(hEdition).State = EntityState.Modified;
                db.SaveChanges();

                EventLog.Log(db, Session.GetUser(), EventLogType.EditionEdited, $"Edited Edition{hEdition.DisplayName}", true);

                return RedirectToAction("Index");
            }

            ViewBag.CourseId = new SelectList(db.Courses, "Id", "Name", edition.CourseId);
            return View(edition);
        }

        // GET: Editions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Edition edition = db.Editions.Find(id);
            if (edition == null)
            {
                return HttpNotFound();
            }
            return View(edition);
        }

        // POST: Editions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Edition edition = db.Editions.Find(id);
            db.Editions.Remove(edition);
            db.SaveChanges();

            EventLog.Log(db, Session.GetUser(), EventLogType.EditionDeleted, $"Deleted Edition{edition.DisplayName}", true);

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
