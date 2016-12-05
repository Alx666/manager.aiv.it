using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace manager.aiv.it
{
    [MetadataType(typeof(ISubmissionMetaData))]
    public partial class Submission : ISubmissionMetaData
    {
        [DisplayName("Name")]
        public string DisplayName => $"Submission";

      
    }

    public interface ISubmissionMetaData
    {
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = AivManagerEnvironment.DateFormat)]
        DateTime SubmissionDate { get; }
    }
}