using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace manager.aiv.it
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;


    [MetadataType(typeof(IUserMetaData))]
    public partial class User
    {                
        public int?      DisplayTotalLessons => Class == null ? 0 : Class.Lessons.Where(l => l.Date < DateTime.Now).Count();
        public int?      DisplayPresences    => Class == null ? 0 : LessonsFollowed.Where(l => l.Class.Id == this.Class.Id).Count();
        public string   DisplayFrequency     => Class == null ? string.Empty : $"{DisplayPresences} / {DisplayTotalLessons}";

        public override string   ToString()  => $"{Name} {Surname}";
    }

    public interface IUserMetaData
    {
        [DisplayFormat(NullDisplayText = "-")]
        Class Class { get; }
    }
}