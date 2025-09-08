using System;

namespace Ynost.Models;

public class StudentOlympiad
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TeacherId { get; set; }        // ← добавить
    public string Level { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Form { get; set; } = string.Empty;
    public string Cadet { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
}
