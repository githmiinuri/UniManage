using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage3.Models
{
    public class Module
    {
        [Key]
        public int Id { get; set; }

        [Required]
<<<<<<< HEAD
        [StringLength(50)]
=======
        [StringLength(100)]
>>>>>>> 355f77b (#1 feat: Implement the lecturer flow)
        public string ModuleCode { get; set; }

        [Required]
        [StringLength(255)]
        public string ModuleName { get; set; }

<<<<<<< HEAD
        public string Description { get; set; }

        [Required]
        [Range(1, 20)]
        public int Credits { get; set; }

        // Foreign Key
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
=======
        [Required]
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [Required]
        public int LecturerId { get; set; }
        [ForeignKey("LecturerId")]
        public virtual Lecturer Lecturer { get; set; }
        public virtual ICollection<CourseMaterial> CourseMaterials { get; set; }
        public virtual ICollection<Assignment> Assignments { get; set; }
>>>>>>> 355f77b (#1 feat: Implement the lecturer flow)
    }
}
