using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using manager.aiv.it;
using System.Text.RegularExpressions;


//TODO: Submission.Details, pulsante Save deve redirettare correttamente
//TODO: Submission.Details, necessita di un link all'esercizio associato (non si sa cosa si sta correggendo)

namespace manager.aiv.it.Controllers
{
    public class SubmissionsController : Controller
    {
        private AivEntities db = new AivEntities();

        // GET: Submissions
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Index()
        {
            var submissions = db.Submissions.Include(s => s.Student).Include(s => s.Revisor).Include(s => s.Assignment);
            return View(submissions.OrderByDescending(s => s.SubmissionDate));
        }

        // GET: Submissions/Details/5
        [CustomAuthorize(RoleType.Teacher, RoleType.Student)]
        public ActionResult Details(int? assignmentId, int? studentId)
        {
            if(Session.GetUser().IsOnly(RoleType.Student) && Session.GetUser().Id != studentId)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (assignmentId == null || studentId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Submission submission = db.Submissions.Find(assignmentId, studentId);
            if (submission == null)
            {
                return HttpNotFound();
            }

            SelectList scores = new SelectList(Enumerable.Range(0, (int)submission.Assignment.ExerciseValue + 1), submission.Score);
            ViewBag.scores = scores;

            return View(submission);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RoleType.Teacher)]
        public ActionResult Edit([Bind(Include = "Id,AssignmentId,StudentId,BinaryId,SubmissionDate,RevisionDate,Score,RevisorId, RevisorNote")] Submission submission, int assignmentId, int studentId)
        {
            if (ModelState.IsValid)
            {
                Submission existing = db.Submissions.Find(assignmentId, studentId);
                existing.Revisor = db.Users.Find(Session.GetUser().Id);
                existing.Score = submission.Score;
                existing.RevisorNote = submission.RevisorNote;
                existing.RevisionDate = DateTime.Now;
                db.Entry(existing).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Redirect(Request.UrlReferrer.ToString());
        }

        // FALLBACK for Create action
        [HttpPost]
        [CustomAuthorize(RoleType.Student)]
        public ActionResult Create(int assignmentId, HttpPostedFileBase upload)
        {
            return Upload(assignmentId, upload);
        }

        


        //TODO: Implementare nella view, i filtri per i tipi di file accettato
        [HttpPost]
        public ActionResult Upload(int assignmentId, HttpPostedFileBase upload)
        {
            try
            {
                User        hUser       = db.Users.Find(Session.GetUser().Id);
                bool        bAdd;
                Submission  hSubmission = db.Submissions.Find(assignmentId, hUser.Id);

                if (hSubmission == null)
                {
                    hSubmission             = new Submission();
                    hSubmission.Student     = hUser;
                    hSubmission.Assignment  = db.Assignments.Find(assignmentId);
                    bAdd = true;
                }
                else
                {                    
                    bAdd = false;
                }

                if (hSubmission.Binary != null)
                    db.Binaries.Remove(hSubmission.Binary);

                hSubmission.SubmissionDate  = DateTime.Now;
                hSubmission.RevisionDate    = null;
                hSubmission.Score           = null;
                hSubmission.RevisorId       = null;
                
                Binary hBinaryData      = new Binary();
                byte[] fileBytes        = new byte[upload.InputStream.Length];
                upload.InputStream.Read(fileBytes, 0, fileBytes.Length);
                hBinaryData.Data        = fileBytes;

                string binaryPath = upload.FileName;
                string[] pathParts = Regex.Split(binaryPath, @"(/)|(\\)");
                string filename = pathParts[pathParts.Length - 1];
                hBinaryData.Filename = filename;

                hSubmission.Binary      = hBinaryData;

                if(bAdd)
                    db.Submissions.Add(hSubmission);

                db.SaveChanges();

                if (hUser.IsOnly(RoleType.Student))
                    return View("Details", "Students", new { id = hUser.Id });
                else
                    return Redirect(Request.UrlReferrer.ToString());
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
        [CustomAuthorize(RoleType.Student)]
        public ActionResult DeleteConfirmed(int id)
        {
            Submission submission = db.Submissions.Find(id);

            Binary hBinToDelete = db.Binaries.Find(submission.BinaryId);
            db.Binaries.Remove(hBinToDelete);
            db.Submissions.Remove(submission);

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public FileContentResult Download(int BinaryId)
        {
            Binary foundFile = db.Binaries.Find(BinaryId);
            if (foundFile != null)
                return File(foundFile.Data, System.Net.Mime.MediaTypeNames.Application.Octet, System.IO.Path.GetFileName(foundFile.Filename));

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
