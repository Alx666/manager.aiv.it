using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web;


namespace manager.aiv.it
{
    [MetadataType(typeof(IAssignmentMetaData))]
    public partial class Assignment : IAssignmentMetaData
    {
        [DisplayName("Name")]
        public string DisplayName => $"Assignment";
    }

    public interface IAssignmentMetaData
    {
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = AivManagerEnvironment.DateFormat)]
        DateTime UnlockDate { get; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = AivManagerEnvironment.DateFormat)]
        DateTime Deadline { get; }

        [DataType(DataType.MultilineText)]
        string Description { get; }
    }
}
