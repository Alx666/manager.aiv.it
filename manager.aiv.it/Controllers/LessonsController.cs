using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;


namespace manager.aiv.it.Controllers
{
    public class LessonsController : Controller
    {
        private AivManagementEntities db = new AivManagementEntities();

        // GET: Lessons
        [CustomAuthorize(RoleType.Admin, RoleType.Director, RoleType.Manager, RoleType.Secretary, RoleType.Teacher)]
        public ActionResult Index(string search, int? searchId)
        {
            User hUser = db.Users.Find((int)Session.GetUser().Id);

            var hSearchTypes = from t in Enum.GetValues(typeof(LessonsSearchType)) as LessonsSearchType[] select new { Id = (int)t, Name = Enum.GetName(typeof(LessonsSearchType), t) };
            int? vSelectedvalue = null;

            //defaults lessons of interest for the current teacher
            IEnumerable<ViewLessonFullData> hLessonsView = db.ViewLessonFullDatas;
            IEnumerable<Lesson> hLessons;

            if (searchId.HasValue && search != null)
            {
                string[] hKeywords = search.ToKeywordsArray();

                LessonsSearchType eType = (LessonsSearchType)searchId.Value;

                if (eType == LessonsSearchType.Teacher)
                {
                    var hSearchSet = from t in db.Roles.Find((int)RoleType.Teacher).Users select new { Teacher = t, Name = t.DisplayName.ToLower() };

                    hLessons = (from t in hSearchSet
                                where hKeywords.All(kw => t.Name.Contains(kw))
                                select t.Teacher).SelectMany(t => t.LessonsTeached);
                }
                else if (eType == LessonsSearchType.Student)
                {
                    var hSearchSet = from t in db.Roles.Find((int)RoleType.Student).Users select new { Student = t, Name = t.DisplayName.ToLower() };

                    hLessons = (from t in hSearchSet
                                where hKeywords.All(kw => t.Name.Contains(kw))
                                select t.Student).SelectMany(s => s.Attendings.Select(x => x.Lesson));
                }
                else if (eType == LessonsSearchType.Class)
                {
                    var hSearchSet = db.Classes.ToList().Select(c => new { Class = c, Name = c.DisplayName.ToLower() });

                    hLessons = (from s in hSearchSet
                                where hKeywords.All(kw => s.Name.Contains(kw))
                                select s.Class).SelectMany(c => c.Lessons);
                }
                else if (eType == LessonsSearchType.Course)
                {
                    var hSearchSet = db.Courses.ToList().Select(c => new { Course = c, Name = c.DisplayName.ToLower() }); //Can't invoke DisplayName on entity model

                    hLessons = (from c in hSearchSet
                                from l in db.Lessons
                                where l.Class.Edition.Course == c.Course && hKeywords.All(kw => c.Name.Contains(kw))
                                select l).Distinct();
                }
                else if (eType == LessonsSearchType.Note)
                {
                    hLessons = from l in db.Lessons
                               where hKeywords.All(kw => l.Notes.Contains(kw))
                               select l;
                }
                else if (eType == LessonsSearchType.Topic)
                {
                    var hSearchSet = db.Topics.ToList().Select(t => new { Topic = t, Name = t.DisplayName.ToLower() }); //Can't invoke DisplayName on entity model

                    hLessons = from t in hSearchSet
                               from l in t.Topic.Lessons
                               where hKeywords.All(kw => t.Name.Contains(kw))
                               select l;
                }
                else
                {
                    //search everything everywhere
                    hLessons = (from l in db.Lessons.Include(x => x.Class.Edition.Course)
                                from t in l.Topics
                                from x in hKeywords
                                where
                                    l.Notes.Contains(x) ||
                                    t.Name.Contains(x) ||
                                    t.Description.Contains(x) 
                                select l).DistinctBy(l => l.Id);
                }

                vSelectedvalue = searchId.Value;

                hLessonsView = from l in hLessons
                               join v in hLessonsView
                               on l.Id equals v.Id
                               select v;

                ViewBag.SearchId = new SelectList(hSearchTypes, "Id", "Name", vSelectedvalue);
            }
            else
            {
                ViewBag.SearchId = new SelectList(hSearchTypes, "Id", "Name", null);
            }



            return View(hLessonsView.GroupBy(l => l.Date).OrderByDescending(l => l.Key));
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
            foreach (var student in lesson.Attendings.Select(x => x.Student))
            {
                if (student.BinaryId != null)
                    student.Picture = db.Binaries.Find(student.BinaryId);
            }

            if (lesson == null)
            {
                return HttpNotFound();
            }

            if (lesson.BinaryId != null)
                lesson.Binary = db.Binaries.Find(lesson.BinaryId);
            return View(lesson);
        }


