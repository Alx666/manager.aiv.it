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
    public class SubmissionsController : Controller
    {
        private AivEntities db = new AivEntities();

        // GET: Submissions
        public ActionResult Index()
        {
            var submissions = db.Submissions.Include(s => s.Student).Include(s => s.Revisor).Include(s => s.Assignment).Include(s => s.Binary);
            return View(submissions.ToList());
        }

        // GET: Submissions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Submission submission = db.Submissions.Find(id);
            if (submission == null)
            {
                return HttpNotFound();
            }
            return View(submission);
        }

        // POST: Submissions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Upload(int AssignmentId, HttpPostedFileBase upload)
        {
            try
            {
                User        hUser       = db.Users.Find((int)this.Session["UserId"]);
                bool        bAdd;
                Submission  hSubmission = db.Submissions.Find(AssignmentId, hUser.Id);

                if (hSubmission == null)
                {
                    hSubmission             = new Submission();
                    hSubmission.Student     = hUser;
                    hSubmission.Assignment  = db.Assignments.Find(AssignmentId);
                    bAdd = true;
                }
                else
                {                    
                    bAdd = false;
                }

                if (hSubmission.Binary != null)
                    db.Binaries.Remove(hSubmission.Binary);

                hSubmission.SubmissionDate = DateTime.Now;
                

                Binary hBinaryData      = new Binary();
                byte[] fileBytes        = new byte[upload.InputStream.Length];
                upload.InputStream.Read(fileBytes, 0, fileBytes.Length);
                hBinaryData.Data        = fileBytes;
                hBinaryData.Filename    = upload.FileName;
                hSubmission.Binary      = hBinaryData;

                if(bAdd)
                    db.Submissions.Add(hSubmission);

                db.SaveChanges();

                return View();
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }            
        }

        // GET: Submissions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Submission submission = db.Submissions.Find(id);
            if (submission == null)
            {
                return HttpNotFound();
            }
            return View(submission);
        }

        // POST: Submissions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Submission submission = db.Submissions.Find(id);
            db.Submissions.Remove(submission);
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
