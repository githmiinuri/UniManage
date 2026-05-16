using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.IO;

namespace UniManage3.Models
{
    public class CourseMaterialUploadViewModel : IValidatableObject
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

       public string FilePath { get; set; }

        [Required]
        [Display(Name = "Module")]
        public int ModuleId { get; set; }

        [Display(Name = "Upload File")]
        public IFormFile UploadedFile { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (UploadedFile != null && UploadedFile.Length > 0)
            {
                var allowedExt = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
                {
                    ".pdf",
                    ".ppt",
                    ".pptx",
                    ".mp4",
                    ".mp3",
                    ".mov",
                    ".wmv",
                    ".mpeg",
                    ".mpg",
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".gif"
                };

                var fileExt = Path.GetExtension(UploadedFile.FileName) ?? string.Empty;

                if (!allowedExt.Contains(fileExt))
                {
                    results.Add(new ValidationResult("Invalid file format. Only PDF, PPT, Images, and Video files are allowed.", new[] { nameof(UploadedFile) }));
                }
            }

            return results;
        }
    }
}
