using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ynost.Models
{
    public class AcademicResultMetric
    {
        public Guid Id { get; set; }
        public Guid TeachId { get; set; }
        public string AcademicYear { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Quarter { get; set; } = null!;   // "I2" … "Y"
        public string Kach { get; set; } = "";
        public string Usp { get; set; } = "";
        public string Sou { get; set; } = "";
    }
}