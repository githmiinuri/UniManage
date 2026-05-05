using System.Collections.Generic;
using UniManage3.Models; // Ensure this matches your project's namespace

namespace UniManage3.Models // or .ViewModels
{
    public class CourseCurriculumViewModel
    {
        // The main course being managed
        public Course Course { get; set; }

        // The list of semesters belonging to this course
        public List<Semester> Semesters { get; set; }

        // A list of modules not yet assigned to a semester
        public List<Module> UnassignedModules { get; set; }

        // Properties to capture form data when assigning a module
        public int SelectedModuleId { get; set; }
        public int SelectedSemesterId { get; set; }
    }
}
