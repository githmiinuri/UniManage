using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage3.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string CourseCode { get; set; }

        [Required]
        [StringLength(255)]
        public string CourseName { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "Credits must be between 1 and 10")]
        public int Credits { get; set; }

        // Foreign Key for the Prerequisite Course
        public int? PrerequisiteCourseId { get; set; }

        // Navigation property to the actual Course object acting as a prerequisite
        [ForeignKey("PrerequisiteCourseId")]
        public virtual Course PrerequisiteCourse { get; set; }

        // Active flag
        public bool IsActive { get; set; } = true;

        // Coordinator (Lecturer)
        public int? CoordinatorId { get; set; }
        [ForeignKey("CoordinatorId")]
        public virtual Lecturer Coordinator { get; set; }

        // Department relationship
        public int? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        public virtual ICollection<Module> Modules { get; set; } = new HashSet<Module>();
    }
}
