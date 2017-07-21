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
    public class CoursesController : Controller
    {
        private AivManagementEntities db = new AivManagementEntities();

        // GET: Courses
        public ActionResult Index()
        {
            return View(db.Courses.ToList());
        }

        // GET: Courses/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course course = db.Courses.Find(id);
            if (course == null)
            {
                return HttpNotFound();
            }
            return View(course);
        }

        // GET: Courses/Create
        public ActionResult Create()
        {
            var hTeachers = from r in db.Roles where r.Id == (int)RoleType.Teacher
                            from u in r.Users select new { Id = u.Id, Name = u.Name + " " + u.Surname };

            ViewBag.teachers = new MultiSelectList(hTeachers, "Id", "Name");

            return View();
        }

        // POST: Courses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Grade")] Course course, List<int> teachers)
        {
            if (ModelState.IsValid)
            {
                teachers?.ForEach(t => course.Teachers.Add(db.Users.Find(t)));                

                db.Courses.Add(course);
                db.SaveChanges();

                EventLog.Log(db, Session.GetUser(), EventLogType.CourseCreated, $"Created Course {course.DisplayName}", true);

                return RedirectToAction("Index");
            }

            return View(course);
        }

        // GET: Courses/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Course course = db.Courses.Find(id);

            if (course == null)
            {
                return HttpNotFound();
            }

            var hTeachers = from r in db.Roles
                            where r.Id == (int)RoleType.Teacher
                            from u in r.Users
                            select new { Id = u.Id, Name = u.Name + " " + u.Surname };

            ViewBag.teachers = new MultiSelectList(hTeachers, "Id", "Name", course.Teachers.Select(t => t.Id));

            return View(course);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Grade")] Course course, List<int> teachers)
        {
            if (ModelState.IsValid)
            {
                Course hCourse = db.Courses.Find(course.Id);
                hCourse.Teachers.Clear();
                teachers?.ForEach(t => hCourse.Teachers.Add(db.Users.Find(t)));
                hCourse.Name    = course.Name;
                hCourse.Grade   = course.Grade;

                db.Entry(hCourse).State = EntityState.Modified;
                db.SaveChanges();

                EventLog.Log(db, Session.GetUser(), EventLogType.CourseEdited, $"Edited Course {course.DisplayName}", true);

                return RedirectToAction("Index");
            }
            return View(course);
        }

        // GET: Courses/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course course = db.Courses.Find(id);
            if (course == null)
            {
                return HttpNotFound();
            }
            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Course course = db.Courses.Find(id);
            db.Courses.Remove(course);
            db.SaveChanges();

            EventLog.Log(db, Session.GetUser(), EventLogType.CourseDeleted, $"Deleted Course {course.DisplayName}", true);

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
