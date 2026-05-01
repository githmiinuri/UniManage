using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage3.Models
{
    public class CourseMaterial
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string MaterialName { get; set; }

        [Required]
        public MaterialTypeEnum MaterialType { get; set; }

        public string Description { get; set; }

        [StringLength(50)]
        // Optional: "Week 1", "Module 2"
        public string Duration { get; set; }

        [Required]
        // Stores the local path, e.g., "/uploads/materials/intro-video.mp4"
        public string FilePath { get; set; }

        [Required]
        public int ModuleId { get; set; }
        [ForeignKey("ModuleId")]
        public virtual Module Module { get; set; }
    }
}
