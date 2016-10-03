using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public ExerciseViewModels(Exercise hEx)
        {
            Id = hEx.Id;
            Name = hEx.Name;
            Author = hEx.Author.Name + " " + hEx.Author.Surname;
            Topics = hEx.Topics.Select(t => t.Name).ToList();
            Type = hEx.Type.Name;
            Course = hEx.Course.Name + " " + hEx.Course.Grade;
            Description = hEx.Description;
            Value = hEx.Value;
        }

        public int      Id              { get; set; }
        public string   Name            { get; set; }
        public List<string> Topics      { get; set; }
        public string   Author          { get; set; }
        public int      Value           { get; set; }
        public string   Type            { get; set; }
        public string   Course          { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description       { get; set; }

    }
}