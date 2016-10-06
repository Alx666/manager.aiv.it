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
            return View(db.Assignments.Include(x => x.Exercise).ToList());
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
        //TODO: gli esercizi proposti vengono selezionati in base ad alcuni criteri
        //1) esercizi non già assegnati alla classe
        //2) tutti gli esercizi per il corso della classe (ma di tutti i livelli, per esempio programmazione 3 può fare gli esercizi di programmazione 1 e 2 ma non il contrario, discriminare in base al Course.Name e Course.Grade)
        //3) il docente deve essere abilitato all'assegnazione degli esercizi di un corso
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Create(int? exerciseid)
        {
            User hUser = db.Users.Find((int)this.Session["UserId"]);

            var hClasses    =   from hClass in db.Classes  where hClass.Edition.DateEnd > DateTime.Now
                                from hCourse in db.Courses where hCourse.Teachers.Select(t => t.Id).Contains(hUser.Id) && hClass.Edition.Course == hCourse
                                select hClass;


            var hExercises  =   from hExercise in db.Exercises
                                where hExercise.Course.Teachers.Select(t => t.Id).Contains(hUser.Id)
                                select hExercise;


            ViewBag.ClassId     = new SelectList(hClasses, "Id", "DisplayName");
            ViewBag.ExerciseId  = new SelectList(hExercises, "Id", "Name");
            

            Assignment hAssignment = new Assignment();
            

            if (exerciseid.HasValue)
            {
                hAssignment.Exercise = db.Exercises.Find(exerciseid);
            }
            else
                hAssignment.Exercise = db.Exercises.Find(hExercises.First().Id);

            ViewBag.topics = new MultiSelectList(hAssignment.Exercise.Topics, "Id", "DisplayName");

            return View(hAssignment);
        }


        // POST: Assignments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Create([Bind(Include = "ExerciseId,ClassId,Deadline,UnlockDate,Description")] Assignment assignment)
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
                hAssignment.Description     = assignment.Description;

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
        public ActionResult Edit(int? id, int? exerciseid)
        {
            User hUser = db.Users.Find((int)this.Session["UserId"]);

            var hClasses = from hClass in db.Classes
                           where hClass.Edition.DateEnd > DateTime.Now
                           from hCourse in db.Courses
                           where hCourse.Teachers.Select(t => t.Id).Contains(hUser.Id) && hClass.Edition.Course == hCourse
                           select hClass;


            var hExercises = from hExercise in db.Exercises
                             where hExercise.Course.Teachers.Select(t => t.Id).Contains(hUser.Id)
                             select hExercise;


            ViewBag.ClassId = new SelectList(hClasses, "Id", "DisplayName");
            ViewBag.ExerciseId = new SelectList(hExercises, "Id", "Name");


            Assignment hAssignment = new Assignment();


            if (exerciseid.HasValue)
            {
                hAssignment.Exercise = db.Exercises.Find(exerciseid);
            }
            else
                hAssignment.Exercise = db.Exercises.Find(hExercises.First().Id);

            ViewBag.topics = new MultiSelectList(hAssignment.Exercise.Topics, "Id", "DisplayName");

            return View(hAssignment);
        }

        // POST: Assignments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Edit([Bind(Include = "Id,ClassId,ExerciseId,UnlockDate,Deadline,Description,TeacherId")] Assignment assignment)
        {
            if (ModelState.IsValid)
            {
                assignment.Teacher = db.Users.Find((int)this.Session["UserId"]);

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
