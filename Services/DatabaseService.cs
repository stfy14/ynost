using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Newtonsoft.Json;
using Ynost.Models;
using System.Collections.ObjectModel;

namespace Ynost.Services
{
    /// <summary>
    /// Сервис доступа к PostgreSQL: загрузка/сохранение базовых преподавателей
    /// и мониторинга учителей по новым таблицам.
    /// </summary>
    internal static class SnakeHelper
    {
        public static string ToSnake(this string s) =>
            string.Concat(s.Select((c, i) =>
                i > 0 && char.IsUpper(c)
                    ? "_" + char.ToLowerInvariant(c)
                    : char.ToLowerInvariant(c).ToString()));
    }
    public class DatabaseService
    {
        private readonly string _cs;
        private readonly string _cachePath;
        public string ConnectionString => _cs;
        public DatabaseService(string connectionString)
        {
            _cs = connectionString;
            _cachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "teachers_cache.json");
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        }

        #region Helpers

        private static NpgsqlConnection Conn(string cs) => new(cs);



        #endregion

        #region LoadAll / SaveAll for Teacher

        public async Task<List<Teacher>?> LoadAllAsync()
        {
            try
            {
                await using var db = Conn(_cs);
                await db.OpenAsync();

                var teachers = (await db.QueryAsync<Teacher>("SELECT * FROM teachers")).ToList();
                if (!teachers.Any()) return teachers;

                async Task LoadChildAsync<T>(string table, Action<Teacher, IEnumerable<T>> setter)
                    where T : class
                {
                    var lookup = (await db.QueryAsync<T>($"SELECT * FROM {table}"))
                                 .ToLookup(r => (Guid)typeof(T).GetProperty("TeacherId")!.GetValue(r)!);

                    foreach (var t in teachers)
                        setter(t, lookup[t.Id]);
                }

                await LoadChildAsync<AcademicYearResult>("academic_year_results", (t, r) => t.AcademicResults = r.ToList());
                await LoadChildAsync<IntermediateAssessment>("intermediate_assessments", (t, r) => t.IntermediateAssessments = r.ToList()); // ← ДОБАВЛЕНО
                await LoadChildAsync<GiaResult>("gia_results", (t, r) => t.GiaResults = r.ToList());
                await LoadChildAsync<DemoExamResult>("demo_exam_results", (t, r) => t.DemoExamResults = r.ToList());
                await LoadChildAsync<IndependentAssessment>("independent_assessments", (t, r) => t.IndependentAssessments = r.ToList());
                await LoadChildAsync<SelfDeterminationActivity>("self_determinations", (t, r) => t.SelfDeterminations = r.ToList());
                await LoadChildAsync<StudentOlympiad>("student_olympiads", (t, r) => t.StudentOlympiads = r.ToList());
                await LoadChildAsync<JuryActivity>("jury_activities", (t, r) => t.JuryActivities = r.ToList());
                await LoadChildAsync<MasterClass>("master_classes", (t, r) => t.MasterClasses = r.ToList());
                await LoadChildAsync<Speech>("speeches", (t, r) => t.Speeches = r.ToList());
                await LoadChildAsync<Publication>("publications", (t, r) => t.Publications = r.ToList());
                await LoadChildAsync<ExperimentalProject>("experimental_projects", (t, r) => t.ExperimentalProjects = r.ToList());
                await LoadChildAsync<Mentorship>("mentorships", (t, r) => t.Mentorships = r.ToList());
                await LoadChildAsync<ProgramMethodSupport>("program_supports", (t, r) => t.ProgramSupports = r.ToList());
                await LoadChildAsync<ProfessionalCompetition>("professional_competitions", (t, r) => t.ProfessionalCompetitions = r.ToList());

                return teachers;
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "DB-LOAD");
                return null;
            }
        }
        /// <summary>Чтение 1.1 для одного преподавателя и учебного года.</summary>
        public async Task<List<SubjectQuarterMetric>> LoadSubjectQuarterMetricsAsync(Guid teachId, string year)
        {
            const string sql = @"
SELECT *
FROM subject_quarter_metrics
WHERE teach_id = @teachId
  AND academic_year = @year
ORDER BY subject, quarter;";

            try
            {
                await using var db = Conn(_cs);
                await db.OpenAsync();
                return (await db.QueryAsync<SubjectQuarterMetric>(sql, new { teachId, year })).ToList();
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "LOAD-SQM");
                return new List<SubjectQuarterMetric>();
            }
        }

        /// <summary>Полностью заменяет данные 1.1 для teachId + year.</summary>
        public async Task<bool> SaveSubjectQuarterMetricsAsync(Guid teachId,
                                                               string year,
                                                               IEnumerable<SubjectQuarterMetric> list)
        {
            LastError = null;
            var rows = list.ToList();        // материализуем

            try
            {
                await using var db = Conn(_cs);
                await db.OpenAsync();
                await using var tx = await db.BeginTransactionAsync();

                /* 1. Сносим старые строки этого преподавателя / года */
                await db.ExecuteAsync(
                    @"DELETE FROM subject_quarter_metrics
                      WHERE teach_id = @teachId AND academic_year = @year",
                    new { teachId, year }, tx);

                if (rows.Any())
                {
                    /* 2. Подготавливаем INSERT */
                    var props = typeof(SubjectQuarterMetric).GetProperties()
                                                            .Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                                                            .ToList();

                    var cols = string.Join(",", props.Select(p => p.Name.ToSnake()));
                    var values = string.Join(",", props.Select(p => "@" + p.Name));
                    var sql = $"INSERT INTO subject_quarter_metrics ({cols}) VALUES ({values});";

                    /* 3. Гарантируем TeachId + Year */
                    foreach (var r in rows)
                    {
                        r.TeachId = teachId;
                        r.AcademicYear = year;
                    }

                    await db.ExecuteAsync(sql, rows, tx);
                }

                await tx.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                LastError = FormatError(ex);
                Logger.Write(ex, "SAVE-SQM");
                return false;
            }
        }

        #endregion

        public string? LastError { get; private set; }

        private static string FormatError(Exception ex) =>
            ex is PostgresException pex
                ? $"PostgreSQL: {pex.MessageText} (SQLSTATE {pex.SqlState})"
                : ex.ToString();

        public async Task<bool> SaveAllAsync(IEnumerable<Teacher> teachers, CancellationToken ct = default)
        {
            LastError = null;
            var list = teachers?.ToList() ?? new();

            try
            {
                await using var db = Conn(_cs);
                await db.OpenAsync(ct);
                await using var tx = await db.BeginTransactionAsync(ct);

                const string upsertSql = @"
INSERT INTO teachers (id, full_name, is_lecturer)
VALUES (@Id, @FullName, @IsLecturer)
ON CONFLICT (id) DO UPDATE
SET full_name   = EXCLUDED.full_name,
    is_lecturer = EXCLUDED.is_lecturer;";

                foreach (var t in list)
                {
                    await db.ExecuteAsync(upsertSql, t, tx);

                    await ReplaceAsync(db, tx, "academic_year_results", t.Id, t.AcademicResults);
                    await ReplaceAsync(db, tx, "intermediate_assessments", t.Id, t.IntermediateAssessments); // ← ДОБАВЛЕНО
                    await ReplaceAsync(db, tx, "gia_results", t.Id, t.GiaResults);
                    await ReplaceAsync(db, tx, "demo_exam_results", t.Id, t.DemoExamResults);
                    await ReplaceAsync(db, tx, "independent_assessments", t.Id, t.IndependentAssessments);
                    await ReplaceAsync(db, tx, "self_determinations", t.Id, t.SelfDeterminations);
                    await ReplaceAsync(db, tx, "student_olympiads", t.Id, t.StudentOlympiads);
                    await ReplaceAsync(db, tx, "jury_activities", t.Id, t.JuryActivities);
                    await ReplaceAsync(db, tx, "master_classes", t.Id, t.MasterClasses);
                    await ReplaceAsync(db, tx, "speeches", t.Id, t.Speeches);
                    await ReplaceAsync(db, tx, "publications", t.Id, t.Publications);
                    await ReplaceAsync(db, tx, "experimental_projects", t.Id, t.ExperimentalProjects);
                    await ReplaceAsync(db, tx, "mentorships", t.Id, t.Mentorships);
                    await ReplaceAsync(db, tx, "program_supports", t.Id, t.ProgramSupports);
                    await ReplaceAsync(db, tx, "professional_competitions", t.Id, t.ProfessionalCompetitions);
                }

                await tx.CommitAsync(ct);
                return true;
            }
            catch (Exception ex)
            {
                LastError = FormatError(ex);
                Logger.Write(ex, "DB-SAVE");
                return false;
            }
        }

        private static async Task ReplaceAsync<T>(
            IDbConnection db,
            IDbTransaction tx,
            string table,
            Guid teacherId,
            IEnumerable<T> rows)
        {
            var list = rows.ToList();

            await db.ExecuteAsync(
                $"DELETE FROM {table} WHERE teacher_id = @teacherId",
                new { teacherId }, tx);

            if (!list.Any()) return;

            var props = typeof(T).GetProperties();
            var columnNames = props.Select(p =>
            {
                var snake = p.Name.ToSnake();
                return snake == "group" ? "\"group\"" : snake;
            });
            var columns = string.Join(",", columnNames);
            var values = string.Join(",", props.Select(p => "@" + p.Name));
            var insertSql = $"INSERT INTO {table} ({columns}) VALUES ({values});";

            await db.ExecuteAsync(insertSql, list, tx);
        }

        public async Task<List<AcademicResultMetric>> LoadAcademicResultMetricsAsync(
         Guid teachId, string academicYear)
        {
            const string sql = @"
        SELECT *
        FROM   academic_result_metrics
        WHERE  teach_id      = @teachId
          AND  academic_year = @academicYear
        ORDER  BY subject, quarter";

            try
            {
                await using var db = Conn(_cs);
                await db.OpenAsync();
                return (await db.QueryAsync<AcademicResultMetric>(
                            sql, new { teachId, academicYear }))
                       .ToList();
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "LOAD-ARM");
                return new List<AcademicResultMetric>();
            }
        }

        /// <summary>Полностью заменяет показатели успеваемости (1.x) для учителя и года.</summary>
        /// <returns>true – всё ок; false – исключение, текст в LastError</returns>
        public async Task<bool> SaveAcademicResultMetricsAsync(
                Guid teachId, string academicYear,
                IEnumerable<AcademicResultMetric> rows)
        {
            LastError = null;
            var list = rows.ToList();

            try
            {
                await using var db = Conn(_cs);
                await db.OpenAsync();
                await using var tx = await db.BeginTransactionAsync();

                // 1. удаляем старые записи
                await db.ExecuteAsync(
                    @"DELETE FROM academic_result_metrics
              WHERE teach_id = @teachId AND academic_year = @academicYear",
                    new { teachId, academicYear }, tx);

                // 2. вставляем новые (если есть)
                if (list.Any())
                {
                    // гарантируем TeachId и AcademicYear внутри объектов
                    foreach (var r in list)
                    {
                        r.TeachId = teachId;
                        r.AcademicYear = academicYear;
                    }

                    var insertSql = @"
INSERT INTO academic_result_metrics (
    id, teach_id, academic_year, subject, quarter, kach, usp, sou)
VALUES (
    gen_random_uuid(),          -- id
    @TeachId, @AcademicYear,    -- FK, год
    @Subject, @Quarter,         -- ключ
    @Kach, @Usp, @Sou);";       // показатели

                    await db.ExecuteAsync(insertSql, list, tx);
                }

                await tx.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                LastError = FormatError(ex);
                Logger.Write(ex, "SAVE-ARM");
                return false;
            }
        }

        #region Cache (JSON)

        public async Task SaveToCacheAsync(List<Teacher> teachers)
        {
            try
            {
                var json = JsonConvert.SerializeObject(teachers, Formatting.Indented);
                await File.WriteAllTextAsync(_cachePath, json);
            }
            catch (Exception ex) { Logger.Write(ex, "CACHE-SAVE"); }
        }

        public async Task<List<Teacher>?> LoadFromCacheAsync()
        {
            if (!File.Exists(_cachePath)) return null;
            try
            {
                var json = await File.ReadAllTextAsync(_cachePath);
                return JsonConvert.DeserializeObject<List<Teacher>>(json);
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "CACHE-LOAD");
                return null;
            }
        }

        #endregion

        #region Teacher/Teach Monitoring

        // 1) Load methods for each new table

        public async Task<List<AcademicYearResultTeacher>> LoadAcademicYearResultsTeacherAsync(Guid teachId)
        {
            const string sql = @"
SELECT * FROM academic_year_results_teacher
WHERE teach_id = @teachId";
            try
            {
                await using var db = Conn(_cs);
                await db.OpenAsync();
                return (await db.QueryAsync<AcademicYearResultTeacher>(sql, new { teachId })).ToList();
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "LOAD-AYRT");
                return new List<AcademicYearResultTeacher>();
            }
        }

        public async Task<List<AcademicResultTeacher>> LoadAcademicResultsTeacherAsync(Guid teachId)
        {
            const string sql = @"
SELECT * FROM academic_results_teacher
WHERE teach_id = @teachId";
            try
            {
                await using var db = Conn(_cs);
                await db.OpenAsync();
                return (await db.QueryAsync<AcademicResultTeacher>(sql, new { teachId })).ToList();
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "LOAD-ART");
                return new List<AcademicResultTeacher>();
            }
        }

        public async Task<List<GiaResultTeacher>> LoadGiaResultsTeacherAsync(Guid teachId)
        {
            const string sql = @"
SELECT * FROM gia_results_teacher
WHERE teach_id = @teachId";
            try
            {
                await using var db = Conn(_cs);
                await db.OpenAsync();
                return (await db.QueryAsync<GiaResultTeacher>(sql, new { teachId })).ToList();
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "LOAD-GRT");
                return new List<GiaResultTeacher>();
            }
        }

        public async Task<List<OgeResultTeacher>> LoadOgeResultsTeacherAsync(Guid teachId)
        {
            const string sql = @"
SELECT * FROM oge_results_teacher
WHERE teach_id = @teachId";
            try
            {
                await using var db = Conn(_cs);
                await db.OpenAsync();
                return (await db.QueryAsync<OgeResultTeacher>(sql, new { teachId })).ToList();
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "LOAD-OGE");
                return new List<OgeResultTeacher>();
            }
        }

        public async Task<List<IndependentAssessmentTeacher>> LoadIndependentAssessmentsTeacherAsync(Guid teachId)
        {
            const string sql = @"
SELECT * FROM independent_assessments_teacher
WHERE teach_id = @teachId";
            try
            {
                await using var db = Conn(_cs);
                await db.OpenAsync();
                return (await db.QueryAsync<IndependentAssessmentTeacher>(sql, new { teachId })).ToList();
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "LOAD-IAT");
                return new List<IndependentAssessmentTeacher>();
            }
        }

        public async Task<List<SelfDeterminationTeacher>> LoadSelfDeterminationsTeacherAsync(Guid teachId)
        {
            const string sql = @"
SELECT * FROM self_determinations_teacher
WHERE teach_id = @teachId";
            try
            {
                await using var db = Conn(_cs);
                await db.OpenAsync();
                return (await db.QueryAsync<SelfDeterminationTeacher>(sql, new { teachId })).ToList();
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "LOAD-SDT");
                return new List<SelfDeterminationTeacher>();
            }
        }

        public async Task<List<StudentOlympiadTeacher>> LoadStudentOlympiadsTeacherAsync(Guid teachId)
        {
            const string sql = @"
SELECT * FROM student_olympiads_teacher
WHERE teach_id = @teachId";
            try
            {
                await using var db = Conn(_cs);
                await db.OpenAsync();
                return (await db.QueryAsync<StudentOlympiadTeacher>(sql, new { teachId })).ToList();
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "LOAD-SOT");
                return new List<StudentOlympiadTeacher>();
            }
        }

        public async Task<List<JuryActivityTeacher>> LoadJuryActivitiesTeacherAsync(Guid teachId)
        {
            const string sql = @"
SELECT * FROM jury_activities_teacher
WHERE teach_id = @teachId";
            try
            {
                await using var db = Conn(_cs);
                await db.OpenAsync();
                return (await db.QueryAsync<JuryActivityTeacher>(sql, new { teachId })).ToList();
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "LOAD-JAT");
                return new List<JuryActivityTeacher>();
            }
        }

        public async Task<List<MasterClassTeacher>> LoadMasterClassesTeacherAsync(Guid teachId)
        {
            const string sql = @"
SELECT * FROM master_classes_teacher
WHERE teach_id = @teachId";
            try
            {
                await using var db = Conn(_cs);
                await db.OpenAsync();
                return (await db.QueryAsync<MasterClassTeacher>(sql, new { teachId })).ToList();
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "LOAD-MCT");
                return new List<MasterClassTeacher>();
            }
        }

        public async Task<List<SpeechTeacher>> LoadSpeechesTeacherAsync(Guid teachId)
        {
            const string sql = @"
SELECT * FROM speeches_teacher
WHERE teach_id = @teachId";
            try
            {
                await using var db = Conn(_cs);
                await db.OpenAsync();
                return (await db.QueryAsync<SpeechTeacher>(sql, new { teachId })).ToList();
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "LOAD-ST");
                return new List<SpeechTeacher>();
            }
        }

        public async Task<List<PublicationTeacher>> LoadPublicationsTeacherAsync(Guid teachId)
        {
            const string sql = @"
SELECT * FROM publications_teacher
WHERE teach_id = @teachId";
            try
            {
                await using var db = Conn(_cs);
                await db.OpenAsync();
                return (await db.QueryAsync<PublicationTeacher>(sql, new { teachId })).ToList();
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "LOAD-PT");
                return new List<PublicationTeacher>();
            }
        }

        public async Task<List<ExperimentalProjectTeacher>> LoadExperimentalProjectsTeacherAsync(Guid teachId)
        {
            const string sql = @"
SELECT * FROM experimental_projects_teacher
WHERE teach_id = @teachId";
            try
            {
                await using var db = Conn(_cs);
                await db.OpenAsync();
                return (await db.QueryAsync<ExperimentalProjectTeacher>(sql, new { teachId })).ToList();
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "LOAD-EPT");
                return new List<ExperimentalProjectTeacher>();
            }
        }

        public async Task<List<MentorshipTeacher>> LoadMentorshipsTeacherAsync(Guid teachId)
        {
            const string sql = @"
SELECT * FROM mentorships_teacher
WHERE teach_id = @teachId";
            try
            {
                await using var db = Conn(_cs);
                await db.OpenAsync();
                return (await db.QueryAsync<MentorshipTeacher>(sql, new { teachId })).ToList();
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "LOAD-MTT");
                return new List<MentorshipTeacher>();
            }
        }

        public async Task<List<ProgramSupportTeacher>> LoadProgramSupportsTeacherAsync(Guid teachId)
        {
            const string sql = @"
SELECT * FROM program_supports_teacher
WHERE teach_id = @teachId";
            try
            {
                await using var db = Conn(_cs);
                await db.OpenAsync();
                return (await db.QueryAsync<ProgramSupportTeacher>(sql, new { teachId })).ToList();
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "LOAD-PST");
                return new List<ProgramSupportTeacher>();
            }
        }
        public async Task<List<Teach>> LoadAllTeachesAsync()
        {
            const string sql = @"
        SELECT id, full_name AS FullName
        FROM teach
        ORDER BY full_name;
    ";
            try
            {
                await using var db = Conn(_cs);
                await db.OpenAsync();
                var list = (await db.QueryAsync<Teach>(sql)).ToList();
                return list;
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "LOAD-TEACHES");
                return new List<Teach>();
            }
        }

        public async Task<List<ProfessionalCompetitionTeacher>> LoadProfessionalCompetitionsTeacherAsync(Guid teachId)
        {
            const string sql = @"
SELECT * FROM professional_competitions_teacher
WHERE teach_id = @teachId";
            try
            {
                await using var db = Conn(_cs);
                await db.OpenAsync();
                return (await db.QueryAsync<ProfessionalCompetitionTeacher>(sql, new { teachId })).ToList();
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "LOAD-PCT");
                return new List<ProfessionalCompetitionTeacher>();
            }
        }

        // 2) Общий Replace для мониторинга

        private static async Task ReplaceMonitoringAsync<T>(
     IDbConnection db,
     IDbTransaction tx,
     string table,
     Guid teachId,
     IEnumerable<T> rows)
        {
            var list = rows.ToList();

            // 1. Гарантируем корректный внешний ключ
            foreach (var r in list)
            {
                typeof(T).GetProperty("TeachId")?.SetValue(r, teachId);
                typeof(T).GetProperty("TeacherId")?.SetValue(r, teachId);
            }

            // 2. Удаляем старые строки этого преподавателя
            await db.ExecuteAsync(
                $"DELETE FROM {table} WHERE teach_id = @teachId",
                new { teachId }, tx);

            if (!list.Any())
                return;

            // 3. Формируем INSERT **без** первичного ключа Id —
            //    Postgres сам сгенерирует новый UUID (DEFAULT gen_random_uuid()).
            var props = typeof(T)
                .GetProperties()
                .Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)) // ← ключ исключён
                .ToList();

            var columnNames = props.Select(p => p.Name.ToSnake());
            var columns = string.Join(",", columnNames);
            var values = string.Join(",", props.Select(p => "@" + p.Name));

            var insertSql = $"INSERT INTO {table} ({columns}) VALUES ({values});";

            await db.ExecuteAsync(insertSql, list, tx);
        }


        /// <summary>
        /// Сохраняет все коллекции мониторинга для одного teachId.
        /// </summary>
        public async Task<bool> SaveTeacherMonitoringAsync(
            Guid teachId,
            IEnumerable<AcademicYearResultTeacher> academicYearResults,
            IEnumerable<AcademicResultTeacher> academicResults,
            IEnumerable<GiaResultTeacher> giaResults,
            IEnumerable<OgeResultTeacher> ogeResults,
            IEnumerable<IndependentAssessmentTeacher> independentAssessments,
            IEnumerable<SelfDeterminationTeacher> selfDeterminations,
            IEnumerable<StudentOlympiadTeacher> studentOlympiads,
            IEnumerable<JuryActivityTeacher> juryActivities,
            IEnumerable<MasterClassTeacher> masterClasses,
            IEnumerable<SpeechTeacher> speeches,
            IEnumerable<PublicationTeacher> publications,
            IEnumerable<ExperimentalProjectTeacher> experimentalProjects,
            IEnumerable<MentorshipTeacher> mentorships,
            IEnumerable<ProgramSupportTeacher> programSupports,
            IEnumerable<ProfessionalCompetitionTeacher> professionalCompetitions)
        {
            LastError = null;
            try
            {
                await using var db = Conn(_cs);
                await db.OpenAsync();
                await using var tx = await db.BeginTransactionAsync();

                await ReplaceMonitoringAsync(db, tx, "academic_year_results_teacher", teachId, academicYearResults);
                await ReplaceMonitoringAsync(db, tx, "academic_results_teacher", teachId, academicResults);
                await ReplaceMonitoringAsync(db, tx, "gia_results_teacher", teachId, giaResults);
                await ReplaceMonitoringAsync(db, tx, "oge_results_teacher", teachId, ogeResults);
                await ReplaceMonitoringAsync(db, tx, "independent_assessments_teacher", teachId, independentAssessments);
                await ReplaceMonitoringAsync(db, tx, "self_determinations_teacher", teachId, selfDeterminations);
                await ReplaceMonitoringAsync(db, tx, "student_olympiads_teacher", teachId, studentOlympiads);
                await ReplaceMonitoringAsync(db, tx, "jury_activities_teacher", teachId, juryActivities);
                await ReplaceMonitoringAsync(db, tx, "master_classes_teacher", teachId, masterClasses);
                await ReplaceMonitoringAsync(db, tx, "speeches_teacher", teachId, speeches);
                await ReplaceMonitoringAsync(db, tx, "publications_teacher", teachId, publications);
                await ReplaceMonitoringAsync(db, tx, "experimental_projects_teacher", teachId, experimentalProjects);
                await ReplaceMonitoringAsync(db, tx, "mentorships_teacher", teachId, mentorships);
                await ReplaceMonitoringAsync(db, tx, "program_supports_teacher", teachId, programSupports);
                await ReplaceMonitoringAsync(db, tx, "professional_competitions_teacher", teachId, professionalCompetitions);

                await tx.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                LastError = FormatError(ex);
                Logger.Write(ex, "DB-SAVE-MON");
                return false;
            }
        }
        public async Task<bool> DeleteTeacherAsync(Guid teacherId)
        {
            LastError = null;
            try
            {
                await using var db = Conn(_cs);
                await db.OpenAsync();
                await using var tx = await db.BeginTransactionAsync();

                // Список дочерних таблиц для ПРЕПОДАВАТЕЛЕЙ
                var childTables = new[]
                {
                    "academic_year_results", "intermediate_assessments", "gia_results", "demo_exam_results", // ← ДОБАВЛЕНО
                    "independent_assessments", "self_determinations", "student_olympiads",
                    "jury_activities", "master_classes", "speeches", "publications",
                    "experimental_projects", "mentorships", "program_supports",
                    "professional_competitions"
                };

                // Удаляем связанные записи
                foreach (var table in childTables)
                {
                    await db.ExecuteAsync($"DELETE FROM {table} WHERE teacher_id = @teacherId", new { teacherId }, tx);
                }

                // Удаляем самого преподавателя
                await db.ExecuteAsync("DELETE FROM teachers WHERE id = @teacherId", new { teacherId }, tx);

                await tx.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                LastError = FormatError(ex);
                Logger.Write(ex, "DB-DELETE-TEACHER");
                return false;
            }
        }
        public async Task<Teach> AddTeachAsync(string fullName)
        {
            LastError = null;
            const string sql = "INSERT INTO teach (full_name) VALUES (@fullName) RETURNING id, full_name";
            try
            {
                await using var db = Conn(_cs);
                return await db.QuerySingleAsync<Teach>(sql, new { fullName });
            }
            catch (Exception ex)
            {
                LastError = FormatError(ex);
                Logger.Write(ex, "DB-ADD-TEACH");
                return null;
            }
        }
        public async Task<bool> UpdateTeachNameAsync(Guid teachId, string newName)
        {
            LastError = null;
            const string sql = "UPDATE teach SET full_name = @newName WHERE id = @teachId";
            try
            {
                await using var db = Conn(_cs);
                await db.ExecuteAsync(sql, new { teachId, newName });
                return true;
            }
            catch (Exception ex)
            {
                LastError = FormatError(ex);
                Logger.Write(ex, "DB-UPDATE-TEACH-NAME");
                return false;
            }
        }
        public async Task<bool> DeleteTeachAsync(Guid teachId)
        {
            LastError = null;
            try
            {
                await using var db = Conn(_cs);
                await db.OpenAsync();
                await using var tx = await db.BeginTransactionAsync();

                // Список дочерних таблиц для УЧИТЕЛЕЙ (мониторинг)
                var childTables = new[]
                {
                    "academic_year_results_teacher", "academic_results_teacher",
                    "gia_results_teacher", "oge_results_teacher", "independent_assessments_teacher",
                    "self_determinations_teacher", "student_olympiads_teacher",
                    "jury_activities_teacher", "master_classes_teacher", "speeches_teacher",
                    "publications_teacher", "experimental_projects_teacher",
                    "mentorships_teacher", "program_supports_teacher",
                    "professional_competitions_teacher", "subject_quarter_metrics"
                };

                // Удаляем связанные записи
                foreach (var table in childTables)
                {
                    // В этих таблицах ключ называется teach_id
                    await db.ExecuteAsync($"DELETE FROM {table} WHERE teach_id = @teachId", new { teachId }, tx);
                }

                // Удаляем самого учителя
                await db.ExecuteAsync("DELETE FROM teach WHERE id = @teachId", new { teachId }, tx);

                await tx.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                LastError = FormatError(ex);
                Logger.Write(ex, "DB-DELETE-TEACH");
                return false;
            }
        }
        internal async Task<bool> SaveTeacherMonitoringAsync(int id, ObservableCollection<AcademicYearResultTeacher> academicYearResults, IEnumerable<AcademicResultTeacher> enumerable1, IEnumerable<GiaResultTeacher> enumerable2, IEnumerable<OgeResultTeacher> enumerable3, IEnumerable<IndependentAssessmentTeacher> enumerable4, IEnumerable<SelfDeterminationTeacher> enumerable5, IEnumerable<StudentOlympiadTeacher> enumerable6, IEnumerable<JuryActivityTeacher> enumerable7, IEnumerable<MasterClassTeacher> enumerable8, IEnumerable<SpeechTeacher> enumerable9, IEnumerable<PublicationTeacher> enumerable10, IEnumerable<ExperimentalProjectTeacher> enumerable11, IEnumerable<MentorshipTeacher> enumerable12, IEnumerable<ProgramSupportTeacher> enumerable13, IEnumerable<ProfessionalCompetitionTeacher> enumerable14)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}