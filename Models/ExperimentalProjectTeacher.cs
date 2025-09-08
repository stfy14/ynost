using System;

namespace Ynost.Models
{
    public class ExperimentalProjectTeacher
    {
        public Guid Id { get; set; }
        public Guid TeachId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string EventDate { get; set; } = string.Empty;
        public string DocScan { get; set; } = string.Empty;
    }
}
