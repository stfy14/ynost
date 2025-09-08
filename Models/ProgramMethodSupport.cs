using System;

namespace Ynost.Models;

public class ProgramMethodSupport
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TeacherId { get; set; }        // ← добавить
    public string ProgramName { get; set; } = string.Empty;
    public bool HasControlMaterials { get; set; }
    public string Link { get; set; } = string.Empty;
}
