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
using System.Text.RegularExpressions;

namespace manager.aiv.it.Controllers
{
    public class LessonsController : Controller
    {
        private AivEntities db = new AivEntities();

        // GET: Lessons
        [CustomAuthorize(RoleType.Admin, RoleType.Director, RoleType.Manager, RoleType.Secretary, RoleType.Teacher)]
        public ActionResult Index()
        {
            var lessons = (from hL in db.Lessons select hL).ToList().OrderByDescending(l => l.Date).ToList();

            return View(lessons);
        }

        // GET: Lessons/Details/5
        [CustomAuthorize(RoleType.Admin, RoleType.Director, RoleType.Manager, RoleType.Secretary, RoleType.Teacher, RoleType.Student)]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            Lesson lesson = db.Lessons.Find(id);
            foreach(var student in lesson.Students)
            {
                if (student.PictureId != null)
                    student.Picture = db.Binaries.Find(student.PictureId);
            }

            if (lesson == null)
            {
                return HttpNotFound();
            }

            //if (lesson.BinaryId != null)
            //    lesson.Binary = db.Binaries.Find(lesson.BinaryId);
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

            Lesson hLesson = new Lesson();
            hLesson.Date = DateTime.Now;
           
            return View(hLesson);
        }

        // POST: Lessons/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Create([Bind(Include = "Id,ClassId,TeacherId,Date,Notes")] Lesson lesson, List<int> topics, List<int> students, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {                
                topics?.ForEach(t => lesson.Topics.Add(db.Topics.Find(t)));

                if (students != null)
                {
                    students.ForEach(s => lesson.Students.Add(db.Users.Find(s)));
                    lesson.ClassSize = (short)db.Classes.Find(lesson.ClassId).Students.Count();
                    lesson.Frequency = (float)lesson.Students.Count() / (float)lesson.ClassSize;
                }
                if (upload != null)
                {
                    Binary binary = Binary.CreateFrom(upload, true);

                    db.Binaries.Add(binary);
                    db.SaveChanges();

                    lesson.BinaryId = binary.Id;
                }
                //if (upload != null)
                //{
                //    Binary binaryFile = new Binary();
                //    byte[] fileBytes = new byte[upload.InputStream.Length];
                //    upload.InputStream.Read(fileBytes, 0, fileBytes.Length);
                //    binaryFile.Data = fileBytes;

                //    string binaryPath = upload.FileName;
                //    string[] pathParts = Regex.Split(binaryPath, @"(/)|(\\)");
                //    string filename = pathParts[pathParts.Length - 1];
                //    binaryFile.Filename = filename;

                //    db.Binaries.Add(binaryFile);
                //    db.SaveChanges();
                //    /* 
                //    Layer di validazione : se il file è stato effettivamente salvato, lo vado a cercare nel db 
                //    per essere sicuro di non attribuire a "excercise" un BinaryId fasullo
                //    */
                //    Binary saved = db.Binaries.Find(binaryFile.Id);
                //    if (saved != null)
                //    {
                //        lesson.Binary = saved;
                //        lesson.BinaryId = saved.Id;
                //    }
                //}


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
        public ActionResult Edit([Bind(Include = "Id,ClassId,TeacherId,Date,Notes")] Lesson lesson, List<int> students, List<int> topics, HttpPostedFileBase upload)
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
                
                if (upload != null)
                {
                    Binary binary = Binary.CreateFrom(upload, true);

                    //RIMOZIONE SICURA
                    Binary hPrevious = db.Binaries.Find(hLesson.BinaryId);
                    if (hPrevious != null)
                    {
                        db.Binaries.Remove(hPrevious);
                    }
                    //RIMOZIONE SICURA

                    db.Binaries.Add(binary);
                    db.SaveChanges();

                    hLesson.BinaryId = binary.Id;
                }
                //if (upload != null)
                //{
                //    Binary binaryFile = new Binary();
                //    byte[] fileBytes = new byte[upload.InputStream.Length];
                //    upload.InputStream.Read(fileBytes, 0, fileBytes.Length);
                //    binaryFile.Data = fileBytes;

                //    string binaryPath = upload.FileName;
                //    string[] pathParts = Regex.Split(binaryPath, @"(/)|(\\)");
                //    string filename = pathParts[pathParts.Length - 1];
                //    binaryFile.Filename = filename;

                //    db.Binaries.Add(binaryFile);
                //    db.SaveChanges();
                //    /* 
                //    Layer di validazione : se il file è stato effettivamente salvato, lo vado a cercare nel db 
                //    per essere sicuro di non attribuire a "excercise" un BinaryId fasullo
                //    */
                //    Binary saved = db.Binaries.Find(binaryFile.Id);
                //    if (saved != null)
                //    {
                //        lesson.Binary = saved;
                //        lesson.BinaryId = saved.Id;
                //    }
                //}

                hLesson.Notes = lesson.Notes;
                hLesson.Teacher = db.Users.Find(lesson.TeacherId);
                hLesson.Class = db.Classes.Find(lesson.ClassId);
                hLesson.ClassSize = (short)hLesson.Class.Students.Count();
                hLesson.Frequency = (float)hLesson.Students.Count() / (float)hLesson.ClassSize;
                hLesson.Date = lesson.Date;

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
        
        public FileContentResult Download(int BinaryId)
        {
            Binary foundFile = db.Binaries.Find(BinaryId);
            if (foundFile != null)
                return File(foundFile.Data, System.Net.Mime.MediaTypeNames.Application.Octet, foundFile.Filename);

            return null;
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

            if (lesson.BinaryId != null)
                db.Binaries.Remove(db.Binaries.Find(lesson.BinaryId));

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
