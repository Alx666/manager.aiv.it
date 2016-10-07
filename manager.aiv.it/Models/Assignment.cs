using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace manager.aiv.it
{
    [MetadataType(typeof(IAssignmentMetaData))]
    public partial class Assignment : IAssignmentMetaData
    {

    }

    public interface IAssignmentMetaData
    {
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM-dd-yyyy}")]
        DateTime UnlockDate { get; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM-dd-yyyy}")]
        DateTime Deadline { get; }
    }
}
