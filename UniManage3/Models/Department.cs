using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UniManage3.Models;

namespace UniManage3.Models
{
    [Table("departments")]
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Department Name")]
        public string DepartmentName { get; set; }

        [Required]
        [Display(Name = "Department Code")]
        public string DepartmentCode { get; set; }

        public string Description { get; set; }

        [Display(Name = "Head of Department")]
        public int? HeadOfDepartmentId { get; set; }

        // Navigation property for the Lecturer (Head)
        [ForeignKey("HeadOfDepartmentId")]
        public virtual Lecturer HeadOfDepartment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
