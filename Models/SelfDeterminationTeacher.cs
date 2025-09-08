using System;

namespace Ynost.Models
{
    public class SelfDeterminationTeacher
    {
        public Guid Id { get; set; }
        public Guid TeachId { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string DocScan { get; set; } = string.Empty;
    }
}
    