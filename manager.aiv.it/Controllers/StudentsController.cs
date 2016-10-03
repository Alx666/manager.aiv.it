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
    public class StudentsController : Controller
    {
        private AivEntities db = new AivEntities();

        // GET: Students
        [CustomAuthorize(RoleType.Secretary)]
        public ActionResult Index()
        {
            var model = from r in db.Roles
                        from s in r.Users
                        where r.Id == (int)RoleType.Student
                        select new StudentViewModels()
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Surname = s.Surname,
                            Email = s.Email,
                            Mobile = s.Mobile,
                            Class = s.Class.Edition.Course.Name + " " + s.Class.Edition.Course.Grade + s.Class.Section
                        };

            return View(model.ToList());
        }

        // GET: Students/Details/5        
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            User user = db.Users.Find(id);

            StudentViewModels hUser = new StudentViewModels();        
            hUser.Id                = user.Id;
            hUser.Name              = user.Name;
            hUser.Surname           = user.Surname;
            hUser.Email             = user.Email;
            hUser.Mobile            = user.Mobile;

            if (user.Class != null)
            {
                hUser.Class = user.Class.Edition.Course.Name + " " + user.Class.Edition.Course.Grade + user.Class.Section;

                int iTotalLessons   = user.Class.Lessons.Where(l => l.Date < DateTime.Now).Count();
                int iPresences      = user.LessonsFollowed.Where(l => l.Class == user.Class).Count();
                hUser.Frequency     = string.Format("{0} / {1}", iPresences, iTotalLessons);


                hUser.MissedTopics = (from hL in user.Class.Lessons
                                      where !hL.Students.Contains(user)
                                      from hT in hL.Topics
                                      select hT.Name + ", " + hT.Description).ToList();

                hUser.Assignments  = user.Class.Assignments.Select(a => new AssignmentViewModels(a.UnlockDate, a.Deadline, a.Description, a.Exercise)).ToList();
            }
            else
            {
                hUser.Class         = string.Empty;
                hUser.Frequency     = string.Empty;
            }



            if (user == null)
            {
                return HttpNotFound();
            }
            return View(hUser);
        }

        // GET: Students/Create
        [CustomAuthorize(RoleType.Secretary)]
        public ActionResult Create()
        {
            ViewBag.ClassId = new SelectList(db.Classes.Select(c => new { Id = c.Id, Name = c.Edition.Course.Name + " " + c.Edition.Course.Grade + c.Section }), "Id", "Name");
            ViewBag.RoleId  = new SelectList(db.Roles, "Id", "Name");
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Secretary)]
        public ActionResult Create([Bind(Include = "Id,Name,Surname,Email,Password,Mobile,RegistrationDate,ClassId")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ClassId = new SelectList(db.Classes, "Id", "Section", user.ClassId);
            ViewBag.RoleId  = new SelectList(db.Roles, "Id", "Name");
            return View(user);
        }

        // GET: Students/Edit/5
        [CustomAuthorize(RoleType.Secretary)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            User user = db.Users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.ClassId = new SelectList(db.Classes.Select(c => new { Id = c.Id, Name = c.Edition.Course.Name + " " + c.Edition.Course.Grade + c.Section }), "Id", "Name", user.ClassId);
            ViewBag.RoleId  = new SelectList(db.Roles, "Id", "Name");
            return View(user);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Secretary)]
        public ActionResult Edit([Bind(Include = "Id,Name,Surname,Email,Password,Mobile,RegistrationDate,ClassId")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ClassId = new SelectList(db.Classes, "Id", "Section", user.ClassId);
            ViewBag.RoleId = new SelectList(db.Roles, "Id", "Name");
            return View(user);
        }

        // GET: Students/Delete/5
        [CustomAuthorize(RoleType.Secretary)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Secretary)]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
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
