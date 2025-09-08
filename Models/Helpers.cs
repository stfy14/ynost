using System;

namespace Ynost.Models;

public static class Helpers
{
    // 1
    public static AcademicYearResultDb ToDb(this AcademicYearResult src, Guid teacherId) =>
        new(src.Id, teacherId, src.Group, src.AcademicPeriod, src.Subject,
            src.AvgSem1, src.ResultATest, src.AvgSem2, src.DynamicsSem, src.AvgSuccessRate,
            src.DynamicsAvgSuccessRate, src.AvgQualityRate, src.DynamicsAvgQualityRate, src.EntrySouRate, src.ExitSouRate, src.Link);

    // 2
    public static GiaResultDb ToDb(this GiaResult src, Guid teacherId) =>
        new(src.Id, teacherId, src.Subject, src.Group, src.TotalParticipants,
            src.Count5, src.Count4, src.Count3, src.Count2, src.AvgScore, src.Link);

    // 3
    public static DemoExamResultDb ToDb(this DemoExamResult src, Guid teacherId) =>
        new(src.Id, teacherId, src.Subject, src.Group, src.TotalParticipants,
            src.Count5, src.Count4, src.Count3, src.Count2, src.AvgScore, src.Link);

    // 4
    public static IndependentAssessmentDb ToDb(this IndependentAssessment src, Guid teacherId) =>
        new(src.Id, teacherId, src.AssessmentName, src.AssessmentDate, 
            src.ClassSubject, src.StudentsTotal, src.StudentsParticipated, 
            src.StudentsPassed, src.Count5, src.Count4, src.Count3, src.Count2, 
            src.Link);

    // 5
    public static SelfDeterminationActivityDb ToDb(this SelfDeterminationActivity src, Guid teacherId) =>
        new(src.Id, teacherId, src.Level, src.Name, src.Role, src.Link);

    // 6
    public static StudentOlympiadDb ToDb(this StudentOlympiad src, Guid teacherId) =>
        new(src.Id, teacherId, src.Level, src.Name, src.Form,
            src.Cadet, src.Result, src.Link);

    // 7
    public static JuryActivityDb ToDb(this JuryActivity src, Guid teacherId) =>
        new(src.Id, teacherId, src.Level, src.Name, src.EventDate, src.Link);

    // 8
    public static MasterClassDb ToDb(this MasterClass src, Guid teacherId) =>
        new(src.Id, teacherId, src.Level, src.Name, src.EventDate, src.Link);

    // 9
    public static SpeechDb ToDb(this Speech src, Guid teacherId) =>
        new(src.Id, teacherId, src.Level, src.Name, src.EventDate, src.Link);

    // 10
    public static PublicationDb ToDb(this Publication src, Guid teacherId) =>
        new(src.Id, teacherId, src.Level, src.Title, src.Date, src.Link);

    // 11
    public static ExperimentalProjectDb ToDb(this ExperimentalProject src, Guid teacherId) =>
        new(src.Id, teacherId, src.Name, src.Date, src.Link);

    // 12
    public static MentorshipDb ToDb(this Mentorship src, Guid teacherId) =>
        new(src.Id, teacherId, src.Trainee, src.OrderNo, src.OrderDate, src.Link);

    // 13
    public static ProgramMethodSupportDb ToDb(this ProgramMethodSupport src, Guid teacherId) =>
        new(src.Id, teacherId, src.ProgramName, src.HasControlMaterials.ToString(), src.Link);

    // 14
    public static ProfessionalCompetitionDb ToDb(this ProfessionalCompetition src, Guid teacherId) =>
        new(src.Id, teacherId, src.Level, src.Name, src.Achievement, src.EventDate, src.Link);
}
