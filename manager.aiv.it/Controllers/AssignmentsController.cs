﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using manager.aiv.it;
using manager.aiv.it.Models;
using System.Text;

namespace manager.aiv.it.Controllers
{
    public class AssignmentsController : Controller
    {        
        private AivManagementEntities db = new AivManagementEntities();

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
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Create(int? exerciseid)
        {
            User hUser = db.Users.Find(Session.GetUser().Id);

            var hClasses    =   from hClass in db.Classes  where hClass.Edition.DateEnd > DateTime.Now
                                from hCourse in db.Courses where hCourse.Teachers.Select(t => t.Id).Contains(hUser.Id) && hClass.Edition.Course == hCourse
                                select hClass;


            var hExercises  =   from hExercise in db.Exercises
                                where hExercise.Course.Teachers.Select(t => t.Id).Contains(hUser.Id)
                                select hExercise;


            ViewBag.ClassId     = new SelectList(hClasses, "Id", "DisplayName");
            ViewBag.ExerciseId  = new SelectList(hExercises, "Id", "Name");
            

            Assignment hAssignment = new Assignment();
            hAssignment.UnlockDate = DateTime.Now;
            hAssignment.Deadline = DateTime.Now;

            if (exerciseid.HasValue)
            {
                hAssignment.Exercise = db.Exercises.Find(exerciseid);
            }
            else
            {
                if(hExercises.First() != null)
                {
                    hAssignment.Exercise = db.Exercises.Find(hExercises.First().Id);
                }
            }

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
                hAssignment.Teacher         = db.Users.Find(Session.GetUser().Id);
                hAssignment.Description     = assignment.Description;

                db.Assignments.Add(hAssignment);
                db.SaveChanges();

                //Send Email to all the users for this class
                List<User> hUsers   = db.Classes.Find(assignment.ClassId).ActiveStudents.ToList();

                Emailer.Send(hUsers, "New Homework starting on " + assignment.UnlockDate.ToShortDateString(), string.Empty);

                return RedirectToAction("Index");
            }
                                    
            ViewBag.ClassId     = new SelectList(db.Classes, "Id", "Section", assignment.ClassId);
            ViewBag.ExerciseId  = new SelectList(db.Exercises, "Id", "Name", assignment.ExerciseId);


            return View(assignment);
        }



        // GET: Assignments/Edit/5
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Edit(int? id, int? exerciseid)
        {
            User hUser = db.Users.Find(Session.GetUser().Id);

            var hClasses    = from hClass in db.Classes
                              where hClass.Edition.DateEnd > DateTime.Now
                              from hCourse in db.Courses
                              where hCourse.Teachers.Select(t => t.Id).Contains(hUser.Id) && hClass.Edition.Course == hCourse
                              select hClass;


            var hExercises  = from hExercise in db.Exercises
                              where hExercise.Course.Teachers.Select(t => t.Id).Contains(hUser.Id)
                              select hExercise;


            ViewBag.ExerciseId = new SelectList(hExercises, "Id", "Name");


            Assignment hAssignment = db.Assignments.Find(id);

            if (exerciseid.HasValue)
            {
                hAssignment.Exercise = db.Exercises.Find(exerciseid);
            }
            else
                hAssignment.Exercise = db.Exercises.Find(hExercises.First().Id);

            ViewBag.ClassId = new SelectList(hClasses, "Id", "DisplayName", hAssignment.ClassId);

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
                Assignment hPrevious = db.Assignments.Find(assignment.Id);

                hPrevious.UnlockDate = assignment.UnlockDate;
                hPrevious.Deadline = assignment.Deadline;
                hPrevious.Class = db.Classes.Find(assignment.ClassId);
                hPrevious.Description = assignment.Description;
                hPrevious.TeacherId = assignment.TeacherId;
                hPrevious.Teacher = db.Users.Find(Session.GetUser().Id);
                hPrevious.Exercise = db.Exercises.Find(assignment.ExerciseId);

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
