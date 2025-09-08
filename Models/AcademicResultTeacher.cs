using System;

namespace Ynost.Models
{
    public class AcademicResultTeacher
    {
        public Guid TeachId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string AvgSuccess { get; set; } = string.Empty;
        public string AvgQuality { get; set; } = string.Empty;
        public string SouRate { get; set; } = string.Empty;
        public string DocScan { get; set; } = string.Empty;
    }
}
