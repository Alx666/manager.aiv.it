using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace manager.aiv.it.Models
{
    public class LessonViewModels
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Teacher { get; set; }
        public string Class { get; set; }
        public string Topic { get; set; }
        public string Students { get; set; }
    }
}