using System;

namespace Ynost.Models;

public class ExperimentalProject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TeacherId { get; set; }        // ← добавить
    public string Name { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
}
