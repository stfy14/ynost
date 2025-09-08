// File: Models/SubjectQuarterMetric.cs
using System;

namespace Ynost.Models
{
    /// <summary>Запись блока 1.1: показатели по предмету за четверть.</summary>
    public class SubjectQuarterMetric
    {
        public Guid Id { get; set; }
        public Guid TeachId { get; set; }

        public string AcademicYear { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Quarter { get; set; } = null!;   // "I2" … "Y"

        // ← теперь строки, т.к. в БД varchar
        public string Kach { get; set; } = "";   // храним «87.5» или «87,5»
        public string Usp { get; set; } = "";
        public string Sou { get; set; } = "";
    }
}