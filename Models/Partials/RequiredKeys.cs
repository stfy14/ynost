using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ynost.Models.Partials
{

        public interface ITeacherKey           // нужна только тут
        {
            Guid Id { get; set; }
            Guid TeachId { get; set; }
        }

        public partial class AcademicYearResultTeacher : ITeacherKey { public Guid Id { get; set; } public Guid TeachId { get; set; } }
        public partial class AcademicResultTeacher : ITeacherKey { public Guid Id { get; set; } public Guid TeachId { get; set; } }
        public partial class GiaResultTeacher : ITeacherKey { public Guid Id { get; set; } public Guid TeachId { get; set; } }
        public partial class OgeResultTeacher : ITeacherKey { public Guid Id { get; set; } public Guid TeachId { get; set; } }
        public partial class IndependentAssessmentTeacher : ITeacherKey { public Guid Id { get; set; } public Guid TeachId { get; set; } }
        public partial class SelfDeterminationTeacher : ITeacherKey { public Guid Id { get; set; } public Guid TeachId { get; set; } }
        public partial class StudentOlympiadTeacher : ITeacherKey { public Guid Id { get; set; } public Guid TeachId { get; set; } }
        public partial class JuryActivityTeacher : ITeacherKey { public Guid Id { get; set; } public Guid TeachId { get; set; } }
        public partial class MasterClassTeacher : ITeacherKey { public Guid Id { get; set; } public Guid TeachId { get; set; } }
        public partial class SpeechTeacher : ITeacherKey { public Guid Id { get; set; } public Guid TeachId { get; set; } }
        public partial class PublicationTeacher : ITeacherKey { public Guid Id { get; set; } public Guid TeachId { get; set; } }
        public partial class ExperimentalProjectTeacher : ITeacherKey { public Guid Id { get; set; } public Guid TeachId { get; set; } }
        public partial class MentorshipTeacher : ITeacherKey { public Guid Id { get; set; } public Guid TeachId { get; set; } }
        public partial class ProgramSupportTeacher : ITeacherKey { public Guid Id { get; set; } public Guid TeachId { get; set; } }
        public partial class ProfessionalCompetitionTeacher : ITeacherKey { public Guid Id { get; set; } public Guid TeachId { get; set; } }
    }


