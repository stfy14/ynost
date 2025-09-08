using System;

namespace Ynost.Models
{
    public class StudentOlympiadTeacher
    {
        public Guid Id { get; set; }
        public Guid TeachId { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Form { get; set; } = string.Empty;
        public string CadetName { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public string DocScan { get; set; } = string.Empty;
    }
}
