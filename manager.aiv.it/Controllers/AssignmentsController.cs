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
    public class AssignmentsController : Controller
    {
        private AivEntities db = new AivEntities();

        // GET: Assignments
        [CustomAuthorize(RoleType.Teacher, RoleType.Director, RoleType.Manager)]
        public ActionResult Index()
        {
            var result = db.Assignments.ToList().Select(a => new AssignmentViewModels(a));

            return View(result);
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
        public ActionResult Create(int? exerciseid)
        {
            User hUser = db.Users.Find((int)this.Session["UserId"]);

            var hClasses    =   from hClass in db.Classes where hClass.Edition.DateEnd > DateTime.Now || !hClass.Edition.DateEnd.HasValue
                                from hCourse in db.Courses where hCourse.Teachers.Select(t => t.Id).Contains(hUser.Id) where hClass.Edition.Course == hCourse
                                select new { Id = hClass.Id, Name = hClass.Edition.Course.Name + " " + hClass.Edition.Course.Grade + hClass.Section };


            var hExercises = from hExercise in db.Exercises
                             where hExercise.Course.Teachers.Select(t => t.Id).Contains(hUser.Id)
                             select new
                             {
                                 Id         = hExercise.Id,
                                 Name       = "(Grade " +  hExercise.Course.Grade + " " + hExercise.Value + "pts) " + hExercise.Name
                             };

            
            ViewBag.ClassId     = new SelectList(hClasses, "Id", "Name");
            ViewBag.ExerciseId  = new SelectList(hExercises, "Id", "Name");

            AssignmentViewModels hView;

            if (exerciseid.HasValue)
            {
                Exercise hSelected = db.Exercises.Find(exerciseid);
                hView = new AssignmentViewModels(hSelected);
            }
            else
                hView = new AssignmentViewModels(db.Exercises.Find(hExercises.First().Id));

            return View(hView);
        }


        // POST: Assignments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Create([Bind(Include = "ExerciseId,ClassId,Deadline,UnlockDate,Notes")] AssignmentViewModels assignment)
        {
            if (ModelState.IsValid)
            {
                Assignment hAssignment      = new Assignment();
                hAssignment.Exercise        = db.Exercises.Find(assignment.ExerciseId);
                hAssignment.Class           = db.Classes.Find(assignment.ClassId);
                hAssignment.Deadline        = assignment.Deadline;
                hAssignment.UnlockDate      = assignment.UnlockDate;
                hAssignment.ExerciseValue   = hAssignment.Exercise.Value;
                hAssignment.Teacher         = db.Users.Find(this.Session["UserId"]);
                hAssignment.Description     = assignment.Notes;

                db.Assignments.Add(hAssignment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }


            ViewBag.ClassId = new SelectList(db.Classes, "Id", "Section", assignment.ClassId);
            ViewBag.ExerciseId = new SelectList(db.Exercises, "Id", "Name", assignment.ExerciseId);
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
