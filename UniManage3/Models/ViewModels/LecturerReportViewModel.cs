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
        // Class average of graded submissions. Defaults to 0 when there are no graded submissions.
        public double ClassAverage { get; set; }
        public int LateSubmissions { get; set; }
    }
}
