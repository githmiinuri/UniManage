using System.ComponentModel.DataAnnotations;

namespace UniManage3.Models
{
    public class Batch
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        // e.g. "B001"
        public string BatchCode { get; set; }

        [Required]
        [StringLength(255)]
        public string BatchName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public virtual ICollection<Assignment> Assignments { get; set; }
    }
}