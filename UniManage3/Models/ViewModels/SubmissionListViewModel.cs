using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace UniManage3.Models.ViewModels
{
    using System;
    public class SubmissionListViewModel
    {
        public IEnumerable<UniManage3.Models.AssignmentSubmission> Submissions { get; set; } = new List<UniManage3.Models.AssignmentSubmission>();
        public SelectList Batches { get; set; }
        public int? SelectedBatchId { get; set; }
    }
}
