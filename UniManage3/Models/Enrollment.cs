using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage3.Models
{
    public class Enrollment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } // 'Completed', 'In-Progress', or 'Failed'

        [StringLength(5)]
        public string Grade { get; set; }

        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        public DateTime? CompletionDate { get; set; }

        // Navigation Properties
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
    }
}
