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
    public class LessonsController : Controller
    {
        private AivEntities db = new AivEntities();

        // GET: Lessons
        public ActionResult Index()
        {
            var lessons = (from hL in db.Lessons select hL).ToList();

            return View(lessons);
        }

        // GET: Lessons/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Lesson lesson = db.Lessons.Find(id);

            if (lesson == null)
            {
                return HttpNotFound();
            }
            return View(lesson);
        }


        //Get
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Create(int? classid)
        {
            User hTeacher = db.Users.Find(Session.GetUser().Id);
            if(hTeacher == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var classes = from c in db.Classes where c.Edition.Course.Teachers.Select(t => t.Id).Contains(hTeacher.Id) select c;

            if (!classid.HasValue)
            {
                ViewBag.ClassId     = new SelectList(classes, "Id", "DisplayName");
                ViewBag.TeacherId   = new SelectList(classes.First().Edition.Course.Teachers, "Id", "DisplayName");
                ViewBag.topics      = new MultiSelectList(classes.First().Edition.Topics, "Id", "DisplayName");
                ViewBag.Students    = new MultiSelectList(classes.First().Students, "Id", "DisplayName");
            }
            else
            {
                Class selected      = db.Classes.Find(classid);

                if(selected == null)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                if (!selected.Edition.Course.Teachers.Select(t => t.Id).Contains(hTeacher.Id))
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
               
                var hTeachers       = selected.Edition.Course.Teachers.Select(t => new { Id = t.Id, Name = t.Name + " " + t.Surname });

                ViewBag.ClassId     = new SelectList(classes, "Id", "DisplayName", selected.Id);
                ViewBag.TeacherId   = new SelectList(hTeachers, "Id", "Name", hTeacher.Id);
                ViewBag.topics      = new MultiSelectList(selected.Edition.Topics, "Id", "DisplayName");
                ViewBag.students    = new MultiSelectList(selected.Students, "Id", "DisplayName");
            }
           
            return View();
        }

        // POST: Lessons/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Create([Bind(Include = "Id,ClassId,TeacherId,Date,Notes")] Lesson lesson, List<int> topics, List<int> students)
        {
            if (ModelState.IsValid)
            {                
                topics?.ForEach(t => lesson.Topics.Add(db.Topics.Find(t)));

                if (students != null)
                {
                    students.ForEach(s => lesson.Students.Add(db.Users.Find(s)));
                    lesson.ClassSize = (short)db.Classes.Find(lesson.ClassId).Students.Count();
                }
                

                db.Lessons.Add(lesson);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ClassId     = new SelectList(db.Classes, "Id", "Section", lesson.ClassId);
            ViewBag.TeacherId   = new SelectList(db.Users, "Id", "Name", lesson.TeacherId);            

            return View(lesson);
        }

        // GET: Lessons/Edit/5
        [CustomAuthorize(RoleType.Teacher, RoleType.Secretary)]
        public ActionResult Edit(int? id, int? classid)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Lesson lesson = db.Lessons.Find(id);
            if (lesson == null)
            {
                return HttpNotFound();
            }

            IEnumerable<int> hSelectedStudents;

            if (!classid.HasValue)
            {
                classid = lesson.Class.Id;
                hSelectedStudents = lesson.Students.Select(s => s.Id);
            }
            else
            {
                hSelectedStudents = Enumerable.Empty<int>();
            }

            var vClassStudents = from hU in db.Classes.Find(classid).Students select new { Id = hU.Id, Name = hU.Name + " " + hU.Surname };
            //var vToSelect      = from hS in lesson.Class.S where hS.c
            Class hClass = db.Classes.Find(classid);

            ViewBag.ClassId = new SelectList(db.Classes.Select(c => new { Id = c.Id, Name = c.Edition.Course.Name + " " + c.Edition.Course.Grade + c.Section }), "Id", "Name", classid);
            ViewBag.TeacherId = new SelectList(db.Users.Select(u => new { Id = u.Id, Name = u.Name + " " + u.Surname }), "Id", "Name", lesson.TeacherId);
            ViewBag.Students = new MultiSelectList(vClassStudents, "Id", "Name", hSelectedStudents);
            ViewBag.topics = new MultiSelectList(hClass.Edition.Topics.Select(t => new { Id = t.Id, Name = t.Name + ", " + t.Description }), "Id", "Name", lesson.Topics.Select(e => e.Id));

            return View(lesson);
        }

        // POST: Lessons/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Teacher, RoleType.Secretary)]
        public ActionResult Edit([Bind(Include = "Id,ClassId,TeacherId,Date,Notes")] Lesson lesson, List<int> students, List<int> topics)
        {
            if (ModelState.IsValid)
            {
                Lesson hLesson = db.Lessons.Find(lesson.Id);
                hLesson.Students.Clear();
                hLesson.Topics.Clear();

                if (students != null)
                {
                    var hStudents = students.Select(s => db.Users.Find(s));                    
                    hStudents.ToList().ForEach(s => hLesson.Students.Add(s));
                }

                if (topics != null)
                {
                    var hTopics = topics.Select(t => db.Topics.Find(t));
                    hTopics.ToList().ForEach(t => hLesson.Topics.Add(t));
                }


                hLesson.Notes = lesson.Notes;
                hLesson.Date = lesson.Date;
                hLesson.Teacher = db.Users.Find(lesson.TeacherId);
                hLesson.Class = db.Classes.Find(lesson.ClassId);
                hLesson.ClassSize = (short)hLesson.Class.Students.Count();
                hLesson.Frequency = (float)hLesson.Students.Count() / (float)hLesson.ClassSize;

                if (double.IsNaN(hLesson.Frequency))
                    hLesson.Frequency = 0.0;

                db.Entry(hLesson).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ClassId = new SelectList(db.Classes, "Id", "Section", lesson.ClassId);
            ViewBag.TeacherId = new SelectList(db.Users, "Id", "Name", lesson.TeacherId);
            return View(lesson);
        }

        // GET: Lessons/Delete/5
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Lesson lesson = db.Lessons.Find(id);
            if (lesson == null)
            {
                return HttpNotFound();
            }
            return View(lesson);
        }

        // POST: Lessons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult DeleteConfirmed(int id)
        {
            Lesson lesson = db.Lessons.Find(id);
            db.Lessons.Remove(lesson);
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
