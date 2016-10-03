
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
    
public partial class Assignment
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public Assignment()
    {

        this.Submissions = new HashSet<Submission>();

    }


    public int Id { get; set; }

    public int ClassId { get; set; }

    public int ExerciseId { get; set; }

    public System.DateTime Deadline { get; set; }

    public string Description { get; set; }

    public int TeacherId { get; set; }

    public System.DateTime UnlockDate { get; set; }

    public byte ExerciseValue { get; set; }



    public virtual Class Class { get; set; }

    public virtual Exercise Exercise { get; set; }

    public virtual User Teacher { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<Submission> Submissions { get; set; }

}

}
