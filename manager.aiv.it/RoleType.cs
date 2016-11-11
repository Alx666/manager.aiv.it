using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace manager.aiv.it
{
    public enum RoleType
    {
        NoAccess = 0,
        Student = 2,
        Teacher = 4,
        Director = 8,
        Secretary = 16,
        Bursar = 32,
        Manager = 64,
        Admin = 128,
        Developer = 256
    }


    public class AivManagerEnvironment
    {
        public const string DateFormat = "{0:MM/dd/yyyy}";
    }
}

