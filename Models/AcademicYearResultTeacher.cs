// File: Models/AcademicYearResultTeacher.cs
using System;

namespace Ynost.Models
{
    public class AcademicYearResultTeacher
    {
        public Guid Id { get; set; }              // PK
        public Guid TeachId { get; set; }         // FK на teach.id
        public string AcademicYear { get; set; }  // "2024–2025"
        public string AvgSem1Start { get; set; }
        public string AvgYearEnd { get; set; }
        public string Dynamics { get; set; }
        public string DocScan { get; set; }       // путь или имя файла
    }
}
