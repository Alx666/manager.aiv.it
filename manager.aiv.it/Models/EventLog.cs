﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace manager.aiv.it
{
    public partial class EventLog
    {
        public static EventLog Create(User hUser, EventLogType eType, string sText)
        {
            EventLog hNew = new EventLog();
            hNew.User = hUser;
            hNew.Type = (short)eType;
            hNew.Description = sText;
            hNew.Date = DateTime.Now;
            return hNew;
        }

        public void Log(AivEntities hDb, User hUser, EventLogType eType, string sText)
        {
            EventLog hNew = Create(hUser, eType, sText);
            hDb.EventLogs.Add(hNew);
        }
    }


    public enum EventLogType : short
    {
        LoginSuccess        = 0,
        LoginFailed         = 1,
        StudentCreated      = 2,
        StudentEdited       = 3,
        StudentDeleted      = 4,
        StaffCreated        = 5,
        StaffEdited         = 6,
        StaffDeleted        = 7,
        NoteCreated         = 8,
        NoteDeleted         = 9,
        LessonCreated       = 10,
        LessonEdited        = 11,
        LessonDeleted       = 12,
        LessonDownload      = 13,
        TopicCreated        = 14,
        TopicEdited         = 15,
        TopicDeleted        = 16,
        CourseCreated       = 17,
        CourseEdited        = 18,
        CourseDeleted       = 19,
        EditionCreated      = 20,
        EditionEdited       = 21,
        EditionDeleted      = 22,
        EmailerTriggered    = 23,
    }
}