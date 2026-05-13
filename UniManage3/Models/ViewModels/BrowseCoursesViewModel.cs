using System.Collections.Generic;

namespace UniManage3.ViewModels
{
    public class BrowseCoursesViewModel
    {
        public List<DepartmentGroupViewModel> Departments { get; set; } = new();
    }

    public class DepartmentGroupViewModel
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<CourseCardViewModel> Courses { get; set; } = new();
    }

    public class CourseCardViewModel
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Credits { get; set; }
        public string? Duration { get; set; }
        public int ModuleCount { get; set; }
        public int SemesterCount { get; set; }
        public List<ModuleInfoViewModel> Modules { get; set; } = new();
    }

    public class ModuleInfoViewModel
    {
        public string ModuleCode { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Credits { get; set; }
    }
}