        //Get
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Create(int? classid)
        {
            User hTeacher = db.Users.Find(Session.GetUser().Id);
            if (hTeacher == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var classes = from c in db.Classes where c.Edition.Course.Teachers.Select(t => t.Id).Contains(hTeacher.Id) && !c.IsClosed select c;

            if (!classid.HasValue)
            {
                ViewBag.ClassId = new SelectList(classes, "Id", "DisplayName");
                ViewBag.TeacherId = new SelectList(classes.First().Edition.Course.Teachers, "Id", "DisplayName");
                ViewBag.topics = new MultiSelectList(classes.First().Edition.Topics, "Id", "DisplayName");
                ViewBag.Students = new MultiSelectList(classes.First().ActiveStudents, "Id", "DisplayName");
            }
            else
            {
                Class selected = db.Classes.Find(classid);

                if (selected == null)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                if (!selected.Edition.Course.Teachers.Select(t => t.Id).Contains(hTeacher.Id))
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                var hTeachers = selected.Edition.Course.Teachers.Select(t => new { Id = t.Id, Name = t.Name + " " + t.Surname });

                ViewBag.ClassId     = new SelectList(classes, "Id", "DisplayName", selected.Id);
                ViewBag.TeacherId   = new SelectList(hTeachers, "Id", "Name", hTeacher.Id);
                ViewBag.topics      = new MultiSelectList(selected.Edition.Topics.OrderBy(t => t.Name).ThenBy(t => t.Description), "Id", "DisplayName");
                ViewBag.students    = new MultiSelectList(selected.ActiveStudents, "Id", "DisplayName");
            }

            Lesson hLesson          = new Lesson();
            hLesson.Date            = DateTime.Now;

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
            User hTeacher = db.Users.Find(Session.GetUser().Id);

            Lesson already = (from l in db.Lessons
                              where l.Teacher.Id == hTeacher.Id &&
                              DbFunctions.TruncateTime(l.Date) == DbFunctions.TruncateTime(DateTime.Now) &&
                              l.ClassId == lesson.ClassId
                              select l).FirstOrDefault();

            if (already != null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (ModelState.IsValid)
            {
                Class hClass = db.Classes.Find(lesson.ClassId);
                if (hClass.IsClosed)
                    throw new ApplicationException("this class is closed");

                topics?.ForEach(t => lesson.Topics.Add(db.Topics.Find(t)));


                if (upload != null)
                {
                    Binary binary = Binary.CreateFrom(upload, true);

                    db.Binaries.Add(binary);
                    db.SaveChanges();

                    lesson.BinaryId = binary.Id;
                }


                db.Lessons.Add(lesson);
                db.SaveChanges();

                EventLog.Log(db, hTeacher, EventLogType.LessonCreated, $"Created Lesson for {hClass.DisplayName}", true);

                return RedirectToAction("Index");
            }

            ViewBag.ClassId = new SelectList(db.Classes, "Id", "Section", lesson.ClassId);
            ViewBag.TeacherId = new SelectList(db.Users, "Id", "Name", lesson.TeacherId);

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

            if (classid.HasValue)
            {                
                hSelectedStudents = lesson.Attendings.Select(s => s.StudentId);
            }
            else
            {
                hSelectedStudents = Enumerable.Empty<int>();
            }

            var vClassStudents = from hU in db.Classes.Find(classid).ActiveStudents select new { Id = hU.Id, Name = hU.Name + " " + hU.Surname };


            User hUser = db.Users.Find(Session.GetUser().Id);
            if (hUser == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Class hClass = db.Classes.Find(classid);

            if (hClass == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (!hClass.Edition.Course.Teachers.Select(t => t.Id).Contains(hUser.Id) && !Session.GetUser().IsSecretary)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var hTeachers = hClass.Edition.Course.Teachers.Select(t => new { Id = t.Id, Name = t.Name + " " + t.Surname });

            ViewBag.ClassId = new SelectList(db.Classes.Select(c => new { Id = c.Id, Name = c.Edition.Course.Name + " " + c.Edition.Course.Grade + c.Section }), "Id", "Name", classid);
            ViewBag.TeacherId = new SelectList(hTeachers, "Id", "Name", lesson.TeacherId);
            ViewBag.Students = new MultiSelectList(vClassStudents, "Id", "Name", hSelectedStudents);
            ViewBag.topics = new MultiSelectList(hClass.Edition.Topics.OrderBy(t => t.Name).ThenBy(t => t.Description), "Id", "DisplayName", lesson.Topics.Select(e => e.Id));

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
                User hLogged    = Session.GetUser();
                Lesson hLesson  = db.Lessons.Find(lesson.Id);
                Class hClass    = db.Classes.Find(lesson.ClassId);


                if (hLogged.IsTeacher && topics != null)
                {
                    hLesson.Topics.Clear();
                    topics.Select(t => db.Topics.Find(t)).ToList().ForEach(t => hLesson.Topics.Add(t));                    
                }

                
                if (students != null && hLogged.IsSecretary)
                {
                    hLesson.Attendings.Clear();
                    var allStudents = hLesson.Class.ActiveStudents.Select(s => s.Id);

                    foreach (var student in allStudents)
                    {
                        if (students.Contains(student))
                        {
                            hLesson.Attendings.Add(new Attending(student, hLesson.ClassId, hLesson.Id, true));
                        }
                        else
                        {
                            hLesson.Attendings.Add(new Attending(student, hLesson.ClassId, hLesson.Id, false));
                        }
                    }                    
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

                    EventLog.Log(db, hLogged, EventLogType.LessonEdited, $"Edited Lesson for {hClass.DisplayName}", true);

                    hLesson.BinaryId = binary.Id;
                }

                
                hLesson.Notes       = lesson.Notes;
                hLesson.Teacher     = db.Users.Find(lesson.TeacherId);
                hLesson.Class       = db.Classes.Find(lesson.ClassId);
                hLesson.ClassSize   = (short)hLesson.Class.ActiveStudents.Count();
                hLesson.Frequency   = (float)hLesson.Attendings.Where(a => a.WasPresent).Count() / (float)hLesson.ClassSize;
                hLesson.Date        = lesson.Date;

                if (double.IsNaN(hLesson.Frequency))
                    hLesson.Frequency = 0.0;

                db.Entry(hLesson).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.ClassId         = new SelectList(db.Classes, "Id", "Section", lesson.ClassId);
            ViewBag.TeacherId       = new SelectList(db.Users, "Id", "Name", lesson.TeacherId);

            return View(lesson);
        }

        public FileContentResult Download(int BinaryId)
        {
            Binary foundFile = db.Binaries.Find(BinaryId);

            if (foundFile != null)
            {
                EventLog.Log(db, Session.GetUser(), EventLogType.LessonDownload, $"Downloaded lesson binary: {BinaryId}", true);
                return File(foundFile.Data, System.Net.Mime.MediaTypeNames.Application.Octet, foundFile.Filename);
            }

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
            User hTeacher = Session.GetUser();
            Lesson lesson = db.Lessons.Find(id);

            if (lesson.BinaryId != null)
                db.Binaries.Remove(db.Binaries.Find(lesson.BinaryId));

            db.Lessons.Remove(lesson);

            db.SaveChanges();

            EventLog.Log(db, hTeacher, EventLogType.LessonDeleted, $"Deleted Lesson {lesson.Id}", true);

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
