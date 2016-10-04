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
using System.Data.Entity.Validation;
using System.Text;

namespace manager.aiv.it.Controllers
{
    public class ExercisesController : Controller
    {
        private AivEntities db = new AivEntities();

        [CustomAuthorize(RoleType.Teacher, RoleType.Manager, RoleType.Director)]
        public ActionResult Index()
        {
            return View(db.Exercises.Include(e => e.Author).Include(e => e.Course).Include(e => e.Type).ToList());
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
            
            ViewBag.CourseId = new SelectList(hTeacher.Courses, "Id", "DisplayName");
            ViewBag.value    = new SelectList(Enumerable.Range(1, 15));
            ViewBag.type     = new SelectList(db.ExerciseTypes, "Id", "Name");

            if (courseid.HasValue)
            {
                Edition hLast = (from e in db.Editions where e.CourseId == courseid.Value orderby e.DateStart descending select e).FirstOrDefault();

                if (hLast != null)
                    ViewBag.topics = new MultiSelectList(hLast.Topics, "Id", "DisplayName");
                else
                    ViewBag.topics = new MultiSelectList(Enumerable.Empty<Topic>(), "Id", "DisplayName");
            }
            else
            {
                ViewBag.topics = new MultiSelectList(Enumerable.Empty<Topic>(), "Id", "DisplayName");
            }
            
            return View();
        }

        // POST: Exercises/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Create([Bind(Include = "Id,CourseId,Name,Description,Value,TypeId")] Exercise exercise, List<int> topics)
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

            ViewBag.BinaryId = new SelectList(db.Binaries, "Id", "Id", exercise.BinaryId);
            ViewBag.CourseId = new SelectList(db.Courses, "Id", "Name", exercise.CourseId);

            return View(exercise);
        }

        // GET: Exercises/Edit/5
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Edit(int? id, int? courseid)
        {
            User hTeacher = db.Users.Find((int)this.Session["UserId"]);

            if (id == null || hTeacher == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Exercise exercise = db.Exercises.Find(id);
            if (exercise == null)
                return HttpNotFound();

            ViewBag.CourseId    = new SelectList(exercise.Author.Courses.Select(c => new { Id = c.Id, Name = c.Name + " " + c.Grade }), "Id", "Name", exercise.CourseId);
            ViewBag.value       = new SelectList(Enumerable.Range(1, 15), exercise.Value);
            ViewBag.TypeId      = new SelectList(db.ExerciseTypes, "Id", "Name", exercise.TypeId);

            Edition hLast;
            List<int> hSelected;
            if (courseid.HasValue)
            {
                hLast = (from e in db.Editions where e.CourseId == courseid.Value orderby e.DateStart descending select e).FirstOrDefault();
                hSelected = new List<int>();
            }
            else
            {
                hLast = (from e in exercise.Course.Editions orderby e.DateStart descending select e).FirstOrDefault();
                hSelected = exercise.Topics.Select(t => t.Id).ToList();
            }

            ViewBag.topics = new MultiSelectList(hLast.Topics, "Id", "DisplayName", hSelected);

            return View(exercise);
        }

        // POST: Exercises/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Edit([Bind(Include = "Id,CourseId,Name,Description,Value,TypeId")] Exercise exercise, List<int> topics)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Exercise hEdited = db.Exercises.Find(exercise.Id);

                    hEdited.Topics.Clear();

                    if (topics != null)
                        topics.ForEach(t => hEdited.Topics.Add(db.Topics.Find(t)));

                    hEdited.Name = exercise.Name;
                    hEdited.Description = exercise.Description;
                    hEdited.CourseId = exercise.CourseId;
                    hEdited.Value = exercise.Value;

                    db.Entry(hEdited).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException e)
                {
                    StringBuilder hSb = new StringBuilder();
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        hSb.AppendFormat("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            hSb.AppendFormat("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage);
                        }
                    }

                    string sMex = hSb.ToString();
                    throw;
                }
            }

            //ViewBag.BinaryId = new SelectList(db.Binaries, "Id", "Id", exercise.BinaryId);
            //ViewBag.CourseId = new SelectList(db.Courses, "Id", "Name", exercise.CourseId);
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
