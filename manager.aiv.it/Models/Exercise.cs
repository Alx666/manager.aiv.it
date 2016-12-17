using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace manager.aiv.it
{
    [MetadataType(typeof(IExerciseMetaData))]
    public partial class Exercise
    {
        [DisplayName("Name")]
        public string DisplayName => $"Exercise";
    }
    public interface IExerciseMetaData
    {
        [Required]
        [StringLength(50)]
        string Name { get; }

        [Required]
        [DataType(DataType.MultilineText)]
        [StringLength(5000)]
        [AllowHtml]
        string Description { get; }

        [Required]
        ICollection<Topic> Topics { get; }
    }
}