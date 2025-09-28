using CommunityToolkit.Mvvm.ComponentModel;
using Ynost.Models;

namespace Ynost.Models;

public partial class Teacher : ObservableObject, IChangeTrackable
{
    [ObservableProperty]
    private Guid _id = Guid.NewGuid();
    [ObservableProperty]
    private Guid _teacherId;
    [ObservableProperty]
    private string _fullName = string.Empty;
    [ObservableProperty]
    private bool _isLecturer;

    [ObservableProperty]
    private List<AcademicYearResult> _academicResults = new();
    [ObservableProperty]
    private List<IntermediateAssessment> _intermediateAssessments = new();
    [ObservableProperty]
    private List<GiaResult> _giaResults = new();
    [ObservableProperty]
    private List<DemoExamResult> _demoExamResults = new();
    [ObservableProperty]
    private List<IndependentAssessment> _independentAssessments = new();
    [ObservableProperty]
    private List<SelfDeterminationActivity> _selfDeterminations = new();
    [ObservableProperty]
    private List<StudentOlympiad> _studentOlympiads = new();
    [ObservableProperty]
    private List<JuryActivity> _juryActivities = new();
    [ObservableProperty]
    private List<MasterClass> _masterClasses = new();
    [ObservableProperty]
    private List<Speech> _speeches = new();
    [ObservableProperty]
    private List<Publication> _publications = new();
    [ObservableProperty]
    private List<ExperimentalProject> _experimentalProjects = new();
    [ObservableProperty]
    private List<Mentorship> _mentorships = new();
    [ObservableProperty]
    private List<ProgramMethodSupport> _programSupports = new();
    [ObservableProperty]
    private List<ProfessionalCompetition> _professionalCompetitions = new();

    [ObservableProperty]
    private int _version = 1;

    [ObservableProperty]
    [System.Text.Json.Serialization.JsonIgnore]
    private bool _isConflicting;
}