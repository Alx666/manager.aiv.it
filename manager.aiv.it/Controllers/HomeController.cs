using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace manager.aiv.it.Controllers
{
    public class HomeController : Controller
    {
        private AivEntities db = new AivEntities();

                
        [CustomAuthorize(RoleType.Admin, RoleType.Director, RoleType.Manager, RoleType.Teacher, RoleType.Secretary, RoleType.Bursar)]
        public ActionResult Index()
        {
            HomeViewModel hModel = new HomeViewModel();

            if (Session.GetUser().IsTeacher)
            {
                User hTeacher = db.Users.Find(Session.GetUser().Id);
                DateTime hNextDay = DateTime.Now.AddDays(-1);

                hModel.Submissions = from a in hTeacher.Assignments
                                     where hNextDay > a.Deadline
                                     from s in a.Submissions
                                     where s.Score == null
                                     select s;                
            }

            if(Session.GetUser().IsSecretary)
                hModel.Lessons     = from l in db.Lessons where (!l.ClassSize.HasValue || l.Students.Count() == 0)  && l.Date <= DateTime.Now orderby l.Date descending select l;

            return View(hModel);
        }
    }
}