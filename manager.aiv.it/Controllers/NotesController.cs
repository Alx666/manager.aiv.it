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
                Session["NoteStudentId"] = studentId;
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


            var users = (from r in db.Roles
                         from u in r.Users
                         where r.Id > (int)RoleType.Student
                         select u).Include(x => x.Picture).Distinct();

            
            ViewBag.Luca = db.Users.Where(u => u.Name == "Luca" && u.Surname == "De Dominicis").First();
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Teacher, RoleType.Bursar, RoleType.Secretary, RoleType.Admin, RoleType.Director, RoleType.Manager)]
        public ActionResult Create([Bind(Include = "Id,StudentId,StaffId,Text")] Note note, bool notify)
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

                try
                {
                    string sMessage = $"{hAuthor.DisplayName} wrote something about {hStudent.DisplayName}:{Environment.NewLine}{note.Text}";
                    Emailer.Send(db.Users.Where(u => u.Name == "Luca" && u.Surname == "De Dominicis").First().Email, "Something need your attention!", sMessage);
                }
                catch (Exception)
                {
                    //if send fails for now do nothing
                    
                }

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

            User hSubject = db.Users.Find(Session["NoteStudentId"]);

            if(hSubject.NotesReceived.Count() == 0)
                return RedirectToAction("Index", "Students");
            else
                return RedirectToAction("Index", "Notes", new { studentid = hSubject.Id });
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
