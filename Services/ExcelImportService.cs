using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using Ynost.Models;
using Ynost.ViewModels;
using static Ynost.ViewModels.TeacherMonitoringViewModel;

namespace Ynost.Services
{
    public class ExcelImportService
    {
        public void ImportMonitoringData(string filePath, TeacherMonitoringViewModel vm)
        {
            using (var workbook = new XLWorkbook(filePath))
            {
                // Берем первый лист или лист с названием "Данные мониторинга"
                var worksheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name == "Данные мониторинга") ?? workbook.Worksheets.FirstOrDefault();

                if (worksheet == null) throw new Exception("Не найден лист с данными.");

                // Очищаем текущие коллекции перед импортом
                ClearAllCollections(vm);

                // === 1.1 Итоги освоения (SubjectBoards) ===
                ImportBoards(worksheet, vm);

                // === 2. ГИА (ЕГЭ) ===
                ImportSection(worksheet, "2. Результаты ГИА (ЕГЭ)", vm.GiaResults,
                    row => new GiaResultTeacher
                    {
                        Id = Guid.NewGuid(),
                        TeachId = vm.SelectedTeach!.Id,
                        Subject = GetVal(row, 1),
                        Class = GetVal(row, 2),
                        TotalListed = GetVal(row, 3),
                        TotalParticipated = GetVal(row, 4),
                        Pct8199 = GetVal(row, 5),
                        Pct6180 = GetVal(row, 6),
                        Pct060 = GetVal(row, 7),
                        PctBelowMin = GetVal(row, 8),
                        DocScan = GetVal(row, 9)
                    });

                // === 3. ГИА (ОГЭ) ===
                ImportSection(worksheet, "3. Результаты ГИА (ОГЭ)", vm.OgeResults,
                    row => new OgeResultTeacher
                    {
                        Id = Guid.NewGuid(),
                        TeachId = vm.SelectedTeach!.Id,
                        Subject = GetVal(row, 1),
                        Class = GetVal(row, 2),
                        TotalListed = GetVal(row, 3),
                        TotalParticipated = GetVal(row, 4),
                        Cnt5 = GetVal(row, 5),
                        Cnt4 = GetVal(row, 6),
                        Cnt3 = GetVal(row, 7),
                        Cnt2 = GetVal(row, 8),
                        AvgScore = GetVal(row, 9),
                        DocScan = GetVal(row, 10)
                    });

                // === 4. Независимая оценка (НОКО) ===
                ImportSection(worksheet, "4. Результаты независимой оценки качества", vm.IndependentAssessments,
                    row => new IndependentAssessmentTeacher
                    {
                        Id = Guid.NewGuid(),
                        TeachId = vm.SelectedTeach!.Id,
                        AssessmentType = GetVal(row, 1),
                        AssessmentDate = GetVal(row, 2),
                        ClassSubject = GetVal(row, 3),
                        StudentsTotal = GetVal(row, 4),
                        StudentsParticipatedCnt = GetVal(row, 5),
                        StudentsParticipatedPct = GetVal(row, 6),
                        StudentsPassedCnt = GetVal(row, 7),
                        StudentsPassedPct = GetVal(row, 8),
                        DocScan = GetVal(row, 9)
                    });

                // === 5. Самоопределение ===
                ImportSection(worksheet, "5. Самоопределение и профориентация", vm.SelfDeterminations,
                    row => new SelfDeterminationTeacher
                    {
                        Id = Guid.NewGuid(),
                        TeachId = vm.SelectedTeach!.Id,
                        Level = GetVal(row, 1),
                        Name = GetVal(row, 2),
                        Role = GetVal(row, 3),
                        DocScan = GetVal(row, 4)
                    });

                // === 6. Олимпиады ===
                ImportSection(worksheet, "6. Участие в олимпиадах и конкурсах", vm.StudentOlympiads,
                    row => new StudentOlympiadTeacher
                    {
                        Id = Guid.NewGuid(),
                        TeachId = vm.SelectedTeach!.Id,
                        Level = GetVal(row, 1),
                        Name = GetVal(row, 2),
                        Form = GetVal(row, 3),
                        CadetName = GetVal(row, 4),
                        Result = GetVal(row, 5),
                        DocScan = GetVal(row, 6)
                    });

                // === 7. Жюри ===
                ImportSection(worksheet, "7. Деятельность в качестве члена жюри", vm.JuryActivities,
                    row => new JuryActivityTeacher
                    {
                        Id = Guid.NewGuid(),
                        TeachId = vm.SelectedTeach!.Id,
                        Level = GetVal(row, 1),
                        Name = GetVal(row, 2),
                        EventDate = GetVal(row, 3),
                        DocScan = GetVal(row, 4)
                    });

                // === 8. Мастер-классы ===
                ImportSection(worksheet, "8. Проведение мастер-классов и мероприятий", vm.MasterClasses,
                    row => new MasterClassTeacher
                    {
                        Id = Guid.NewGuid(),
                        TeachId = vm.SelectedTeach!.Id,
                        Level = GetVal(row, 1),
                        Name = GetVal(row, 2),
                        EventDate = GetVal(row, 3),
                        DocScan = GetVal(row, 4)
                    });

                // === 9. Выступления ===
                ImportSection(worksheet, "9. Выступления на конференциях и семинарах", vm.Speeches,
                    row => new SpeechTeacher
                    {
                        Id = Guid.NewGuid(),
                        TeachId = vm.SelectedTeach!.Id,
                        Level = GetVal(row, 1),
                        Name = GetVal(row, 2),
                        EventDate = GetVal(row, 3),
                        DocScan = GetVal(row, 4)
                    });

                // === 10. Публикации ===
                ImportSection(worksheet, "10. Публикации", vm.Publications,
                    row => new PublicationTeacher
                    {
                        Id = Guid.NewGuid(),
                        TeachId = vm.SelectedTeach!.Id,
                        Level = GetVal(row, 1),
                        Title = GetVal(row, 2),
                        PubDate = GetVal(row, 3),
                        DocScan = GetVal(row, 4)
                    });

                // === 11. Экспериментальные проекты ===
                ImportSection(worksheet, "11. Экспериментальная и инновационная деятельность", vm.ExperimentalProjects,
                    row => new ExperimentalProjectTeacher
                    {
                        Id = Guid.NewGuid(),
                        TeachId = vm.SelectedTeach!.Id,
                        ProjectName = GetVal(row, 1),
                        EventDate = GetVal(row, 2),
                        DocScan = GetVal(row, 3)
                    });

                // === 12. Наставничество ===
                ImportSection(worksheet, "12. Наставничество", vm.Mentorships,
                    row => new MentorshipTeacher
                    {
                        Id = Guid.NewGuid(),
                        TeachId = vm.SelectedTeach!.Id,
                        TraineeName = GetVal(row, 1),
                        OrderNo = GetVal(row, 2),
                        OrderDate = GetVal(row, 3),
                        DocScan = GetVal(row, 4)
                    });

                // === 13. Программно-методическое сопровождение ===
                ImportSection(worksheet, "13. Методическое сопровождение", vm.ProgramSupports,
                    row => new ProgramSupportTeacher
                    {
                        Id = Guid.NewGuid(),
                        TeachId = vm.SelectedTeach!.Id,
                        ProgramName = GetVal(row, 1),
                        HasMaterials = GetVal(row, 2),
                        DocScan = GetVal(row, 3)
                    });

                // === 14. Профессиональные конкурсы ===
                ImportSection(worksheet, "14. Участие в профессиональных конкурсах", vm.ProfessionalCompetitions,
                    row => new ProfessionalCompetitionTeacher
                    {
                        Id = Guid.NewGuid(),
                        TeachId = vm.SelectedTeach!.Id,
                        Level = GetVal(row, 1),
                        Name = GetVal(row, 2),
                        Achievement = GetVal(row, 3),
                        EventDate = GetVal(row, 4),
                        DocScan = GetVal(row, 5)
                    });
            }
        }

