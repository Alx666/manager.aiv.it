using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace manager.aiv.it
{
    [MetadataType(typeof(ITopicMetaData))]
    public partial class Topic
    {
        public string DisplayName => $"{this.Name}, {this.Description}";
    }
    public interface ITopicMetaData
    {
        [Required]
        [StringLength(50)]
        string Name { get; }

        [Required]
        [StringLength(50)]
        string Description { get; }
    }
}