using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace manager.aiv.it.Controllers
{
    public class CommunicationsController : Controller
    {
        private AivManagementEntities db = new AivManagementEntities();


        [CustomAuthorize(RoleType.Director, RoleType.Admin, RoleType.Teacher, RoleType.Secretary)]
        public ActionResult Create()
        {
            ViewBag.classes = new SelectList(db.Classes.ToList(), "Id", "DisplayName");
            ViewBag.staff   = new SelectList((from r in db.Roles
                                              from u in r.Users
                                              where r.Id > (int)RoleType.Student
                                              select u).Distinct().ToList(), "Id", "DisplayName");


            return View();
        }

        // POST: Lessons/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [CustomAuthorize(RoleType.Director, RoleType.Admin, RoleType.Teacher, RoleType.Secretary)]
        [HandleError()]
        public ActionResult Create(List<int> classes, List<int> staff, string subject, string message)
        {
            classes         = classes   ?? new List<int>();
            staff           = staff     ?? new List<int>();


            var hStudents   = from i in classes
                                join c in db.Classes
                                on i equals c.Id
                                from s in c.ActiveStudents
                                select s;

            var hStaff      = from i in staff
                                join u in db.Users
                                on i equals u.Id
                                select u;

            var hAll = hStudents.Concat(hStaff).Distinct().ToList();

            Emailer.Send(hAll, subject, message);

            return RedirectToAction("Index", "Home");
        }
    }
}