        private void ClearAllCollections(TeacherMonitoringViewModel vm)
        {
            vm.YearlyBoards.Clear();
            vm.GiaResults.Clear(); vm.OgeResults.Clear(); vm.IndependentAssessments.Clear();
            vm.SelfDeterminations.Clear(); vm.StudentOlympiads.Clear(); vm.JuryActivities.Clear();
            vm.MasterClasses.Clear(); vm.Speeches.Clear(); vm.Publications.Clear();
            vm.ExperimentalProjects.Clear(); vm.Mentorships.Clear(); vm.ProgramSupports.Clear();
            vm.ProfessionalCompetitions.Clear();
        }

        // --- Хелперы для парсинга ---

        private string GetVal(IXLRangeRow row, int cellIndex)
        {
            return row.Cell(cellIndex).GetValue<string>().Trim();
        }

        private int FindStartRow(IXLWorksheet sheet, string titleSnippet)
        {
            // Ищем ячейку, которая начинается с указанного текста (например "2. Результаты...")
            var cell = sheet.CellsUsed(c => c.GetString().Trim().StartsWith(titleSnippet, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            return cell?.Address.RowNumber ?? -1;
        }

        private void ImportSection<T>(IXLWorksheet sheet, string title, ICollection<T> collection, Func<IXLRangeRow, T> mapper)
        {
            int titleRow = FindStartRow(sheet, title);
            if (titleRow == -1) return; // Раздел не найден

            // Данные начинаются через 2 строки после заголовка (Заголовок -> Шапка -> Данные)
            int startRow = titleRow + 2;

            // Читаем до тех пор, пока не встретим пустую строку или начало следующего раздела (жирный текст в первой колонке)
            int currentRow = startRow;
            while (true)
            {
                var row = sheet.Row(currentRow);
                if (row.Cell(1).IsEmpty()) break; // Пустая строка — конец таблицы

                // Защита: если вдруг наткнулись на следующий заголовок без пустой строки
                var firstCellVal = row.Cell(1).GetString();
                if (firstCellVal.Length > 0 && char.IsDigit(firstCellVal[0]) && firstCellVal.Contains("."))
                {
                    // Похоже на "3. Результаты...", прерываем
                    break;
                }

                try
                {
                    // Используем RangeRow для удобства (отсчет ячеек внутри строки)
                    var rangeRow = sheet.Range(currentRow, 1, currentRow, 20).Row(1);
                    var item = mapper(rangeRow);
                    collection.Add(item);
                }
                catch { /* Игнорируем ошибки парсинга конкретной строки */ }

                currentRow++;
            }
        }

        private void ImportBoards(IXLWorksheet sheet, TeacherMonitoringViewModel vm)
        {
            int titleRow = FindStartRow(sheet, "1.1 Итоги освоения");
            if (titleRow == -1) return;

            int currentRow = titleRow + 2;
            YearlySubjectGroup? currentYearGroup = null;

            while (true)
            {
                var row = sheet.Row(currentRow);
                var cell1 = row.Cell(1);

                if (cell1.IsEmpty() && row.Cell(2).IsEmpty()) break; // Конец блока данных

                // Проверяем, является ли строка заголовком ГОДА
                if (cell1.Style.Fill.BackgroundColor == XLColor.LightGray || cell1.Style.Font.FontSize > 14)
                {
                    currentYearGroup = new YearlySubjectGroup { Year = cell1.GetString().Trim() };
                    vm.YearlyBoards.Add(currentYearGroup);
                    currentRow++;
                    continue;
                }

                // Проверяем, является ли строка заголовком ПРЕДМЕТА
                if (row.IsMerged() || cell1.Style.Font.Bold)
                {
                    if (currentYearGroup == null)
                    {
                        // Если год не был найден, создаем "неизвестный"
                        currentYearGroup = new YearlySubjectGroup { Year = "Неизвестный год" };
                        vm.YearlyBoards.Add(currentYearGroup);
                    }

                    var board = new SubjectBoard { SubjectName = cell1.GetString().Trim() };
                    currentYearGroup.SubjectBoards.Add(board);

                    currentRow++; // Пропускаем заголовок предмета
                    currentRow++; // Пропускаем шапку таблицы (I2, II2...)

                    // Читаем 3 строки метрик
                    for (int i = 0; i < 3; i++)
                    {
                        var metricRow = sheet.Row(currentRow);
                        string type = metricRow.Cell(1).GetString().Trim();
                        var metric = board.Metrics.FirstOrDefault(m => m.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
                        if (metric != null)
                        {
                            metric.I2 = metricRow.Cell(2).GetString();
                            metric.II2 = metricRow.Cell(3).GetString();
                            metric.III2 = metricRow.Cell(4).GetString();
                            metric.IV2 = metricRow.Cell(5).GetString();
                        }
                        currentRow++;
                    }
                }
                else
                {
                    // Если это не заголовок, просто двигаемся дальше, чтобы избежать бесконечного цикла
                    currentRow++;
                }
            }
        }
        public void ImportTeacherData(string filePath, TeacherViewModel vm)
        {
            using (var workbook = new XLWorkbook(filePath))
            {
                // Ищем лист. Обычно экспорт главного окна называется "Данные портфолио"
                var worksheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name == "Данные портфолио")
                             ?? workbook.Worksheets.FirstOrDefault();

                if (worksheet == null) throw new Exception("Не найден лист с данными.");

                // Очищаем коллекции ViewModel (это автоматически помечивает старые записи как удаленные в трекере изменений)
                ClearAllTeacherCollections(vm);

                // === 1. Итоговые результаты (AcademicYearResult) ===
                // Порядок колонок см. в ExcelExportService.WriteTable для Section 1
                ImportSection(worksheet, "1. Итоговые результаты успеваемости", vm.AcademicResults,
                    row => new AcademicYearResult
                    {
                        TeacherId = vm.Id, // Используем Id преподавателя
                        AcademicPeriod = GetVal(row, 1),
                        Subject = GetVal(row, 2),
                        AvgSem1 = GetVal(row, 3),
                        AvgSem2 = GetVal(row, 4),
                        // DynamicsSem (5) - вычисляемое, пропускаем или игнорируем при вставке
                        AvgSuccessRate = GetVal(row, 6),
                        AvgSuccessRateSem2 = GetVal(row, 7),
                        // DynamicsAvgSuccessRate (8)
                        AvgQualityRate = GetVal(row, 9),
                        AvgQualityRateSem2 = GetVal(row, 10),
                        // DynamicsAvgQualityRate (11)
                        EntrySouRate = GetVal(row, 12),
                        ExitSouRate = GetVal(row, 13),
                        // DynamicsSouRate (14)
                        Link = GetVal(row, 15)
                    });

                // === 1А. Промежуточная аттестация ===
                ImportSection(worksheet, "1А. Результаты промежуточной аттестации", vm.IntermediateAssessments,
                    row => new IntermediateAssessment
                    {
                        TeacherId = vm.Id,
                        AcademicYear = GetVal(row, 1),
                        Subject = GetVal(row, 2),
                        AvgScore = GetVal(row, 3),
                        Quality = GetVal(row, 4),
                        Sou = GetVal(row, 5),
                        Link = GetVal(row, 6)
                    });

                // === 2. ГИА (ЕГЭ) ===
                ImportSection(worksheet, "2. Результаты государственной итоговой аттестации (ГИА)", vm.GiaResults,
                    row => new GiaResult
                    {
                        TeacherId = vm.Id,
                        Subject = GetVal(row, 1),
                        Group = GetVal(row, 2),
                        TotalParticipants = GetVal(row, 3),
                        Count5 = GetVal(row, 4),
                        Count4 = GetVal(row, 5),
                        Count3 = GetVal(row, 6),
                        Count2 = GetVal(row, 7),
                        AvgScore = GetVal(row, 8),
                        Link = GetVal(row, 9)
                    });

                // === 3. ДЭ ===
                ImportSection(worksheet, "3. Результаты государственной аттестации (ДЭ)", vm.DemoExamResults,
                    row => new DemoExamResult
                    {
                        TeacherId = vm.Id,
                        Subject = GetVal(row, 1),
                        Group = GetVal(row, 2),
                        TotalParticipants = GetVal(row, 3),
                        Count5 = GetVal(row, 4),
                        Count4 = GetVal(row, 5),
                        Count3 = GetVal(row, 6),
                        Count2 = GetVal(row, 7),
                        AvgScore = GetVal(row, 8),
                        Link = GetVal(row, 9)
                    });

                // === 4. НОКО ===
                ImportSection(worksheet, "4. Результаты освоения по итогам независимой оценки", vm.IndependentAssessments,
                    row => new IndependentAssessment
                    {
                        TeacherId = vm.Id,
                        AssessmentName = GetVal(row, 1),
                        AssessmentDate = GetVal(row, 2),
                        ClassSubject = GetVal(row, 3),
                        StudentsTotal = GetVal(row, 4),
                        StudentsParticipated = GetVal(row, 5),
                        StudentsPassed = GetVal(row, 6),
                        Count5 = GetVal(row, 7),
                        Count4 = GetVal(row, 8),
                        Count3 = GetVal(row, 9),
                        Count2 = GetVal(row, 10),
                        Link = GetVal(row, 11)
                    });

                // === 5. Самоопределение ===
                ImportSection(worksheet, "5. Деятельность по раннему самоопределению", vm.SelfDeterminations,
                    row => new SelfDeterminationActivity
                    {
                        TeacherId = vm.Id,
                        Level = GetVal(row, 1),
                        Name = GetVal(row, 2),
                        Role = GetVal(row, 3),
                        Link = GetVal(row, 4)
                    });

                // === 6. Олимпиады ===
                ImportSection(worksheet, "6. Участие обучающихся в олимпиадах", vm.StudentOlympiads,
                    row => new StudentOlympiad
                    {
                        TeacherId = vm.Id,
                        Level = GetVal(row, 1),
                        Name = GetVal(row, 2),
                        Form = GetVal(row, 3),
                        Cadet = GetVal(row, 4),
                        Result = GetVal(row, 5),
                        Link = GetVal(row, 6)
                    });

                // === 7. Жюри ===
                ImportSection(worksheet, "7. Деятельность в качестве члена жюри", vm.JuryActivities,
                    row => new JuryActivity
                    {
                        TeacherId = vm.Id,
                        Level = GetVal(row, 1),
                        Name = GetVal(row, 2),
                        EventDate = GetVal(row, 3),
                        Link = GetVal(row, 4)
                    });

                // === 8. Мастер-классы ===
                ImportSection(worksheet, "8. Проведение мастер-классов", vm.MasterClasses,
                    row => new MasterClass
                    {
                        TeacherId = vm.Id,
                        Level = GetVal(row, 1),
                        Name = GetVal(row, 2),
                        EventDate = GetVal(row, 3),
                        Link = GetVal(row, 4)
                    });

                // === 9. Выступления ===
                ImportSection(worksheet, "9. Наличие выступлений", vm.Speeches,
                    row => new Speech
                    {
                        TeacherId = vm.Id,
                        Level = GetVal(row, 1),
                        Name = GetVal(row, 2),
                        EventDate = GetVal(row, 3),
                        Link = GetVal(row, 4)
                    });

                // === 10. Публикации ===
                ImportSection(worksheet, "10. Наличие публикации", vm.Publications,
                    row => new Publication
                    {
                        TeacherId = vm.Id,
                        Level = GetVal(row, 1),
                        Title = GetVal(row, 2),
                        Date = GetVal(row, 3),
                        Link = GetVal(row, 4)
                    });

                // === 11. Экспериментальные проекты ===
                ImportSection(worksheet, "11. Участие в экспериментальной", vm.ExperimentalProjects,
                    row => new ExperimentalProject
                    {
                        TeacherId = vm.Id,
                        Name = GetVal(row, 1),
                        Date = GetVal(row, 2),
                        Link = GetVal(row, 3)
                    });

                // === 12. Наставничество ===
                ImportSection(worksheet, "12. Наставническая деятельность", vm.Mentorships,
                    row => new Mentorship
                    {
                        TeacherId = vm.Id,
                        Trainee = GetVal(row, 1),
                        OrderNo = GetVal(row, 2),
                        OrderDate = GetVal(row, 3),
                        Link = GetVal(row, 4)
                    });

                // === 13. Программы ===
                ImportSection(worksheet, "13. Разработка программно-методического", vm.ProgramSupports,
                    row => new ProgramMethodSupport
                    {
                        TeacherId = vm.Id,
                        ProgramName = GetVal(row, 1),
                        // Чекбокс парсим как текст "TRUE"/"FALSE" или "Истина"/"Ложь" или наличие текста
                        HasControlMaterials = ParseBool(GetVal(row, 2)),
                        Link = GetVal(row, 3)
                    });

                // === 14. Конкурсы ===
                ImportSection(worksheet, "14. Участие в профессиональных конкурсах", vm.ProfessionalCompetitions,
                    row => new ProfessionalCompetition
                    {
                        TeacherId = vm.Id,
                        Level = GetVal(row, 1),
                        Name = GetVal(row, 2),
                        Achievement = GetVal(row, 3),
                        EventDate = GetVal(row, 4),
                        Link = GetVal(row, 5)
                    });
            }
        }

        private void ClearAllTeacherCollections(TeacherViewModel vm)
        {
            vm.AcademicResults.Clear();
            vm.IntermediateAssessments.Clear();
            vm.GiaResults.Clear();
            vm.DemoExamResults.Clear();
            vm.IndependentAssessments.Clear();
            vm.SelfDeterminations.Clear();
            vm.StudentOlympiads.Clear();
            vm.JuryActivities.Clear();
            vm.MasterClasses.Clear();
            vm.Speeches.Clear();
            vm.Publications.Clear();
            vm.ExperimentalProjects.Clear();
            vm.Mentorships.Clear();
            vm.ProgramSupports.Clear();
            vm.ProfessionalCompetitions.Clear();
        }

        private bool ParseBool(string val)
        {
            if (string.IsNullOrWhiteSpace(val)) return false;
            val = val.ToLower().Trim();
            return val == "true" || val == "истина" || val == "да" || val == "1" || val == "+";
        }
    }
}