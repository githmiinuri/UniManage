using System;
using System.ComponentModel.DataAnnotations;

namespace UniManage3.Models
{
    public enum SubjectCategory
    {
        General = 0,
        Assignment = 1,
        Grades = 2,
        Attendance = 3,
        Other = 99
    }

    public class CommunicationHub
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }
        public int? LecturerId { get; set; }

        public SubjectCategory SubjectCategory { get; set; }

        // Student's initial message
        public string MessageContent { get; set; }

        // Lecturer's reply
        public string AdminReply { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public DateTime? RepliedAt { get; set; }

        public bool IsReadByStudent { get; set; } = false;
    }
}