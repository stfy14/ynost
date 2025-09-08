// Models/ITeacherOwned.cs
using System;

namespace Ynost.Models
{
    public interface ITeacherOwned
    {
        Guid Id { get; set; }
        Guid TeachId { get; set; }
    }
}
