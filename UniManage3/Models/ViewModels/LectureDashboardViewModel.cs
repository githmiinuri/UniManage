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

        // Dummy/submission structures for future implementation
        public class DummyStudentSubmission
        {
            public string StudentName { get; set; }
            public string AssignmentName { get; set; }
            public string ModuleName { get; set; }
            public DateTime SubmittedDate { get; set; }
            public string Status { get; set; }
        }

        public class DummyGradingSummary
        {
            public string ModuleName { get; set; }
            public int TotalSubmissions { get; set; }
            public int GradedCount { get; set; }
            public double AverageScore { get; set; }
        }

        public List<DummyStudentSubmission> RecentSubmissions { get; set; } = new List<DummyStudentSubmission>();
        public List<DummyGradingSummary> GradingSummaries { get; set; } = new List<DummyGradingSummary>();
    }
}
