﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace manager.aiv.it
{
    public partial class Course
    {
        public string DisplayName => $"{Name} {Grade}";
    }
}