using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

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
        [Range(1, 1000, ErrorMessage = "Credits must be between 1 and 1000")]
        public int Credits { get; set; }
        public string Duration { get; set; } = "Not Specified";

        public int? PrerequisiteCourseId { get; set; }

        [ForeignKey("PrerequisiteCourseId")]
        public virtual Course PrerequisiteCourse { get; set; }

        public bool IsActive { get; set; } = true;

        public int? CoordinatorId { get; set; }
        [ForeignKey("CoordinatorId")]
        public virtual Lecturer Coordinator { get; set; }

        public int? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        public virtual ICollection<Module> Modules { get; set; } = new HashSet<Module>();
        public virtual ICollection<Semester> Semesters { get; set; } = new List<Semester>();
    }
}
