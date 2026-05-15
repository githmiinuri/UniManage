using System;
using System.ComponentModel.DataAnnotations;

namespace UniManage3.Models.ViewModels
{
    public class GradingViewModel
    {
        public int Id { get; set; }
        public string StudentName { get; set; }
        public string AssignmentName { get; set; }
        public DateTime SubmittedTime { get; set; }
        public UniManage3.Models.SubmissionStatus Status { get; set; }
        public string DownloadPath { get; set; }

        // Grading inputs
        [Range(0, 100, ErrorMessage = "Marks must be between {1} and {2}.")]
        [Display(Name = "Marks")]
        public double? Marks { get; set; }

        [StringLength(8, ErrorMessage = "Grade cannot be longer than {1} characters.")]
        public string Grade { get; set; }

        [StringLength(2000, ErrorMessage = "Review cannot be longer than {1} characters.")]
        public string Review { get; set; }
    }
}
