using System;
using System.Collections.Generic;

namespace UniManage3.Models
{
    public class LecturerItemDto
    {
        public int LecturerId { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string ContactNumber { get; set; }
        public string NICNumber { get; set; }
        public string ZipCode { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class LecturerManagementViewModel
    {
        public string SelectedTab { get; set; } = "pending";
        public IEnumerable<LecturerItemDto> PendingLecturers { get; set; }
        public IEnumerable<LecturerItemDto> ActiveLecturers { get; set; }
        public IEnumerable<LecturerItemDto> SuspendedLecturers { get; set; }
        public string SearchTerm { get; set; }
    }
}
