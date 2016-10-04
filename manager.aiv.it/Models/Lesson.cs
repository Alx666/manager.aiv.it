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

    [MetadataType(typeof(ILessonMetadata))]
    public partial class Lesson
    {
        [DisplayName("Students")]
        public string DisplayStudentsCount => $"{this.Students.Count()} / {this.ClassSize}";
    }

    public interface ILessonMetadata
    {
        [DataType(DataType.Date)]        
        DateTime Date { get; }

        [DataType(DataType.MultilineText)]
        string Notes { get; }
    }
}