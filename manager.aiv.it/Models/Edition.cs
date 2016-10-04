using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace manager.aiv.it
{
    [MetadataType(typeof(IEditionMetaData))]
    public partial class Edition : IEditionMetaData
    {
        [DisplayName("Name")]
        public string DisplayName => $"{Course.Name} {Course.Grade} ({AcademicYear})";
    }


    public interface IEditionMetaData
    {
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true)]
        DateTime DateStart { get; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true)]
        DateTime DateEnd { get; }
    }
}