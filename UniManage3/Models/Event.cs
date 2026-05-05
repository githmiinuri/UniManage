using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage3.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        public string Description { get; set; } // TEXT maps to string

        [Required]
        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; }

        [StringLength(255)]
        public string Location { get; set; }

        [StringLength(100)]
        [Display(Name = "Target Audience")]
        public string TargetAudience { get; set; } // e.g., 'All', 'Students'

        public int? CreatedByAdminId { get; set; }

        [ForeignKey("CreatedByAdminId")]
        public virtual Administrator CreatedBy { get; set; }
    }
}
