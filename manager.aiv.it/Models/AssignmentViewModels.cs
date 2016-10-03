using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace manager.aiv.it.Models
{
    public class AssignmentViewModels : ExerciseViewModels
    {
        public string Class         { get; set; }
        public int ClassId          { get; set; }
        public int ExerciseId       { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime UnlockDate  { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Deadline    { get; set; }        

        [DataType(DataType.MultilineText)]
        public string Notes         { get; set; }
        public int TeacherId        { get; set; }
        public string Teacher       { get; set; }

        public int AssignmentId { get; set; }

        public AssignmentViewModels(DateTime vUnlock, DateTime vDeadLine, string sNotes, Exercise hEx) : base(hEx)
        {
            UnlockDate  = vUnlock;
            Deadline    = vDeadLine;
            Notes       = sNotes;
        }

        public AssignmentViewModels(Exercise hEx) : base(hEx)
        {
            UnlockDate  = DateTime.Now;
            Deadline    = DateTime.Now.AddDays(7);
        }

        public AssignmentViewModels(Assignment a) : base(a.Exercise)
        {
            AssignmentId = a.Id;
            Class       = a.Class.Edition.Course.Name + " " + a.Class.Edition.Course.Grade + a.Class.Section;
            UnlockDate  = a.UnlockDate;
            Deadline    = a.Deadline;
            Notes       = a.Description;
            Teacher     = a.Teacher.Name + " " + a.Teacher.Surname;
        }

        public AssignmentViewModels() : base()
        {
            UnlockDate  = DateTime.Now;
            Deadline    = DateTime.Now.AddDays(7);
        }
    }
}