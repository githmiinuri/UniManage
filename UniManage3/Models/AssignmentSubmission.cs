using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage3.Models
{
    public enum SubmissionStatus
    {
        Pending,
        OnTime,
        Late
    }

    public class AssignmentSubmission
    {
        public int Id { get; set; }

        // Reference to the User (student)
        [Required]
        public int StudentId { get; set; }
        public virtual User Student { get; set; }

        // Reference to Assignment
        [Required]
        public int AssignmentId { get; set; }
        public virtual Assignment Assignment { get; set; }

        [Required]
        public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;

        public DateTime SubmittedTime { get; set; }

        [StringLength(1024)]
        public string SubmittedFilePath { get; set; }

        // Grading fields
        public double? Marks { get; set; }
        [StringLength(8)]
        public string? Grade { get; set; }
        [StringLength(2000)]
        public string? Review { get; set; }
    }
}
