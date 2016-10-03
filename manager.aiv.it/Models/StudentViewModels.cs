using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace manager.aiv.it.Models
{
    public class StudentViewModels
    {
        public int    Id                 { get; set; }
        public string Name               { get; set; }
        public string Surname            { get; set; }
        public string Email              { get; set; }
        public string Mobile             { get; set; }
        public string Class              { get; set; }
        public string Frequency          { get; set; }
        public List<AssignmentViewModels>  Assignments { get; set; }
        public List<LessonViewModels> MissedLessons { get; set; }
    }
}