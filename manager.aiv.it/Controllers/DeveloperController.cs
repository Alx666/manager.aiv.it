using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace manager.aiv.it.Controllers
{
    public class DeveloperController : Controller
    {
        private AivEntities db = new AivEntities();


        [CustomAuthorize(RoleType.Developer)]
        public ActionResult EventLogs()
        {
            return View(db.EventLogs.OrderByDescending(e => e.Date));
        }


        [CustomAuthorize(RoleType.Developer)]
        public void UpdateLessons()
        {
            string sPath = Server.MapPath("~\\FileStorage");

            if (!Directory.Exists(sPath))
                Directory.CreateDirectory(sPath);


            foreach (Lesson hLesson in db.Lessons)
            {
                if (hLesson.BinaryId == null)
                    continue;

                manager.aiv.it.File hFile = new it.File();

                hFile.OriginalName      = hLesson.Binary.Filename;
                hFile.ArchivationName   = AivExtensions.RandomString(50) + ".dat";
                
                using (FileStream hFs = System.IO.File.OpenWrite(sPath + "\\" + hFile.ArchivationName))
                {
                    hFs.Write(hLesson.Binary.Data, 0, hLesson.Binary.Data.Length);
                    hFs.Flush();
                }

                hLesson.File = hFile;
            }

            db.SaveChanges();

        }

        //[CustomAuthorize(RoleType.Developer)]
        //public ActionResult ClearBinaries()
        //{
        //    List<Binary> hTopicBin = this.Clear<Topic, Binary>(db.Topics, db.Binaries, t => t.Binary != null, t => t.Binary, t => t.Binary = null);
        //    List<Binary> hLessoBin = this.Clear<Lesson, Binary>(db.Lessons, db.Binaries, t => t.Binary != null, t => t.Binary, t => t.Binary = null);
        //    List<Binary> hSubmiBin = this.Clear<Submission, Binary>(db.Submissions, db.Binaries, t => t.Binary != null, t => t.Binary, t => t.Binary = null);
        //    List<Binary> hExercise = this.Clear<Exercise, Binary>(db.Exercises, db.Binaries, t => t.Binary != null, t => t.Binary, t => t.Binary = null);

        //    List<string> hAll = new List<string>();
        //    hAll.Add($"Removed From Topics: {hTopicBin.Count}");
        //    hAll.AddRange(hTopicBin.Select(b => $"{b.Id} {b.Filename}"));

        //    hAll.Add($"Removed From Lessons: {hLessoBin.Count}");
        //    hAll.AddRange(hLessoBin.Select(b => $"{b.Id} {b.Filename}"));

        //    hAll.Add($"Removed From Submissions: {hSubmiBin.Count}");
        //    hAll.AddRange(hSubmiBin.Select(b => $"{b.Id} {b.Filename}"));

        //    hAll.Add($"Removed From Exercise: {hExercise.Count}");
        //    hAll.AddRange(hExercise.Select(b => $"{b.Id} {b.Filename}"));


        //    db.SaveChanges();

        //    return View(hAll);
        //}

        //Generic purpose clean-up algorithm
        private List<T> Clear<K,T>(DbSet<K> hSet, DbSet<T> hRemoveSet, Func<K,bool> hCondition, Func<K,T> hSelector, Action<K> hNullifier) 
            where K : class
            where T : class
        {
            List<T> hToRemove = hSet.Where(hCondition).Select(hSelector).ToList();
            hRemoveSet.RemoveRange(hToRemove);
            hSet.ToList().ForEach(hNullifier);

            return hToRemove;
        }

        //[CustomAuthorize(RoleType.Developer)]
        //public void RepairBrokenLessons()
        //{
        //    var hLessons = from l in db.Lessons where l.Students.Count() == 0 select l;
        //    hLessons.ToList().ForEach(l => l.ClassSize = null);
        //}



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