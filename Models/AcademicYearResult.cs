using System;

namespace Ynost.Models;

public class AcademicYearResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TeacherId { get; set; }        // ← добавить
    public string Group { get; set; } = string.Empty;
    public string AcademicPeriod { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string AvgSem1 { get; set; } = string.Empty;
    public string ResultATest { get; set; } = string.Empty; //new
    public string AvgSem2 { get; set; } = string.Empty;
    public string DynamicsSem { get; set; } = string.Empty;
    public string AvgSuccessRate { get; set; } = string.Empty;
    public string DynamicsAvgSuccessRate { get; set; } = string.Empty; //new
    public string AvgQualityRate { get; set; } = string.Empty;
    public string DynamicsAvgQualityRate { get; set; } = string.Empty; //new
    public string EntrySouRate { get; set; } = string.Empty;
    public string ExitSouRate { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
}
