using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using manager.aiv.it;

namespace manager.aiv.it.Controllers
{
    public class AssignmentsController : Controller
    {
        private AivEntities db = new AivEntities();

        // GET: Assignments
        [CustomAuthorize(RoleType.Teacher, RoleType.Director, RoleType.Manager)]
        public ActionResult Index()
        {
            var assignments = db.Assignments.Include(a => a.Class).Include(a => a.Exercise).Include(a => a.Teacher);
            return View(assignments.ToList());
        }

        // GET: Assignments/Details/5
        [CustomAuthorize(RoleType.Teacher, RoleType.Manager, RoleType.Director)]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Assignment assignment = db.Assignments.Find(id);
            if (assignment == null)
            {
                return HttpNotFound();
            }
            return View(assignment);
        }

        // GET: Assignments/Create
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Create()
        {
            ViewBag.ClassId = new SelectList(db.Classes, "Id", "Section");
            ViewBag.ExerciseId = new SelectList(db.Exercises, "Id", "Name");
            ViewBag.TeacherId = new SelectList(db.Users, "Id", "Name");
            return View();
        }

        // POST: Assignments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Create([Bind(Include = "Id,ClassId,ExerciseId,Deadline,Description,TeacherId,Date")] Assignment assignment)
        {
            if (ModelState.IsValid)
            {
                db.Assignments.Add(assignment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ClassId = new SelectList(db.Classes, "Id", "Section", assignment.ClassId);
            ViewBag.ExerciseId = new SelectList(db.Exercises, "Id", "Name", assignment.ExerciseId);
            ViewBag.TeacherId = new SelectList(db.Users, "Id", "Name", assignment.TeacherId);
            return View(assignment);
        }

        // GET: Assignments/Edit/5
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Assignment assignment = db.Assignments.Find(id);
            if (assignment == null)
            {
                return HttpNotFound();
            }
            ViewBag.ClassId = new SelectList(db.Classes, "Id", "Section", assignment.ClassId);
            ViewBag.ExerciseId = new SelectList(db.Exercises, "Id", "Name", assignment.ExerciseId);
            ViewBag.TeacherId = new SelectList(db.Users, "Id", "Name", assignment.TeacherId);
            return View(assignment);
        }

        // POST: Assignments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Edit([Bind(Include = "Id,ClassId,ExerciseId,Deadline,Description,TeacherId,Date")] Assignment assignment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(assignment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ClassId = new SelectList(db.Classes, "Id", "Section", assignment.ClassId);
            ViewBag.ExerciseId = new SelectList(db.Exercises, "Id", "Name", assignment.ExerciseId);
            ViewBag.TeacherId = new SelectList(db.Users, "Id", "Name", assignment.TeacherId);
            return View(assignment);
        }

        // GET: Assignments/Delete/5
        [CustomAuthorize(RoleType.Teacher, RoleType.Director)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Assignment assignment = db.Assignments.Find(id);
            if (assignment == null)
            {
                return HttpNotFound();
            }
            return View(assignment);
        }

        // POST: Assignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Teacher, RoleType.Director)]
        public ActionResult DeleteConfirmed(int id)
        {
            Assignment assignment = db.Assignments.Find(id);
            db.Assignments.Remove(assignment);
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
