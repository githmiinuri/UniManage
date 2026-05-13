using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UniManage3.Models;

namespace UniManage3.Models
{
    public class Semester
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Semester Name")]
        public string SemesterName { get; set; } // e.g., 'Year 1 - Semester 1'

        public int SemesterNumber { get; set; } = 1;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required]
        public int CourseId { get; set; }

        // Navigation Property back to Course
        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }
        public virtual ICollection<Module> Modules { get; set; } = new List<Module>();
    }
}
