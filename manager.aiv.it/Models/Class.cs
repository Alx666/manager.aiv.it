using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace manager.aiv.it
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public partial class Class
    {        
        [DisplayName("Students")]
        public int      DisplayStudentsCount    => this.Students.Count();

        [DisplayName("Lessons")]
        public int      DisplayLessonsCount     => this.Lessons.Count();

        [DisplayName("Lesson Frequency")]
        [DisplayFormat(DataFormatString = "{0:0.0}%")]
        public float    DisplayFrequency        => this.Lessons.Count() == 0 ? 0f : (float)this.Lessons.Average(l => l.Frequency) * 100f;

        [DisplayName("Points")]
        public int      DisplayPoints           => this.Assignments.Sum(a => a.ExerciseValue);

        [DisplayName("Class")]
        [DisplayFormat(NullDisplayText = "-")]
        public string DisplayName               => this.Edition != null ? $"{this.Edition.Course.Name} {this.Edition.Course.Grade}{this.Section}" : null;
    }
}