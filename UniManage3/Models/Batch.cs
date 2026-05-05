using System;
using System.Collections.Generic;
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

        [Required]
        [Display(Name = "Minimum Students")]
        public int MinStudents { get; set; } = 10; // Default matches DB

        [Required]
        [Display(Name = "Maximum Students")]
        public int MaxStudents { get; set; } = 30; // Default matches DB

        [Required]
        [Display(Name = "Allow Exceptions")]
        public bool AllowExceptions { get; set; } = false; // Maps to TINYINT(1)

        public virtual ICollection<Assignment> Assignments { get; set; }
    }
}