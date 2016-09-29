using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace manager.aiv.it.Models
{
    public class ExerciseViewModels
    {
        public ExerciseViewModels()
        {
            Name = string.Empty;
            Author = string.Empty;
            Topics = new List<string>();
            Type = string.Empty;
            Course = string.Empty;
            Description = string.Empty;
        }

        public int      Id              { get; set; }
        public string   Name            { get; set; }
        public List<string> Topics      { get; set; }
        public string   Author          { get; set; }
        public int      Value           { get; set; }
        public string   Type            { get; set; }
        public string   Course          { get; set; }
        public string Description       { get; set; }

    }
}