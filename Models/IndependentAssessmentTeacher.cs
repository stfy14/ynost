using System;

namespace Ynost.Models
{
    public class IndependentAssessmentTeacher
    {
        public Guid Id { get; set; }
        public Guid TeachId { get; set; }
        public string AssessmentType { get; set; } = string.Empty;
        public string AssessmentDate { get; set; } = string.Empty;
        public string ClassSubject { get; set; } = string.Empty;
        public string StudentsTotal { get; set; } = string.Empty;
        public string StudentsParticipatedCnt { get; set; } = string.Empty;
        public string StudentsParticipatedPct { get; set; } = string.Empty;
        public string StudentsPassedCnt { get; set; } = string.Empty;
        public string StudentsPassedPct { get; set; } = string.Empty;
        public string DocScan { get; set; } = string.Empty;
    }
}
