using System;

namespace Ynost.Models;

public class Mentorship
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TeacherId { get; set; }        // ← добавить
    public string Trainee { get; set; } = string.Empty;
    public string OrderNo { get; set; } = string.Empty;
    public string OrderDate { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
}
