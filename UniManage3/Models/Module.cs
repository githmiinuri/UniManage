using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage3.Models
{
    public class Module
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ModuleCode { get; set; }

        [Required]
        [StringLength(255)]
        public string ModuleName { get; set; }

        public string Description { get; set; }

        [Required]
        // Expanded range to prevent validation errors during save
        [Range(1, 1000, ErrorMessage = "Credits must be between 1 and 1000")]
        public int Credits { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        // Foreign Key
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        // Optional lecturer responsible for this module
        public int? LecturerId { get; set; }
        [ForeignKey("LecturerId")]
        public virtual Lecturer Lecturer { get; set; }

        // Foreign Key for the Semester
        public int? SemesterId { get; set; }

        // Navigation property to the Semester
        [ForeignKey("SemesterId")]
        public virtual Semester Semester { get; set; }

        // Navigation property to link with the Timetable/Classes
        public virtual ICollection<Class> Classes { get; set; }
        public virtual ICollection<CourseMaterial> CourseMaterials { get; set; }
        public virtual ICollection<Assignment> Assignments { get; set; }
    }
}
