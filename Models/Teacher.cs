namespace Ynost.Models;

public class Teacher
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TeacherId { get; set; }        // ← добавить
    public string FullName { get; set; } = string.Empty;
    public bool IsLecturer { get; set; }

    /* ------------ 14 списков — делаем set; чтобы ViewModel могла перезаписать -------- */
    public List<AcademicYearResult> AcademicResults { get; set; } = new();
    public List<GiaResult> GiaResults { get; set; } = new();
    public List<DemoExamResult> DemoExamResults { get; set; } = new();
    public List<IndependentAssessment> IndependentAssessments { get; set; } = new();
    public List<SelfDeterminationActivity> SelfDeterminations { get; set; } = new();
    public List<StudentOlympiad> StudentOlympiads { get; set; } = new();
    public List<JuryActivity> JuryActivities { get; set; } = new();
    public List<MasterClass> MasterClasses { get; set; } = new();
    public List<Speech> Speeches { get; set; } = new();
    public List<Publication> Publications { get; set; } = new();
    public List<ExperimentalProject> ExperimentalProjects { get; set; } = new();
    public List<Mentorship> Mentorships { get; set; } = new();
    public List<ProgramMethodSupport> ProgramSupports { get; set; } = new();
    public List<ProfessionalCompetition> ProfessionalCompetitions { get; set; } = new();
}
    