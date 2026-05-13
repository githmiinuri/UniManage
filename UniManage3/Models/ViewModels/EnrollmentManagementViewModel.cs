namespace UniManage3.ViewModels
{
    public class EnrollmentManagementViewModel
    {
        public List<EnrollmentItemViewModel> Enrollments { get; set; } = new();
        public string CurrentStatus { get; set; } = "In-Progress";
        public string SearchTerm { get; set; } = "";

        public int PendingCount { get; set; }
        public int CompletedCount { get; set; }
        public int FailedCount { get; set; }
    }

    public class EnrollmentItemViewModel
    {
        public int Id { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public int? PrerequisiteCourseId { get; set; }
        public string PrerequisiteCode { get; set; }
        public string PrerequisiteName { get; set; }
        public bool PrerequisiteMet { get; set; } // Logic: check if student has a 'Completed' enrollment for PrerequisiteCourseId
    }
}

