using System;

namespace Ynost.Models
{
    public class MentorshipTeacher
    {
        public Guid Id { get; set; }
        public Guid TeachId { get; set; }
        public string TraineeName { get; set; } = string.Empty;
        public string OrderNo { get; set; } = string.Empty;
        public string OrderDate { get; set; } = string.Empty;
        public string DocScan { get; set; } = string.Empty;
    }
}
