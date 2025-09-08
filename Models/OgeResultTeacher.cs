using System;

namespace Ynost.Models
{
    public class OgeResultTeacher
    {
        public Guid Id { get; set; }
        public Guid TeachId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string TotalListed { get; set; } = string.Empty;
        public string TotalParticipated { get; set; } = string.Empty;
        public string Cnt5 { get; set; } = string.Empty;
        public string Cnt4 { get; set; } = string.Empty;
        public string Cnt3 { get; set; } = string.Empty;
        public string Cnt2 { get; set; } = string.Empty;
        public string AvgScore { get; set; } = string.Empty;
        public string DocScan { get; set; } = string.Empty;
    }
}
