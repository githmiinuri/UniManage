using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace UniManage3.Models
{
    public class AssignmentUploadViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string AssignmentName { get; set; }

        public string Description { get; set; }

        public DateTime IssuedDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime DeadlineDate { get; set; }

        public DateTime? LateSubmitDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        // Not exposed for input; used when editing to show current file in view
        public string? ResourceFilePath { get; set; }

        [Required]
        public int ModuleId { get; set; }

        // New BatchId for selecting batch in Create/Edit
        [Required]
        public int BatchId { get; set; }

        // File upload
        public IFormFile UploadedFile { get; set; }
    }
}