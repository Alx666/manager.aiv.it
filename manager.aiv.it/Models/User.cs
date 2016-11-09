using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
        public int?         DisplayPresences    => Class?.Lessons.Where(l => l.Students.Contains(this)).Count();

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
        [DisplayName("Frequency")]
        public float? DisplayFrequencyPercent
        {

            get
            {

                if (DisplayTotalLessons.HasValue && DisplayPresences.HasValue && DisplayTotalLessons.Value > 0f)
                {
                    return ((float)DisplayPresences / (float)DisplayTotalLessons) * 100.0f;
                }
                else
                    return null;
            }
        }

        [DisplayFormat(NullDisplayText = "-")]
        [DisplayName("Missed Lessons")]
        //public List<Lesson> MissedLessons => this.Class.Lessons.Where(l => !l.Students.Contains(this) && l.Date <= DateTime.Now).ToList();
        public List<Lesson> MissedLessons
        {
            get
            {
                if (this.Class == null || this.Class.Lessons == null)
                    return new List<Lesson>();

                IEnumerable<Lesson> dbResults = this.Class.Lessons.Where(l => !l.Students.Contains(this) && l.Date <= DateTime.Now);
                if(dbResults == null)
                    return new List<Lesson>();

                return dbResults.ToList();
            }
        }

        public List<Assignment> ClassAssignments
        {
            get
            {
                if (this.Class == null || this.Class.Assignments == null)
                    return new List<Assignment>();

                IEnumerable<Assignment> dbResults = this.Class.Assignments.Where(a => a.UnlockDate <= DateTime.Now.Date && a.Deadline >= DateTime.Now.Date );
                if (dbResults == null)
                    return new List<Assignment>();

                return dbResults.ToList();
            }
        }

        private List<RoleType> m_hRoles;
        public bool IsSecretary     { get; private set; }
        public bool IsAdmin         { get; private set; }
        public bool IsBursar        { get; private set; }
        public bool IsTeacher       { get; private set; }
        public bool IsDirector      { get; private set; }
        public bool IsManager       { get; private set; }
        public bool IsStudent       { get; private set; }

        public bool IsOnly(RoleType eType)
        {
            return (Roles.Count == 1 && m_hRoles != null && m_hRoles.Contains(eType));
        }

        public void LoadRoles(List<RoleType> hRoles)
        {
            IsSecretary = hRoles.Contains(RoleType.Secretary);
            IsAdmin     = hRoles.Contains(RoleType.Admin);
            IsBursar    = hRoles.Contains(RoleType.Bursar);
            IsTeacher   = hRoles.Contains(RoleType.Teacher);
            IsDirector  = hRoles.Contains(RoleType.Director);
            IsManager   = hRoles.Contains(RoleType.Manager);
            IsStudent   = hRoles.Contains(RoleType.Student);

            m_hRoles = hRoles;
        }

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

        
        [StringLength(50)]
        string Email { get; }

        
        [StringLength(50)]
        string Password { get; }

        //[Required]
        [DisplayFormat(NullDisplayText = "-")]
        [StringLength(50)]
        string Mobile { get; }

        [DisplayFormat(NullDisplayText = "-")]
        [StringLength(50)]
        string City { get; }

        [DisplayFormat(NullDisplayText = "-")]
        [StringLength(50)]
        string Address { get; }

        [DisplayFormat(NullDisplayText = "-")]
        [StringLength(50)]
        string Code { get; }
    }

    public static class SessionExtensions
    {
        public static User GetUser(this HttpSessionStateBase session)
        {            
            return session["User"] as User;
        }
        public static void LoadUser(this HttpSessionStateBase session, User user)
        {
            List<RoleType> hRoles = user.Roles.Select(r => (RoleType)r.Id).ToList();

            session["UserId"] = user.Id;
            session["Roles"]  = hRoles;
            session["User"]   = user;

            user.LoadRoles(hRoles);
        }
    }
}