using System.Collections.Generic;

namespace UniManage3.ViewModels
{
    public class MyCoursesViewModel
    {
        public List<EnrolledCourseViewModel> EnrolledCourses { get; set; } = new();
    }

    public class EnrolledCourseViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public List<SemesterGroupViewModel> Semesters { get; set; } = new();
    }

    public class SemesterGroupViewModel
    {
        public int SemesterId { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public int SemesterNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<ModuleDetailViewModel> Modules { get; set; } = new();
    }

    public class ModuleDetailViewModel
    {
        public int ModuleId { get; set; }
        public string ModuleCode { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Credits { get; set; }
        public string LecturerFullName { get; set; } = "Not Assigned";
    }
}
