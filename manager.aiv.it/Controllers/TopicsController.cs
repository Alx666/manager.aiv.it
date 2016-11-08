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
    public class TopicsController : Controller
    {
        private AivEntities db = new AivEntities();

        // GET: Topics
        [CustomAuthorize(RoleType.Teacher, RoleType.Director)]
        public ActionResult Index(string search)
        {
            var model = db.Topics.OrderBy(t => t.Name);

            if (!string.IsNullOrEmpty(search))
            {
                model = from t in model where t.Name.Contains(search) || t.Description.Contains(search) orderby t.Name select t;
            }

            return View(model);
        }

        // GET: Topics/Details/5        
        [CustomAuthorize(RoleType.Teacher, RoleType.Director)]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Topic topic = db.Topics.Find(id);
            if (topic == null)
            {
                return HttpNotFound();
            }

            if (topic.BinaryId != null)
                topic.Binary = db.Binaries.Find(topic.BinaryId);

            return View(topic);
        }

        // GET: Topics/Create
        [CustomAuthorize(RoleType.Director)]
        public ActionResult Create()
        {            
            ViewBag.CourseId = new SelectList(db.Courses, "Id", "DisplayName");
            return View();
        }

        // POST: Topics/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Director)]
        public ActionResult Create([Bind(Include = "Id,Name,Description,CourseId")] Topic topic, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {
                topic.DateAdded = DateTime.Now;                
                if(upload != null)
                {
                    Binary binary = Binary.CreateFrom(upload, true);
                    db.Binaries.Add(binary);
                    db.SaveChanges();

                    topic.BinaryId = binary.Id;
                }
                db.Topics.Add(topic);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(topic);
        }

        // GET: Topics/Edit/5
        [CustomAuthorize(RoleType.Director)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Topic topic = db.Topics.Find(id);
            ViewBag.CourseId = new SelectList(db.Courses, "Id", "DisplayName", topic.CourseId);

            if (topic == null)
            {
                return HttpNotFound();
            }
            return View(topic);
        }

        // POST: Topics/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Director)]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,Deprecated,CourseId")] Topic topic, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {
                Topic hTopic = db.Topics.Find(topic.Id);
                hTopic.Name = topic.Name;
                hTopic.Description = topic.Description;
                hTopic.DateAdded = DateTime.Now.Date;
                hTopic.CourseId = topic.CourseId;

                if (topic.Deprecated)
                {
                    hTopic.DateDeprecated = DateTime.Now;
                    hTopic.Deprecated = true;
                }
                else
                {
                    hTopic.DateDeprecated = null;
                    hTopic.Deprecated = false;
                }

                if (upload != null)
                {
                    Binary binary = Binary.CreateFrom(upload, true);
                    db.Binaries.Add(binary);
                    db.SaveChanges();

                    hTopic.BinaryId = binary.Id;
                }

                db.Entry(hTopic).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CourseId = new SelectList(db.Courses, "Id", "DisplayName", topic.CourseId);
            return View(topic);
        }

        public FileContentResult Download(int BinaryId)
        {
            Binary foundFile = db.Binaries.Find(BinaryId);
            if (foundFile != null)
                return File(foundFile.Data, System.Net.Mime.MediaTypeNames.Application.Octet, foundFile.Filename);

            return null;
        }

        // GET: Topics/Delete/5
        [CustomAuthorize(RoleType.Director)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Topic topic = db.Topics.Find(id);
            if (topic == null)
            {
                return HttpNotFound();
            }
            return View(topic);
        }

        // POST: Topics/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Director)]
        public ActionResult DeleteConfirmed(int id)
        {
            Topic topic = db.Topics.Find(id);
            db.Topics.Remove(topic);
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
