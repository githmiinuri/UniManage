using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace UniManage3.Models
{
    public class CourseMaterialUploadViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string MaterialName { get; set; }

        [Required]
        public MaterialTypeEnum MaterialType { get; set; }

        public string Description { get; set; }

        [StringLength(50)]
        public string Duration { get; set; }

        // FilePath is not exposed to the user - used internally
        public string FilePath { get; set; }

        [Required]
        public int ModuleId { get; set; }

        // For file upload
        public IFormFile UploadedFile { get; set; }
    }
}
