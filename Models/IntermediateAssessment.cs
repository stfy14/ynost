using System;

namespace Ynost.Models
{
    /// <summary>
    /// Модель для хранения результатов промежуточной аттестации.
    /// </summary>
    public class IntermediateAssessment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TeacherId { get; set; }
        public string AcademicYear { get; set; } = string.Empty; // Учебный год
        public string Subject { get; set; } = string.Empty;      // Предмет
        public string AvgScore { get; set; } = string.Empty;     // СР БАЛ
        public string Quality { get; set; } = string.Empty;      // КАЧЕСТВО
        public string Sou { get; set; } = string.Empty;          // СОУ
        public string Link { get; set; } = string.Empty;         // Ссылка на документ
    }
}