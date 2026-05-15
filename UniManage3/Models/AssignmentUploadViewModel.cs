using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace UniManage3.Models
{
    public class AssignmentUploadViewModel : IValidatableObject
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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            var now = DateTime.Now;

            // Deadline must be after issued date
            if (DeadlineDate <= IssuedDate)
            {
                results.Add(new ValidationResult("Deadline date must be after the issued date.", new[] { nameof(DeadlineDate) }));
            }

            // Deadline must be in the future
            if (DeadlineDate < now)
            {
                results.Add(new ValidationResult("Deadline date must be in the future.", new[] { nameof(DeadlineDate) }));
            }

            if (LateSubmitDate.HasValue)
            {
                // Late submit date must be after deadline
                if (LateSubmitDate.Value <= DeadlineDate)
                {
                    results.Add(new ValidationResult("Late submit date must be after the deadline date.", new[] { nameof(LateSubmitDate) }));
                }

                // Late submit date must be in the future
                if (LateSubmitDate.Value < now)
                {
                    results.Add(new ValidationResult("Late submit date must be in the future.", new[] { nameof(LateSubmitDate) }));
                }
            }

            return results;
        }
    }
}