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
    public class DEBUG_ExercisesController : Controller
    {
        private AivEntities db = new AivEntities();

        // GET: DEBUG_Exercises
        public ActionResult Index()
        {
            var exercises = db.Exercises.Include(e => e.Course).Include(e => e.Type).Include(e => e.Author).Include(e => e.Binary);
            return View(exercises.ToList());
        }

        // GET: DEBUG_Exercises/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Exercise exercise = db.Exercises.Find(id);
            if (exercise == null)
            {
                return HttpNotFound();
            }
            return View(exercise);
        }

        // GET: DEBUG_Exercises/Create
        public ActionResult Create()
        {
            ViewBag.CourseId    = new SelectList(db.Courses, "Id", "Name");
            ViewBag.TypeId      = new SelectList(db.ExerciseTypes, "Id", "Name");
            ViewBag.AuthorId    = new SelectList(db.Users, "Id", "Name");
            ViewBag.BinaryId    = new SelectList(db.Binaries, "Id", "Filename");
            return View();
        }

        // POST: DEBUG_Exercises/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CourseId,Name,Description,BinaryId,AuthorId,Value,TypeId")] Exercise exercise)
        {
            if (ModelState.IsValid)
            {
                db.Exercises.Add(exercise);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CourseId = new SelectList(db.Courses, "Id", "Name", exercise.CourseId);
            ViewBag.TypeId = new SelectList(db.ExerciseTypes, "Id", "Name", exercise.TypeId);
            ViewBag.AuthorId = new SelectList(db.Users, "Id", "Name", exercise.AuthorId);
            ViewBag.BinaryId = new SelectList(db.Binaries, "Id", "Filename", exercise.BinaryId);
            return View(exercise);
        }

        // GET: DEBUG_Exercises/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Exercise exercise = db.Exercises.Find(id);
            if (exercise == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseId = new SelectList(db.Courses, "Id", "Name", exercise.CourseId);
            ViewBag.TypeId = new SelectList(db.ExerciseTypes, "Id", "Name", exercise.TypeId);
            ViewBag.AuthorId = new SelectList(db.Users, "Id", "Name", exercise.AuthorId);
            ViewBag.BinaryId = new SelectList(db.Binaries, "Id", "Filename", exercise.BinaryId);
            return View(exercise);
        }

        // POST: DEBUG_Exercises/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CourseId,Name,Description,BinaryId,AuthorId,Value,TypeId")] Exercise exercise)
        {
            if (ModelState.IsValid)
            {
                db.Entry(exercise).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CourseId = new SelectList(db.Courses, "Id", "Name", exercise.CourseId);
            ViewBag.TypeId = new SelectList(db.ExerciseTypes, "Id", "Name", exercise.TypeId);
            ViewBag.AuthorId = new SelectList(db.Users, "Id", "Name", exercise.AuthorId);
            ViewBag.BinaryId = new SelectList(db.Binaries, "Id", "Filename", exercise.BinaryId);
            return View(exercise);
        }

        // GET: DEBUG_Exercises/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Exercise exercise = db.Exercises.Find(id);
            if (exercise == null)
            {
                return HttpNotFound();
            }
            return View(exercise);
        }

        // POST: DEBUG_Exercises/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Exercise exercise = db.Exercises.Find(id);
            db.Exercises.Remove(exercise);
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
