using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage3.Models
{
    public class Module
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ModuleCode { get; set; }

        [Required]
        [StringLength(255)]
        public string ModuleName { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(1, 20)]
        public int Credits { get; set; }

        // Foreign Key
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
    }
}
