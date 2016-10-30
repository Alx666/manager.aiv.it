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
using System.Data.Entity.Validation;
using System.Text;
using System.Text.RegularExpressions;

namespace manager.aiv.it.Controllers
{
    public class ExercisesController : Controller
    {
        private AivEntities db = new AivEntities();

        [CustomAuthorize(RoleType.Teacher, RoleType.Manager, RoleType.Director)]
        public ActionResult Index()
        {
            return View(db.Exercises.Include(e => e.Author).Include(e => e.Course).Include(e => e.Type).ToList());
        }

        [CustomAuthorize(RoleType.Teacher, RoleType.Manager, RoleType.Director)]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Exercise exercise = db.Exercises.Find(id);
            if (exercise == null)
            {
                return HttpNotFound();
            }
            return View(exercise);
        }

        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Create(int? courseid)
        {
            User hTeacher = db.Users.Find(Session.GetUser().Id);
            
            ViewBag.CourseId = new SelectList(hTeacher.Courses, "Id", "DisplayName");
            ViewBag.value    = new SelectList(Enumerable.Range(1, 15));
            ViewBag.TypeId   = new SelectList(db.ExerciseTypes, "Id", "Name");

            if (courseid.HasValue)
            {
                Edition hLast = (from e in db.Editions where e.CourseId == courseid.Value /*orderby e.DateStart descending*/ select e).FirstOrDefault();

                if (hLast != null)
                    ViewBag.topics = new MultiSelectList(hLast.Topics, "Id", "DisplayName");
                else
                    ViewBag.topics = new MultiSelectList(Enumerable.Empty<Topic>(), "Id", "DisplayName");
            }
            else
            {
                ViewBag.topics = new MultiSelectList(hTeacher.Courses.First().Editions.First().Topics, "Id", "DisplayName");
            }
            
            return View();
        }

        // POST: Exercises/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Create([Bind(Include = "CourseId,Name,Description,Value,TypeId,BinaryId")] Exercise exercise, IEnumerable<int> topics, HttpPostedFileBase upload)
        {
            User hAuthor = db.Users.Find(Session.GetUser().Id);
            
            if (ModelState.IsValid && hAuthor != null)
            {
                if (upload != null)
                {
                    Binary binaryFile = new Binary();
                    byte[] fileBytes = new byte[upload.InputStream.Length];
                    upload.InputStream.Read(fileBytes, 0, fileBytes.Length);
                    binaryFile.Data = fileBytes;
                   
                    string binaryPath = upload.FileName;
                    string[] pathParts = Regex.Split(binaryPath, @"(/)|(\\)");
                    string filename = pathParts[pathParts.Length - 1];
                    binaryFile.Filename = filename;

                    db.Binaries.Add(binaryFile);
                    db.SaveChanges();
                    /* 
                    Layer di validazione : se il file è stato effettivamente salvato, lo vado a cercare nel db 
                    per essere sicuro di non attribuire a "excercise" un BinaryId fasullo
                    */
                    Binary saved = db.Binaries.Find(binaryFile.Id);
                    if(saved != null)
                    {
                        exercise.Binary = saved;
                        exercise.BinaryId = saved.Id;
                    }
                }

                if (topics != null)
                    topics.ToList().ForEach(t => exercise.Topics.Add(db.Topics.Find(t)));

                exercise.Author = hAuthor;
                
                db.Exercises.Add(exercise);                
                db.SaveChanges();
                return RedirectToAction("Index");
            } 

            ViewBag.BinaryId = new SelectList(db.Binaries, "Id", "Id", exercise.BinaryId);
            ViewBag.CourseId = new SelectList(db.Courses, "Id", "Name", exercise.CourseId);

            return View(exercise);
        }

        // GET: Exercises/Edit/5
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Edit(int? id, int? courseid)
        {
            User hTeacher = db.Users.Find(Session.GetUser().Id);

            if (id == null || hTeacher == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Exercise exercise = db.Exercises.Find(id);
            if (exercise == null)
                return HttpNotFound();

            ViewBag.CourseId     = new SelectList(exercise.Author.Courses, "Id", "DisplayName", exercise.CourseId);
            ViewBag.Value        = new SelectList(Enumerable.Range(1, 15), (int)exercise.Value);
            ViewBag.TypeId       = new SelectList(db.ExerciseTypes, "Id", "Name", exercise.TypeId);

            Edition hLast;
            List<int> hSelected;
            if (courseid.HasValue)
            {
                hLast = (from e in db.Editions where e.CourseId == courseid.Value /*orderby e.DateStart descending*/ select e).FirstOrDefault();
                hSelected = new List<int>();
            }
            else
            {
                hLast = (from e in exercise.Course.Editions /*orderby e.DateStart descending*/ select e).FirstOrDefault();
                hSelected = exercise.Topics.Select(t => t.Id).ToList();
            }

            ViewBag.topics = new MultiSelectList(hLast.Topics, "Id", "DisplayName", hSelected);

            return View(exercise);
        }

        // POST: Exercises/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Edit([Bind(Include = "Id,CourseId,Name,Description,Value,TypeId")] Exercise exercise, List<int> topics, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {
                Exercise hEdited = db.Exercises.Find(exercise.Id);

                hEdited.Topics.Clear();

                if (upload != null)
                {
                    Binary binaryFile = new Binary();
                    byte[] fileBytes = new byte[upload.InputStream.Length];
                    upload.InputStream.Read(fileBytes, 0, fileBytes.Length);
                    binaryFile.Data = fileBytes;

                    string binaryPath = upload.FileName;
                    string[] pathParts = Regex.Split(binaryPath, @"(/)|(\\)");
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
                        hEdited.Binary = saved;
                        hEdited.BinaryId = saved.Id;
                    }
                }

                if (topics != null)
                    topics.ForEach(t => hEdited.Topics.Add(db.Topics.Find(t)));

                hEdited.Name = exercise.Name;
                hEdited.Description = exercise.Description;
                hEdited.CourseId = exercise.CourseId;
                hEdited.Value = exercise.Value;

                db.Entry(hEdited).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            //ViewBag.BinaryId = new SelectList(db.Binaries, "Id", "Id", exercise.BinaryId);
            //ViewBag.CourseId = new SelectList(db.Courses, "Id", "Name", exercise.CourseId);
            //ViewBag.TopicId = new SelectList(db.Topics, "Id", "Name", exercise.TopicId);
            return View(exercise);
        }

        // GET: Exercises/Delete/5
        [CustomAuthorize(RoleType.Teacher, RoleType.Director)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Exercise exercise = db.Exercises.Find(id);
            if (exercise == null)
            {
                return HttpNotFound();
            }
            return View(exercise);
        }

        // POST: Exercises/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Teacher, RoleType.Director)]
        public ActionResult DeleteConfirmed(int id)
        {
            Exercise exercise = db.Exercises.Find(id);
            db.Exercises.Remove(exercise);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        
        public FileContentResult Download(int BinaryId)
        {
            Binary foundFile = db.Binaries.Find(BinaryId);
            if(foundFile != null)
                return File(foundFile.Data, System.Net.Mime.MediaTypeNames.Application.Octet, foundFile.Filename);

            return null;
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
