﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace manager.aiv.it
{
    [MetadataType(typeof(IAssignmentMetaData))]
    public partial class Assignment
    {

    }

    public interface IAssignmentMetaData
    {
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true)]
        DateTime UnlockDate { get; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true)]
        DateTime Deadline { get; }
    }
}