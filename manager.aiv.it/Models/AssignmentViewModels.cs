using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace manager.aiv.it.Models
{
    public class AssignmentViewModels : ExerciseViewModels
    {
        public string Class { get; set; }

        
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime UnlockDate { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime Deadline { get; set; }

        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        public AssignmentViewModels(Exercise hEx) : base(hEx)
        {
            UnlockDate = DateTime.Now;
            Deadline = DateTime.Now.AddDays(7);
        }

        public AssignmentViewModels() : base()
        {
            UnlockDate = DateTime.Now;
            Deadline = DateTime.Now.AddDays(7);
        }
    }
}