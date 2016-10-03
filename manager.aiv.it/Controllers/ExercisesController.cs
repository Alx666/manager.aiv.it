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
    public class ExercisesController : Controller
    {
        private AivEntities db = new AivEntities();

        [CustomAuthorize(RoleType.Teacher, RoleType.Manager, RoleType.Director)]
        public ActionResult Index()
        {
            IEnumerable<ExerciseViewModels> hIndex = from e in db.Exercises select new ExerciseViewModels()
            {
                Id = e.Id,
                Name = e.Name,
                Author = e.Author.Name + " " + e.Author.Surname,
                Topics = e.Topics.Select(x => x.Name + ", " + x.Description).ToList(),
                Value = e.Value,
                Type = e.Type.Name,
                Course = e.Course.Name + " " + e.Course.Grade
            };

            return View(hIndex.ToList());
        }

        [CustomAuthorize(RoleType.Teacher, RoleType.Manager, RoleType.Director)]
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

        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Create(int? courseid)
        {
            User hTeacher = db.Users.Find((int)this.Session["UserId"]);
            
            ViewBag.CourseId = new SelectList(hTeacher.Courses.Select(c => new { Id = c.Id, Name = c.Name + " " + c.Grade }), "Id", "Name");
            ViewBag.value    = new SelectList(Enumerable.Range(1, 15));
            ViewBag.type     = new SelectList(db.ExerciseTypes, "Id", "Name");

            if (!courseid.HasValue)
            {
                ViewBag.topics = new MultiSelectList(Enumerable.Empty<Topic>(), "Id", "Name");
            }
            else
            {
                Edition hLast = (from e in db.Editions where e.CourseId == courseid.Value orderby e.DateStart descending select e).FirstOrDefault();

                if (hLast != null)
                {
                    ViewBag.topics = new MultiSelectList(hLast.Topics.Select(t => new { Id = t.Id, Name = t.Name + ", " + t.Description }), "Id", "Name");
                }
                else
                {
                    ViewBag.topics = new MultiSelectList(Enumerable.Empty<Topic>(), "Id", "Name");
                }                                
            }

            return View();
        }

        // POST: Exercises/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Create([Bind(Include = "Id,CourseId,Name,Description,Value")] Exercise exercise, List<int> topics)
        {
            User hAuthor = db.Users.Find((int)this.Session["UserId"]);

            if (ModelState.IsValid && hAuthor != null)
            {
                if(topics != null)
                    topics.ForEach(t => exercise.Topics.Add(db.Topics.Find(t)));

                exercise.Author = hAuthor;

                db.Exercises.Add(exercise);                
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            //ViewBag.BinaryId = new SelectList(db.Binaries, "Id", "Id", exercise.BinaryId);
            //ViewBag.CourseId = new SelectList(db.Courses, "Id", "Name", exercise.CourseId);
            //ViewBag.TopicId = new SelectList(db.Topics, "Id", "Name", exercise.TopicId);

            return View(exercise);
        }

        // GET: Exercises/Edit/5
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Edit(int? id, int? courseid)
        {                                                
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Exercise    exercise    = db.Exercises.Find(id);
            if (exercise == null)
            {
                return HttpNotFound();
            }

            ViewBag.CourseId    = new SelectList(exercise.Author.Courses.Select(c => new { Id = c.Id, Name = c.Name + " " + c.Grade }), "Id", "Name", exercise.CourseId);
            ViewBag.value       = new SelectList(Enumerable.Range(1, 15), exercise.Value);
            ViewBag.type        = new SelectList(db.ExerciseTypes, "Id", "Name", exercise.TypeId);


            //------------------------------------------------------------------------------------------
            User hTeacher = db.Users.Find((int)this.Session["UserId"]);

            if (!courseid.HasValue)
            {
                //ViewBag.topics = new MultiSelectList(Enumerable.Empty<Topic>(), "Id", "Name");
                ViewBag.topics = new MultiSelectList(Enumerable.Empty<Topic>(), "Id", "Name");
            }
            else
            {
                Edition hLast = (from e in db.Editions where e.CourseId == courseid.Value orderby e.DateStart descending select e).FirstOrDefault();

                if (hLast != null)
                {
                    ViewBag.topics = new MultiSelectList(hLast.Topics.Select(t => new { Id = t.Id, Name = t.Name + ", " + t.Description }), "Id", "Name");
                }
                else
                {
                    ViewBag.topics = new MultiSelectList(Enumerable.Empty<Topic>(), "Id", "Name");
                }
            }

            return View(exercise);
        }

        // POST: Exercises/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Edit([Bind(Include = "Id,CourseId,TopicId,Name,Description")] Exercise exercise)
        {
            if (ModelState.IsValid)
            {
                db.Entry(exercise).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BinaryId = new SelectList(db.Binaries, "Id", "Id", exercise.BinaryId);
            ViewBag.CourseId = new SelectList(db.Courses, "Id", "Name", exercise.CourseId);
            //ViewBag.TopicId = new SelectList(db.Topics, "Id", "Name", exercise.TopicId);
            return View(exercise);
        }

        // GET: Exercises/Delete/5
        [CustomAuthorize(RoleType.Teacher, RoleType.Director)]
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

        // POST: Exercises/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Teacher, RoleType.Director)]
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
