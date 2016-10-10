﻿using System;
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
    public class NotesController : Controller
    {
        private AivEntities db = new AivEntities();

        [CustomAuthorize(RoleType.Teacher, RoleType.Bursar, RoleType.Secretary)]
        public ActionResult Index()
        {
            var notes = db.Notes.Include(n => n.Author).Include(n => n.Subject);
            return View(notes.ToList());
        }

        [CustomAuthorize(RoleType.Teacher, RoleType.Bursar, RoleType.Secretary)]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Note note = db.Notes.Find(id);
            if (note == null)
            {
                return HttpNotFound();
            }
            return View(note);
        }

        [CustomAuthorize(RoleType.Teacher, RoleType.Bursar, RoleType.Secretary)]
        public ActionResult Create(int? studentId)
        {
            if (!studentId.HasValue)
                return HttpNotFound();

            ViewBag.StudentId = new SelectList(db.Users, "Id", "DisplayName", db.Users.Find(studentId).Id);

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Teacher, RoleType.Bursar, RoleType.Secretary)]
        public ActionResult Create([Bind(Include = "Id,StudentId,StaffId,Text")] Note note)
        {
            User hAuthor  = db.Users.Find((int)Session["UserId"]);
            User hStudent = db.Users.Find(note.StudentId);

            if(hAuthor == null || hStudent == null)
                return HttpNotFound();


            if (ModelState.IsValid)
            {
                Note hNew = new Note();
                hNew.Date = DateTime.Now;
                hNew.Author = hAuthor;
                hNew.Subject = hStudent;
                hNew.Text = note.Text;


                db.Notes.Add(hNew);
                db.SaveChanges();
                return RedirectToAction("Index", "Students");
            }

            return View(note);
        }

        //[CustomAuthorize(RoleType.Teacher, RoleType.Bursar, RoleType.Secretary)]
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Note note = db.Notes.Find(id);
        //    if (note == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.StaffId = new SelectList(db.Users, "Id", "Name", note.StaffId);
        //    ViewBag.StudentId = new SelectList(db.Users, "Id", "Name", note.StudentId);
        //    return View(note);
        //}


        
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[CustomAuthorize(RoleType.Teacher, RoleType.Bursar, RoleType.Secretary)]
        //public ActionResult Edit([Bind(Include = "Id,StudentId,StaffId,Text,Date")] Note note)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(note).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.StaffId = new SelectList(db.Users, "Id", "Name", note.StaffId);
        //    ViewBag.StudentId = new SelectList(db.Users, "Id", "Name", note.StudentId);
        //    return View(note);
        //}

        [CustomAuthorize(RoleType.Teacher, RoleType.Bursar, RoleType.Secretary)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Note note = db.Notes.Find(id);
            if (note == null)
            {
                return HttpNotFound();
            }
            return View(note);
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Teacher, RoleType.Bursar, RoleType.Secretary)]
        public ActionResult DeleteConfirmed(int id)
        {
            Note note = db.Notes.Find(id);
            db.Notes.Remove(note);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [CustomAuthorize(RoleType.Teacher, RoleType.Bursar, RoleType.Secretary)]
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
