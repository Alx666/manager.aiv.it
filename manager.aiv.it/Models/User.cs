using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace manager.aiv.it
{
    [MetadataType(typeof(IUserMetaData))]
    public partial class User
    {
        [DisplayName("Full Name")]
        public string       DisplayName => $"{Name} {Surname}";

        [DisplayFormat(NullDisplayText = "-")]
        [DisplayName("Class Lessons")]
        public int?         DisplayTotalLessons => Class?.Lessons.Where(l => l.Date < DateTime.Now).Count();

        [DisplayFormat(NullDisplayText = "-")]
        [DisplayName("Followed Lessons")]
        public int?         DisplayPresences    => Class?.Lessons.Where(l => l.Class.Id == this.Class.Id).Count();

        [DisplayFormat(NullDisplayText = "-")]
        [DisplayName("Presences")]
        public string       DisplayFrequency
        { 
            get
            {
                if (DisplayTotalLessons.HasValue && DisplayPresences.HasValue)
                    return $"{DisplayPresences} / {DisplayTotalLessons}";
                else
                    return null;
            }
        }

        [DisplayFormat(NullDisplayText = "-")]
        [DisplayName("Missed Lessons")]
        public List<Lesson> MissedLessons => this.LessonsFollowed.Where(l => !l.Students.Contains(this)).ToList();
    }

    public interface IUserMetaData
    {
        [DisplayFormat(NullDisplayText = "-")]
        Class Class { get; }

        [Required]
        [StringLength(50)]
        string Name { get; }

        [Required]
        [StringLength(50)]
        string Surname { get; }

        [Required]
        [StringLength(50)]
        string Email { get; }

        [Required]
        [StringLength(50)]
        string Password { get; }

        [Required]
        [DisplayFormat(NullDisplayText = "-")]
        [StringLength(50)]
        string Mobile { get; }
    }
}