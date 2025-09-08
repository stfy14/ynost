using System;

namespace Ynost.Models;

public class IndependentAssessment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TeacherId { get; set; }        // ← добавить
    public string AssessmentName { get; set; } = string.Empty;
    public string AssessmentDate { get; set; } = string.Empty;   // храните как текст
    public string ClassSubject { get; set; } = string.Empty;
    public string StudentsTotal { get; set; } = string.Empty;
    public string StudentsParticipated { get; set; } = string.Empty;
    public string StudentsPassed { get; set; } = string.Empty;
    public string Count5 { get; set; } = string.Empty;
    public string Count4 { get; set; } = string.Empty;
    public string Count3 { get; set; } = string.Empty;
    public string Count2 { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
}
