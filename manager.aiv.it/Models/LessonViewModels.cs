using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace manager.aiv.it.Models
{
    public class LessonViewModels
    {
        public LessonViewModels()
        {

        }

        public LessonViewModels(Lesson hLesson)
        {
            Id = hLesson.Id;
            Date = hLesson.Date;
            Teacher = hLesson.Teacher.Name + " " + hLesson.Teacher.Surname;
            Class = hLesson.Class.Edition.Course.Name + " " + hLesson.Class.Edition.Course.Grade + hLesson.Class.Section;
            Topic = hLesson.Topics.Select(t => t.Name).Aggregate((x, y) => x + ", " + y);
        }

        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Teacher { get; set; }
        public string Class { get; set; }
        public string Topic { get; set; }
        public string Students { get; set; }
    }
}