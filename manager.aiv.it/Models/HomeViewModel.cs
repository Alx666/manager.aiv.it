using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace manager.aiv.it
{
    public class HomeViewModel
    {
        public IEnumerable<Submission> Submissions { get; set; }
        public IEnumerable<Lesson>     Lessons     { get; set; }

        public HomeViewModel()
        {
            Submissions = new List<Submission>();
            Lessons     = new List<Lesson>();
        }

    }
}