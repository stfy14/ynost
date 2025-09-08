using System;

namespace Ynost.Models
{
    public class PublicationTeacher
    {
        public Guid Id { get; set; }
        public Guid TeachId { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string PubDate { get; set; } = string.Empty;
        public string DocScan { get; set; } = string.Empty;
    }
}
