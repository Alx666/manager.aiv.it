using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace manager.aiv.it
{
    public partial class Attending
    {
        public Attending()
        {

        }

        public Attending(int studentId, int classId, int lessonId, bool wasPresent)
        {
            StudentId = studentId;
            ClassId = classId;
            LessonId = lessonId;
            WasPresent = wasPresent;
        }
    }
}