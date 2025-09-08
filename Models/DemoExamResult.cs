using System;

namespace Ynost.Models;

public class DemoExamResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TeacherId { get; set; }        // ← добавить
    public string Subject { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string TotalParticipants { get; set; } = string.Empty;
    public string Count5 { get; set; } = string.Empty;
    public string Count4 { get; set; } = string.Empty;
    public string Count3 { get; set; } = string.Empty;
    public string Count2 { get; set; } = string.Empty;
    public string AvgScore { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
}
