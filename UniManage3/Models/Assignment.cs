using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage3.Models
{
    public class Assignment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string AssignmentName { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime IssuedDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime DeadlineDate { get; set; }

        public DateTime? LateSubmitDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [Required]
        // Stores the local path, e.g., "/Uploads/Assignments/Briefs/CW1.pdf"
        public string ResourceFilePath { get; set; }

        [Required]
        public int ModuleId { get; set; }
        [ForeignKey("ModuleId")]
        public virtual Module Module { get; set; }
    }
}
