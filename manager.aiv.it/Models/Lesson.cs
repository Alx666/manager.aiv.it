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

    }

    public interface ILessonMetadata
    {
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = AivManagerEnvironment.DateFormat)]
        DateTime Date { get; }

        [DataType(DataType.MultilineText)]
        string Notes { get; }        
    }

    public enum LessonsSearchType
    {
        Student = 0,
        Teacher = 1,
        Topic = 2,
        Note = 3,
        Class = 4,
        Course = 5,                
    }
}