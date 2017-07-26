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

    [MetadataType(typeof(IClassMetaData))]
    public partial class Class
    {        
        [DisplayName("Students")]
        public int      DisplayStudentsCount    => this.ActiveStudents.Count();

        [DisplayName("Lessons")]
        public int      DisplayLessonsCount     => this.Lessons.Count();

        [DisplayName("Lesson Frequency")]
        [DisplayFormat(DataFormatString = "{0:0.0}%")]
        public float    DisplayFrequency        => this.Lessons.Count() == 0 ? 0f : (float)this.Lessons.Where(l => l.ClassSize != null).Average(l => l.Frequency) * 100f;

        [DisplayName("Points")]
        public int      DisplayPoints           => this.Assignments.Sum(a => a.ExerciseValue);

        [DisplayName("Class")]
        [DisplayFormat(NullDisplayText = "-")]
        public string DisplayName               => this.Edition != null ? $"{this.Edition.Course.Name} {this.Edition.Course.Grade}{this.Section}" : null;
    }

    public interface IClassMetaData
    {
        [Required]
        [StringLength(1)]
        string Section { get; }
    }
}