using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using System.Text.RegularExpressions;
using manager.aiv.it;
using manager.aiv.it.Models;

namespace manager.aiv.it.Controllers
{
    public class StudentsController : Controller
    {
        private AivManagementEntities db = new AivManagementEntities();

        // GET: Students
        [CustomAuthorize(RoleType.Secretary, RoleType.Admin, RoleType.Bursar, RoleType.Director, RoleType.Manager, RoleType.Teacher)]
        public ActionResult Index(string search, int? searchId)
        {
            search = search ?? string.Empty;

            var hSearchTypes    = from t in Enum.GetValues(typeof(StudentsSearchType)) as StudentsSearchType[] select new { Id = (int)t, Name = Enum.GetName(typeof(StudentsSearchType), t) };            

            string[] hKeywords  = search.ToKeywordsArray();

            var hStudents       = from t in db.Roles.Find((int)RoleType.Student).Users select new { Student = t, Name = t.DisplayName.ToLower() };

            //Main query
            var model           = from s in hStudents
                                  where hKeywords.All(kw => s.Name.Contains(kw))
                                  select s;

            if (searchId.HasValue)
            {
                StudentsSearchType eType = (StudentsSearchType)searchId.Value;

                if (eType == StudentsSearchType.Enlisted)
                {
                    model = from s in model where s.Student.ClassId != null orderby s.Student.ClassId select s;
                }
                else if (eType == StudentsSearchType.WithNotes)
                {
                    model = from s in model where s.Student.NotesReceived.Count() > 0 select s;
                }

                ViewBag.SearchId = new SelectList(hSearchTypes, "Id", "Name", searchId.Value);
            }
            else
            {
                ViewBag.SearchId = new SelectList(hSearchTypes, "Id", "Name", null);
            }



            var result = from s in model
                         join x in db.ViewStudentFullDatas
                         on s.Student.Id equals x.Id
                         select x;

            return View(result);
        }

        // GET: Students/Details/5        
        [CustomAuthorize(RoleType.Secretary, RoleType.Admin, RoleType.Bursar, RoleType.Director, RoleType.Manager, RoleType.Teacher, RoleType.Student)]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            User user = db.Users.Find(id);
            User hLogged = Session.GetUser();


            if (user == null)
            {
                return HttpNotFound();
            }
            else if ((hLogged.IsOnly(RoleType.Student) && hLogged.Id != user.Id))
            {
                return Redirect(Request.UrlReferrer.ToString());
            }


            if (user.BinaryId != null)
            {
                Binary picture = db.Binaries.Find(user.BinaryId);
                user.Picture = picture;
            }

            return View(user);
        }

