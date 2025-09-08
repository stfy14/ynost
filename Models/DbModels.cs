using System;

namespace Ynost.Models
{
    // 1. Итоговые результаты успеваемости
    public record AcademicYearResultDb(
        Guid Id, Guid TeacherId, string Group, string AcademicPeriod, string Subject,
        string AvgSem1, string ResultATest, string AvgSem2, string DynamicsSem, string AvgSuccessRate, string DynamicsAvgSuccessRate,
        string AvgQualityRate, string DynamicsAvgQualityRate, string EntrySouRate, string ExitSouRate, string Link);

    // 2. ГИА
    public record GiaResultDb(
        Guid Id, Guid TeacherId, string Subject, string Group, string TotalParticipants,
        string Count5, string Count4, string Count3, string Count2,
        string AvgScore, string Link);

    // 3. ДЭ
    public record DemoExamResultDb(
        Guid Id, Guid TeacherId, string Subject, string Group, string TotalParticipants,
        string Count5, string Count4, string Count3, string Count2,
        string AvgScore, string Link);

    // 4. Независимая оценка
    public record IndependentAssessmentDb(
        Guid Id, Guid TeacherId, string AssessmentName, string AssessmentDate,
        string ClassSubject, string StudentsTotal, string StudentsParticipated,
        string StudentsPassed, string Count5, string Count4, string Count3, string Count2,
        string Link);

    // 5. Раннее самоопределение
    public record SelfDeterminationActivityDb(
        Guid Id, Guid TeacherId, string Level, string Name, string Role, string Link);

    // 6. Олимпиады / конкурсы студентов
    public record StudentOlympiadDb(
        Guid Id, Guid TeacherId, string Level, string Name, string Form,
        string Cadet, string Result, string Link);

    // 7. Работа в жюри
    public record JuryActivityDb(
        Guid Id, Guid TeacherId, string Level, string Name, string EventDate, string Link);

    // 8. Мастер-классы / открытые занятия
    public record MasterClassDb(
        Guid Id, Guid TeacherId, string Level, string Name, string EventDate, string Link);

    // 9. Выступления
    public record SpeechDb(
        Guid Id, Guid TeacherId, string Level, string Name, string EventDate, string Link);

    // 10. Публикации
    public record PublicationDb(
        Guid Id, Guid TeacherId, string Level, string Title, string PubDate, string Link);

    // 11. Экспериментальные проекты
    public record ExperimentalProjectDb(
        Guid Id, Guid TeacherId, string Name, string ProjDate, string Link);

    // 12. Наставничество
    public record MentorshipDb(
        Guid Id, Guid TeacherId, string Trainee, string OrderNo, string OrderDate, string Link);

    // 13. Программно-методическое сопровождение
    public record ProgramMethodSupportDb(
        Guid Id, Guid TeacherId, string ProgramName, string HasControlMaterials, string Link);

    // 14. Профессиональные конкурсы
    public record ProfessionalCompetitionDb(
        Guid Id, Guid TeacherId, string Level, string Name, string Achievement,
        string EventDate, string Link);
}
