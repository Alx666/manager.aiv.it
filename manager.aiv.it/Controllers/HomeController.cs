using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace manager.aiv.it.Controllers
{
    public class HomeController : Controller
    {
        private AivEntities db = new AivEntities();

                
        [CustomAuthorize(RoleType.Admin, RoleType.Director, RoleType.Manager, RoleType.Teacher, RoleType.Secretary, RoleType.Bursar)]
        public ActionResult Index()
        {
            var submissions = db.Submissions.Where(s => DateTime.Now > s.Assignment.Deadline);
            return View(submissions.ToList());
        }
    }
}