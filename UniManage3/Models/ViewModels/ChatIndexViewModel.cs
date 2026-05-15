using System.Collections.Generic;

namespace UniManage3.Models.ViewModels
{
    public class ChatIndexViewModel
    {
        public List<StudentDirectoryItemViewModel> Students { get; set; }
        public List<BatchViewModel> Batches { get; set; }
    }

    public class StudentDirectoryItemViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public int? ContactNumber { get; set; }
        public string NICNumber { get; set; }
        public int? BatchId { get; set; }
    }

    public class BatchViewModel
    {
        public int Id { get; set; }
        public string BatchName { get; set; }
    }
}
