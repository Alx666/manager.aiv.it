﻿using System;
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
            HomeViewModel hModel = new HomeViewModel();

            if(Session.GetUser().IsTeacher)
                hModel.Submissions = db.Submissions.Where(s => DateTime.Now > s.Assignment.Deadline);

            if(Session.GetUser().IsSecretary)
                hModel.Lessons     = from l in db.Lessons where !l.ClassSize.HasValue select l;

            return View(hModel);
        }
    }
}