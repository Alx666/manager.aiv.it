using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace manager.aiv.it
{
    [MetadataType(typeof(ICourseMetaData))]
    public partial class Course
    {
        public string DisplayName => $"{Name} {Grade}";
    }

    public interface ICourseMetaData
    {
        [Required]
        [StringLength(50)]
        string Name { get; }

        [Required]
        [Range(1, 5)]
        byte Grade { get; }
    }
}