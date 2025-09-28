// File: Models/IChangeTrackable.cs
using System;

namespace Ynost.Models
{
    /// <summary>
    /// Определяет сущность, которая имеет идентификатор и версию для отслеживания изменений.
    /// </summary>
    public interface IChangeTrackable
    {
        Guid Id { get; }
        int Version { get; set; }
    }
}