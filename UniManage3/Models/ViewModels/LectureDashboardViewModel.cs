using System;
using System.Collections.Generic;
using UniManage3.Models;

namespace UniManage3.Models.ViewModels
{
    public class LectureDashboardViewModel
    {
        public string LecturerName { get; set; }
        public int TotalEnrolledStudents { get; set; }
        public int OngoingAssignments { get; set; }
        public int TotalModulesAssigned { get; set; }

        public IEnumerable<Course> AssignedCourses { get; set; } = new List<Course>();
        public IEnumerable<Module> AssignedModules { get; set; } = new List<Module>();

        public string ErrorMessage { get; set; }

        public class DummyStudentSubmission
        {
            public int SubmissionId { get; set; }
            public string StudentName { get; set; }
            public string AssignmentName { get; set; }
            public string ModuleName { get; set; }
            public DateTime SubmittedDate { get; set; }
            public string Status { get; set; }
        }

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
