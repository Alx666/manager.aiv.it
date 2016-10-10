using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

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
        [StringLength(1000)]
        string Description { get; }
    }
}