        // GET: Students/Create
        [CustomAuthorize(RoleType.Secretary)]
        public ActionResult Create()
        {
            ViewBag.ClassId = new SelectList(db.Classes.Where(c => !c.IsClosed).Include(c => c.Edition), "Id", "DisplayName");
            ViewBag.RoleId = new SelectList(db.Roles, "Id", "Name");
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Secretary)]
        public ActionResult Create([Bind(Include = "Name,Surname,Email,Password,Mobile,RegistrationDate,ClassId,MissedLessons,City,Address,Code")] User user)
        {
            if (ModelState.IsValid)
            {
                User hLogged = Session.GetUser();
                if (!string.IsNullOrEmpty(user.Email))
                {
                    User hAlreadyPresent = (from u in db.Users where u.Email == user.Email select u).FirstOrDefault();

                    if (hAlreadyPresent != null && hLogged.Id != hAlreadyPresent.Id)
                        throw new HttpException("User Already Present");
                }

                Role hRole = (from r in db.Roles where r.Id == (int)RoleType.Student select r).First();

                user.RegistrationDate = DateTime.Now.Date;
                user.Roles.Add(hRole);
                db.Users.Add(user);

                db.SaveChanges();

                EventLog.Log(db, hLogged, EventLogType.StudentCreated, $"Created Student {user.DisplayName}", true);

                return RedirectToAction("Index");
            }

            ViewBag.ClassId = new SelectList(db.Classes.Where(c => !c.IsClosed).Include(c => c.Edition), "Id", "Section", user.ClassId);
            ViewBag.RoleId = new SelectList(db.Roles, "Id", "Name");
            return View(user);
        }

        // GET: Students/Edit/5
        [CustomAuthorize(RoleType.Secretary, RoleType.Student)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            User user = db.Users.Find(id);
            User hLogged = Session.GetUser();

            if (user == null)
            {
                return HttpNotFound();
            }
            else if ((hLogged.IsOnly(RoleType.Student) && hLogged.Id != user.Id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            ViewBag.ClassId = new SelectList(db.Classes.Where(c => !c.IsClosed).Include(c => c.Edition), "Id", "DisplayName", user.ClassId);
            ViewBag.RoleId = new SelectList(db.Roles, "Id", "Name");

            if (user.BinaryId != null)
            {
                Binary picture = db.Binaries.Find(user.BinaryId);
                user.Picture = picture;
            }

            return View(user);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Secretary)]
        public ActionResult Edit([Bind(Include = "Id,Name,Surname,Email,Password,Mobile,RegistrationDate,ClassId,BinaryId,City,Address,Code")] User user)
        {            
            if (!string.IsNullOrEmpty(user.Email))
            {
                User hAlreadyPresent = (from u in db.Users where u.Email == user.Email select u).FirstOrDefault();

                if (hAlreadyPresent != null && user.Id != hAlreadyPresent.Id)
                    throw new HttpException("User Already Present");
            }


            User hLogged = Session.GetUser();

            if (hLogged.IsOnly(RoleType.Student) && hLogged.Id == user.Id)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            User hToEdit = db.Users.Find(user.Id);
            if (hToEdit == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            hToEdit.Name                = user.Name;
            hToEdit.Surname             = user.Surname;
            hToEdit.Email               = user.Email;
            hToEdit.Password            = user.Password;
            hToEdit.Mobile              = user.Mobile;
            hToEdit.City                = user.City;
            hToEdit.Address             = user.Address;
            hToEdit.Code                = user.Code;
            hToEdit.RegistrationDate    = DateTime.Now.Date;
            hToEdit.ClassId             = user.ClassId;

            

            ViewBag.ClassId = new SelectList(db.Classes, "Id", "Section", user.ClassId);
            ViewBag.RoleId  = new SelectList(db.Roles, "Id", "Name");

            db.SaveChanges();

            EventLog.Log(db, hLogged, EventLogType.StudentEdited, $"Edited Student {user.DisplayName}", true);

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Secretary, RoleType.Student)]
        public ActionResult ChangePicture(int userId, HttpPostedFileBase picture)
        {
            User user = db.Users.Find(userId);
            User hLogged = Session.GetUser();

            if (hLogged.Id != user.Id)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (user != null && picture != null)
            {
                Binary binaryFile = new Binary();
                byte[] fileBytes = new byte[picture.InputStream.Length];
                picture.InputStream.Read(fileBytes, 0, fileBytes.Length);
                binaryFile.Data = fileBytes;

                string picturePath = picture.FileName;
                string[] pathParts = Regex.Split(picturePath, @"(/)|(\\)");
                string filename = pathParts[pathParts.Length - 1];

                binaryFile.Filename = filename;
                db.Binaries.Add(binaryFile);
                db.SaveChanges();
                /* 
                Layer di validazione : se il file è stato effettivamente salvato, lo vado a cercare nel db 
                per essere sicuro di non attribuire a "excercise" un BinaryId fasullo
                */
                Binary saved = db.Binaries.Find(binaryFile.Id);
                if (saved != null)
                {
                    user.BinaryId = saved.Id;
                    user.Picture = saved;
                    db.Entry(user).State = EntityState.Modified;
                }
            }

            db.SaveChanges();
            Session.LoadUser(user);

            return View("Details", user);
        }

        public FileContentResult DownloadPicture(int PictureId)
        {
            Binary foundFile = db.Binaries.Find(PictureId);
            if (foundFile != null)
                return File(foundFile.Data, System.Net.Mime.MediaTypeNames.Application.Octet, foundFile.Filename);

            return null;
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
            User hLogged = Session.GetUser();
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();

            EventLog.Log(db, hLogged, EventLogType.StudentDeleted, $"Deleted Student {user.DisplayName}", true);

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
