using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace manager.aiv.it
{
    public partial class Topic
    {
        public string DisplayName => $"{this.Name}, {this.Description}";
    }
}