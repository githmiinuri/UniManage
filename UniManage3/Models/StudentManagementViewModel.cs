using System;
using System.Collections.Generic;

namespace UniManage3.Models
{
    public class StudentItemDto
    {
        public int StudentId { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string ContactNumber { get; set; }
        public string NICNumber { get; set; }
        public string ZipCode { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class StudentManagementViewModel
    {
        public string SelectedTab { get; set; } = "pending";
        public IEnumerable<StudentItemDto> PendingStudents { get; set; }
        public IEnumerable<StudentItemDto> ActiveStudents { get; set; }
        public IEnumerable<StudentItemDto> SuspendedStudents { get; set; }
        // preserve search term to show in input
        public string SearchTerm { get; set; }
    }
}
