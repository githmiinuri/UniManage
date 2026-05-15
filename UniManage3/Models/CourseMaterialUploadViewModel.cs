using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace UniManage3.Models
{
    public class CourseMaterialUploadViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Material Name")]
        public string MaterialName { get; set; }

        [Required]
        [Display(Name = "Material Type")]
        public MaterialTypeEnum MaterialType { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [StringLength(50)]
        [Display(Name = "Duration")]
        public string Duration { get; set; }

        // FilePath is not exposed to the user - used internally
        public string FilePath { get; set; }

        [Required]
        [Display(Name = "Module")]
        public int ModuleId { get; set; }

        // For file upload
        [Display(Name = "Upload File")]
        public IFormFile UploadedFile { get; set; }
    }
}
