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

        // GET: Submissions
        public ActionResult Index()
        {
            var submissions = db.Submissions.Where(s => DateTime.Now > s.Assignment.Deadline);
            return View(submissions.ToList());
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}