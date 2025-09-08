using System;

namespace Ynost.Models
{
    public class GiaResultTeacher
    {
        public Guid Id { get; set; }
        public Guid TeachId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string TotalListed { get; set; } = string.Empty;
        public string TotalParticipated { get; set; } = string.Empty;
        public string Pct8199 { get; set; } = string.Empty;
        public string Pct6180 { get; set; } = string.Empty;
        public string Pct060 { get; set; } = string.Empty;
        public string PctBelowMin { get; set; } = string.Empty;
        public string DocScan { get; set; } = string.Empty;
    }
}
