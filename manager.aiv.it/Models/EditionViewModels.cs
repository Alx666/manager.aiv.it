using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace manager.aiv.it.Models
{
    public class EditionViewModels
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AcademicYear { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
    }
}