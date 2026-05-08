using System;

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
        public double? Marks { get; set; }
        public string Grade { get; set; }
        public string Review { get; set; }
    }
}
