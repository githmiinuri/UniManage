using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage3.Models
{
    public class LibraryResource
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [StringLength(255)]
        public string Author { get; set; }

        [StringLength(50)]
        [Display(Name = "Resource Type")]
        public string ResourceType { get; set; } // e.g., 'E-Book', 'Research Paper'

        [Required]
        [StringLength(1024)]
        public string FilePath { get; set; } // Path for PDF storage

        public DateTime UploadDate { get; set; } = DateTime.Now;

        public int? DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }
    }
}
