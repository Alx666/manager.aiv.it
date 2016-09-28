using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace manager.aiv.it.Models
{
    public class ClassViewModels
    {
        public int      Id          { get; set; }
        public string   Name        { get; set; }
        public int      Students    { get; set; }
        public int      Lessons     { get; set; }
        public double   Frequency   { get; set; }
        public float    Temp0       { get; set; }
        public float    Temp1       { get; set; }
    }
}