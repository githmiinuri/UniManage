using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniManage3.Models;

namespace UniManage3.Models.ViewModels
{
    public class LecturerReportViewModel
    {
        public SelectList Modules { get; set; }
        public SelectList Batches { get; set; }

        public int? SelectedModuleId { get; set; }
        public int? SelectedBatchId { get; set; }

        public IEnumerable<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();

        // KPIs
        public int TotalSubmissions { get; set; }
        public double? AverageMarks { get; set; }
        public int LateSubmissions { get; set; }
    }
}
