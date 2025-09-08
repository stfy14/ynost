using System;

namespace Ynost.Models
{
    public class ProgramSupportTeacher
    {
        public Guid Id { get; set; }
        public Guid TeachId { get; set; }
        public string ProgramName { get; set; } = string.Empty;
        public string HasMaterials { get; set; } = string.Empty;
        public string DocScan { get; set; } = string.Empty;
    }
}
