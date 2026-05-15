using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace UniManage3.Models
{
    public class AssignmentUploadViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Assignment Name")]
        public string AssignmentName { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Issued Date")]
        public DateTime IssuedDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Deadline Date")]
        public DateTime DeadlineDate { get; set; }

        [Display(Name = "Late Submit Date")]
        public DateTime? LateSubmitDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        // Not exposed for input; used when editing to show current file in view
        public string? ResourceFilePath { get; set; }

        [Required]
        [Display(Name = "Module")]
        public int ModuleId { get; set; }

        // New BatchId for selecting batch in Create/Edit
        [Required]
        [Display(Name = "Batch")]
        public int BatchId { get; set; }

        // File upload
        [Display(Name = "Upload Brief (PDF)")]
        public IFormFile UploadedFile { get; set; }
    }
}