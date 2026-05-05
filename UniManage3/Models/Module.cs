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
        [Range(1, 20)]
        public int Credits { get; set; }

        // Foreign Key to Course
        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        // Foreign Key to Lecturer
        [Required]
        public int LecturerId { get; set; }

        [ForeignKey("LecturerId")]
        public virtual Lecturer Lecturer { get; set; }

        // Navigation collections
        public virtual ICollection<CourseMaterial> CourseMaterials { get; set; } = new HashSet<CourseMaterial>();
        public virtual ICollection<Assignment> Assignments { get; set; } = new HashSet<Assignment>();
    }
}
