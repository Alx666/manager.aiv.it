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
    public class NotesController : Controller
    {
        private AivEntities db = new AivEntities();

        [CustomAuthorize(RoleType.Teacher, RoleType.Bursar, RoleType.Secretary, RoleType.Admin, RoleType.Director, RoleType.Manager)]
        public ActionResult Index(int? studentId)
        {
            IEnumerable<Note> hNotes;

            if (studentId.HasValue)
            {
                hNotes = from hN in db.Notes where hN.Subject.Id == studentId select hN;
            }
            else
            {
                hNotes = from hN in db.Notes select hN;
            }

            
            return View(hNotes.ToList());
        }

        [CustomAuthorize(RoleType.Teacher, RoleType.Bursar, RoleType.Secretary, RoleType.Admin, RoleType.Director,RoleType.Manager)]
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

        [CustomAuthorize(RoleType.Teacher, RoleType.Bursar, RoleType.Secretary, RoleType.Admin, RoleType.Director, RoleType.Manager)]
        public ActionResult Create(int? studentId)
        {
            if (!studentId.HasValue)
                return HttpNotFound();

            ViewBag.StudentId = new SelectList(db.Users, "Id", "DisplayName", db.Users.Find(studentId).Id);

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Teacher, RoleType.Bursar, RoleType.Secretary, RoleType.Admin, RoleType.Director, RoleType.Manager)]
        public ActionResult Create([Bind(Include = "Id,StudentId,StaffId,Text")] Note note)
        {
            User hAuthor  = db.Users.Find(Session.GetUser().Id);
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

        [CustomAuthorize(RoleType.Teacher, RoleType.Bursar, RoleType.Secretary, RoleType.Admin, RoleType.Director, RoleType.Manager)]
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
        [CustomAuthorize(RoleType.Teacher, RoleType.Bursar, RoleType.Secretary, RoleType.Admin, RoleType.Director, RoleType.Manager)]
        public ActionResult DeleteConfirmed(int id)
        {
            Note note = db.Notes.Find(id);
            db.Notes.Remove(note);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [CustomAuthorize(RoleType.Teacher, RoleType.Bursar, RoleType.Secretary, RoleType.Admin, RoleType.Director, RoleType.Manager)]
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
