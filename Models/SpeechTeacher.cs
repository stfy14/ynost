using System;

namespace Ynost.Models
{
    public class SpeechTeacher
    {
        public Guid Id { get; set; }
        public Guid TeachId { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string EventDate { get; set; } = string.Empty;
        public string DocScan { get; set; } = string.Empty;
    }
}
