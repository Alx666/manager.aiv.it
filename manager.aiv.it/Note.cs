//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace manager.aiv.it
{
    using System;
    using System.Collections.Generic;
    
    public partial class Note
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int StaffId { get; set; }
        public string Text { get; set; }
        public System.DateTime Date { get; set; }
    
        public virtual User Author { get; set; }
        public virtual User Subject { get; set; }
    }
}
