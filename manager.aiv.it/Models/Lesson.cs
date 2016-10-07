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
        public string DisplayStudentsCount  => $"{this.Students.Count()} / {this.ClassSize}";

        [DisplayName("Missed Topics")]
        public string DisplayTopics         => this.Topics.Select(t => t.DisplayName).Aggregate((x, y) => x + ", " + y);
    }

    public interface ILessonMetadata
    {
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = AivManagerEnvironment.DateFormat)]
        DateTime Date { get; }

        [DataType(DataType.MultilineText)]
        string Notes { get; }        
    }
}