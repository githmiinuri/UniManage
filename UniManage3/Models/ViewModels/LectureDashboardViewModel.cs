using System;
using System.Collections.Generic;
using UniManage3.Models;

namespace UniManage3.Models.ViewModels
{
    /// <summary>
    /// ViewModel for the Lecturer Dashboard.
    /// Contains KPIs and lists of assigned courses and modules.
    /// Includes dummy structures for frontend previews.
    /// </summary>
    public class LectureDashboardViewModel
    {
        public string LecturerName { get; set; }

        // KPIs
        public int TotalEnrolledStudents { get; set; }
        public int OngoingAssignments { get; set; }
        public int TotalModulesAssigned { get; set; }

        // Collections
        public IEnumerable<Course> AssignedCourses { get; set; } = new List<Course>();
        public IEnumerable<Module> AssignedModules { get; set; } = new List<Module>();

        // Optional
        public string ErrorMessage { get; set; }

        // Recent submissions structure
        public class DummyStudentSubmission
        {
            public int SubmissionId { get; set; }
            public string StudentName { get; set; }
            public string AssignmentName { get; set; }
            public string ModuleName { get; set; }
            public DateTime SubmittedDate { get; set; }
            public string Status { get; set; }
        }

        // New assignment-based grading progress
        public class AssignmentGradingProgress
        {
            public string AssignmentName { get; set; }
            public string ModuleCode { get; set; }
            public int TotalSubmissions { get; set; }
            public int GradedCount { get; set; }
            public int PendingCount => TotalSubmissions - GradedCount;
            public double ProgressPercentage => TotalSubmissions == 0 ? 0 : Math.Round((double)GradedCount / TotalSubmissions * 100);
        }

        public List<DummyStudentSubmission> RecentSubmissions { get; set; } = new List<DummyStudentSubmission>();
        public List<AssignmentGradingProgress> GradingSummaries { get; set; } = new List<AssignmentGradingProgress>();
    }
}
