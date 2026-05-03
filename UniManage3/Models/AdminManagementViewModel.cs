using System;
using System.Collections.Generic;

namespace UniManage3.Models
{
    public class AdminItemDto
    {
        public int AdminId { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string ContactNumber { get; set; }
        public string NICNumber { get; set; }
        public string ZipCode { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminManagementViewModel
    {
        public string SelectedTab { get; set; } = "pending";
        public IEnumerable<AdminItemDto> PendingAdmins { get; set; }
        public IEnumerable<AdminItemDto> ActiveAdmins { get; set; }
        public IEnumerable<AdminItemDto> SuspendedAdmins { get; set; }
        public string SearchTerm { get; set; }
        // counts for quick display
        public int PendingCount { get; set; }
        public int ActiveCount { get; set; }
        public int SuspendedCount { get; set; }
    }
}