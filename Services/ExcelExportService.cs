// Services/ExcelExportService.cs
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ynost.ViewModels;

namespace Ynost.Services
{
    public class ExcelExportService
    {
        #region Экспорт для MainWindow (Преподаватели)

        public void ExportTeacherData(TeacherViewModel teacherVm, string templatePath, string outputPath)
        {
            using (var workbook = new XLWorkbook(templatePath))
            {
                var worksheet = workbook.Worksheets.FirstOrDefault() ?? workbook.Worksheets.Add("Данные портфолио");
                int currentRow = 1;

                worksheet.Cell(currentRow, 1).Value = $"Портфолио: {teacherVm.FullName}";
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 1).Style.Font.FontSize = 16;
                currentRow += 3;

                // --- ПОЛНЫЙ СПИСОК ТАБЛИЦ ---

                WriteTable(worksheet, ref currentRow, "1. Итоговые результаты успеваемости (аттестат)", teacherVm.AcademicResults,
                    headers: new[] { "Учебный год", "Предмет", "Ср. балл 1 сем.", "Ср. балл 2 сем.", "Динамика сем.", "Успеваемость 1 сем.", "Успеваемость 2 сем.", "Динамика успев.", "Качество 1 сем.", "Качество 2 сем.", "Динамика кач.", "СОУ Входной", "СОУ Итоговый", "Динамика СОУ", "Ссылка" },
                    propertyNames: new[] { "AcademicPeriod", "Subject", "AvgSem1", "AvgSem2", "DynamicsSem", "AvgSuccessRate", "AvgSuccessRateSem2", "DynamicsAvgSuccessRate", "AvgQualityRate", "AvgQualityRateSem2", "DynamicsAvgQualityRate", "EntrySouRate", "ExitSouRate", "DynamicsSouRate", "Link" });

                WriteTable(worksheet, ref currentRow, "1А. Результаты промежуточной аттестации", teacherVm.IntermediateAssessments,
                    headers: new[] { "Учебный год", "Предмет", "СР БАЛ", "КАЧЕСТВО", "СОУ", "Ссылка" },
                    propertyNames: new[] { "AcademicYear", "Subject", "AvgScore", "Quality", "Sou", "Link" });

                WriteTable(worksheet, ref currentRow, "2. Результаты государственной итоговой аттестации (ГИА)", teacherVm.GiaResults,
                    headers: new[] { "Предмет", "Группа", "Кол-во участников", "Кол-во «5»", "Кол-во «4»", "Кол-во «3»", "Кол-во «2»", "Средний балл", "Ссылка" },
                    propertyNames: new[] { "Subject", "Group", "TotalParticipants", "Count5", "Count4", "Count3", "Count2", "AvgScore", "Link" });

                WriteTable(worksheet, ref currentRow, "3. Результаты государственной аттестации (ДЭ)", teacherVm.DemoExamResults,
                    headers: new[] { "Наименование компетенции", "Группа", "Кол-во участников", "Кол-во «5»", "Кол-во «4»", "Кол-во «3»", "Кол-во «2»", "Средний балл", "Ссылка" },
                    propertyNames: new[] { "Subject", "Group", "TotalParticipants", "Count5", "Count4", "Count3", "Count2", "AvgScore", "Link" });

                WriteTable(worksheet, ref currentRow, "4. Результаты освоения по итогам независимой оценки качества образования", teacherVm.IndependentAssessments,
                    headers: new[] { "Вид НОКО (организация)", "Дата", "Класс/Предмет", "Кол-во всего", "Кол-во участвовавших", "Кол-во справившихся", "Кол-во 5", "Кол-во 4", "Кол-во 3", "Кол-во 2", "Ссылка" },
                    propertyNames: new[] { "AssessmentName", "AssessmentDate", "ClassSubject", "StudentsTotal", "StudentsParticipated", "StudentsPassed", "Count5", "Count4", "Count3", "Count2", "Link" });

                WriteTable(worksheet, ref currentRow, "5. Деятельность по раннему самоопределению и профессиональной ориентации", teacherVm.SelfDeterminations,
                    headers: new[] { "Уровень", "Наименование мероприятия", "Роль педагога", "Ссылка" },
                    propertyNames: new[] { "Level", "Name", "Role", "Link" });

                WriteTable(worksheet, ref currentRow, "6. Участие обучающихся в олимпиадах, конкурсах, фестивалях, соревнованиях", teacherVm.StudentOlympiads,
                    headers: new[] { "Уровень", "Наименование События", "Форма участия", "ФИО участника/кадета", "Результат", "Ссылка" },
                    propertyNames: new[] { "Level", "Name", "Form", "Cadet", "Result", "Link" });

                WriteTable(worksheet, ref currentRow, "7. Деятельность в качестве члена жюри олимпиад, конкурсов", teacherVm.JuryActivities,
                    headers: new[] { "Уровень", "Наименование мероприятия", "Дата", "Ссылка" },
                    propertyNames: new[] { "Level", "Name", "EventDate", "Link" });

                WriteTable(worksheet, ref currentRow, "8. Проведение мастер-классов, открытых занятий, мероприятий", teacherVm.MasterClasses,
                    headers: new[] { "Уровень", "Наименование", "Дата", "Ссылка" },
                    propertyNames: new[] { "Level", "Name", "EventDate", "Link" });

                WriteTable(worksheet, ref currentRow, "9. Наличие выступлений на научно-практических конференциях", teacherVm.Speeches,
                    headers: new[] { "Уровень", "Наименование", "Дата", "Ссылка" },
                    propertyNames: new[] { "Level", "Name", "EventDate", "Link" });

                WriteTable(worksheet, ref currentRow, "10. Наличие публикации", teacherVm.Publications,
                    headers: new[] { "Уровень", "Наименование", "Дата", "Ссылка" },
                    propertyNames: new[] { "Level", "Title", "Date", "Link" });

                WriteTable(worksheet, ref currentRow, "11. Участие в экспериментальной и инновационной деятельности", teacherVm.ExperimentalProjects,
                    headers: new[] { "Наименование проекта", "Дата", "Ссылка" },
                    propertyNames: new[] { "Name", "Date", "Link" });

                WriteTable(worksheet, ref currentRow, "12. Наставническая деятельность", teacherVm.Mentorships,
                    headers: new[] { "ФИО стажера", "Номер приказа", "Дата приказа", "Ссылка" },
                    propertyNames: new[] { "Trainee", "OrderNo", "OrderDate", "Link" });

                WriteTable(worksheet, ref currentRow, "13. Разработка программно-методического сопровождения", teacherVm.ProgramSupports,
                    headers: new[] { "Наименование рабочей программы", "Контрольно-измерительные материалы", "Ссылка" },
                    propertyNames: new[] { "ProgramName", "HasControlMaterials", "Link" });

                WriteTable(worksheet, ref currentRow, "14. Участие в профессиональных конкурсах", teacherVm.ProfessionalCompetitions,
                    headers: new[] { "Уровень", "Наименование конкурса", "Достижение", "Дата", "Ссылка" },
                    propertyNames: new[] { "Level", "Name", "Achievement", "EventDate", "Link" });

                var usedColumns = worksheet.ColumnsUsed();
                usedColumns.AdjustToContents();
                foreach (var column in usedColumns)
                {
                    if (column.Width > 50) column.Width = 50;
                    if (column.Width < 12) column.Width = 12;
                }

                workbook.SaveAs(outputPath);
            }
        }

        #endregion

        #region Экспорт для TeacherMonitoringWindow (Учителя)

        public void ExportMonitoringData(TeacherMonitoringViewModel monitoringVm, string templatePath, string outputPath)
        {
            if (monitoringVm.SelectedTeach == null) return;

            using (var workbook = new XLWorkbook(templatePath))
            {
                var worksheet = workbook.Worksheets.FirstOrDefault() ?? workbook.Worksheets.Add("Данные мониторинга");
                int currentRow = 1;

                worksheet.Cell(currentRow, 1).Value = $"Мониторинг: {monitoringVm.SelectedTeach.FullName}";
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 1).Style.Font.FontSize = 16;
                currentRow += 3;

                WriteBoardsTable(worksheet, ref currentRow, "1.1 Итоги освоения образовательных программ — по предметам", monitoringVm.Boards);

                WriteTable(worksheet, ref currentRow, "2. Результаты ГИА (ЕГЭ)", monitoringVm.GiaResults,
                    headers: new[] { "Предмет", "Класс", "По списку", "Участвовали", "81–99 баллов, %", "61–80 баллов, %", "0–60 баллов, %", "Ниже мин., %", "Ссылка" },
                    propertyNames: new[] { "Subject", "Class", "TotalListed", "TotalParticipated", "Pct8199", "Pct6180", "Pct060", "PctBelowMin", "DocScan" });

                WriteTable(worksheet, ref currentRow, "3. Результаты ГИА (ОГЭ)", monitoringVm.OgeResults,
                    headers: new[] { "Предмет", "Класс", "По списку", "Участвовали", "«5»", "«4»", "«3»", "«2»", "Ср. балл", "Ссылка" },
                    propertyNames: new[] { "Subject", "Class", "TotalListed", "TotalParticipated", "Cnt5", "Cnt4", "Cnt3", "Cnt2", "AvgScore", "DocScan" });

                WriteTable(worksheet, ref currentRow, "4. Результаты независимой оценки качества", monitoringVm.IndependentAssessments,
                    headers: new[] { "Процедура", "Дата", "Класс/Предмет", "Всего", "Участники, чел.", "Участники, %", "Справились, чел.", "Справились, %", "Ссылка" },
                    propertyNames: new[] { "AssessmentType", "AssessmentDate", "ClassSubject", "StudentsTotal", "StudentsParticipatedCnt", "StudentsParticipatedPct", "StudentsPassedCnt", "StudentsPassedPct", "DocScan" });

                WriteTable(worksheet, ref currentRow, "5. Самоопределение и профориентация", monitoringVm.SelfDeterminations,
                    headers: new[] { "Уровень", "Мероприятие", "Роль", "Ссылка" },
                    propertyNames: new[] { "Level", "Name", "Role", "DocScan" });

                WriteTable(worksheet, ref currentRow, "6. Участие в олимпиадах и конкурсах", monitoringVm.StudentOlympiads,
                    headers: new[] { "Уровень", "Событие", "Форма", "Кадет", "Результат", "Ссылка" },
                    propertyNames: new[] { "Level", "Name", "Form", "CadetName", "Result", "DocScan" });

                WriteTable(worksheet, ref currentRow, "7. Деятельность в качестве члена жюри", monitoringVm.JuryActivities,
                    headers: new[] { "Уровень", "Событие", "Дата", "Ссылка" },
                    propertyNames: new[] { "Level", "Name", "EventDate", "DocScan" });

                WriteTable(worksheet, ref currentRow, "8. Проведение мастер-классов и мероприятий", monitoringVm.MasterClasses,
                    headers: new[] { "Уровень", "Название", "Дата", "Ссылка" },
                    propertyNames: new[] { "Level", "Name", "EventDate", "DocScan" });

                WriteTable(worksheet, ref currentRow, "9. Выступления на конференциях и семинарах", monitoringVm.Speeches,
                    headers: new[] { "Уровень", "Название", "Дата", "Ссылка" },
                    propertyNames: new[] { "Level", "Name", "EventDate", "DocScan" });

                WriteTable(worksheet, ref currentRow, "10. Публикации", monitoringVm.Publications,
                    headers: new[] { "Уровень", "Название", "Дата", "Ссылка" },
                    propertyNames: new[] { "Level", "Title", "PubDate", "DocScan" });

                WriteTable(worksheet, ref currentRow, "11. Экспериментальная и инновационная деятельность", monitoringVm.ExperimentalProjects,
                    headers: new[] { "Проект", "Дата", "Ссылка" },
                    propertyNames: new[] { "ProjectName", "EventDate", "DocScan" });

                WriteTable(worksheet, ref currentRow, "12. Наставничество", monitoringVm.Mentorships,
                    headers: new[] { "Стажёр", "Приказ №", "Дата", "Ссылка" },
                    propertyNames: new[] { "TraineeName", "OrderNo", "OrderDate", "DocScan" });

                WriteTable(worksheet, ref currentRow, "13. Методическое сопровождение", monitoringVm.ProgramSupports,
                    headers: new[] { "Программа", "Есть материалы", "Ссылка" },
                    propertyNames: new[] { "ProgramName", "HasMaterials", "DocScan" });

                WriteTable(worksheet, ref currentRow, "14. Участие в профессиональных конкурсах", monitoringVm.ProfessionalCompetitions,
                    headers: new[] { "Уровень", "Конкурс", "Достижение", "Дата", "Ссылка" },
                    propertyNames: new[] { "Level", "Name", "Achievement", "EventDate", "DocScan" });

                var usedColumns = worksheet.ColumnsUsed();
                usedColumns.AdjustToContents();
                foreach (var column in usedColumns)
                {
                    if (column.Width > 50) column.Width = 50;
                    if (column.Width < 12) column.Width = 12;
                }

                workbook.SaveAs(outputPath);
            }
        }

        #endregion

        #region Вспомогательные методы

        private void WriteTable<T>(IXLWorksheet worksheet, ref int currentRow, string title, IEnumerable<T> data, string[] headers, string[] propertyNames) where T : class
        {
            var titleCell = worksheet.Cell(currentRow, 1);
            titleCell.Value = title;
            titleCell.Style.Font.Bold = true;
            titleCell.Style.Font.FontSize = 14;
            titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            currentRow++;

            int headerRowNumber = currentRow;
            var headerRow = worksheet.Row(headerRowNumber);
            for (int i = 0; i < headers.Length; i++)
            {
                headerRow.Cell(i + 1).Value = headers[i];
            }

            // **ИСПРАВЛЕНИЕ: Создаем точный диапазон для шапки**
            var headerRange = worksheet.Range(headerRowNumber, 1, headerRowNumber, headers.Length);
            // **ИСПРАВЛЕНИЕ: Применяем стили только к этому диапазону**
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#DDEBF7");
            headerRange.Style.Alignment.WrapText = true;

            currentRow++;

            int dataStartRow = currentRow;
            if (data != null && data.Any())
            {
                foreach (var item in data)
                {
                    for (int i = 0; i < propertyNames.Length; i++)
                    {
                        var propertyName = propertyNames[i];
                        var propInfo = typeof(T).GetProperty(propertyName);
                        if (propInfo != null)
                        {
                            var value = propInfo.GetValue(item);
                            worksheet.Cell(currentRow, i + 1).Value = XLCellValue.FromObject(value);
                        }
                    }
                    currentRow++;
                }
            }
            else
            {
                currentRow++;
            }

            var dataRange = worksheet.Range(dataStartRow, 1, currentRow - 1, headers.Length);
            dataRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
            dataRange.Style.Alignment.WrapText = true;
            dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            var fullTableRange = worksheet.Range(headerRowNumber, 1, currentRow - 1, headers.Length);
            fullTableRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
            fullTableRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

            currentRow += 2;
        }

        private void WriteBoardsTable(IXLWorksheet worksheet, ref int currentRow, string title, ObservableCollection<SubjectBoard> boards)
        {
            // ... (здесь была аналогичная проблема, исправляем) ...
            if (boards == null || !boards.Any())
            {
                var titleCell = worksheet.Cell(currentRow, 1);
                titleCell.Value = title;
                titleCell.Style.Font.Bold = true;
                titleCell.Style.Font.FontSize = 14;
                currentRow += 2;
                return;
            }

            worksheet.Cell(currentRow, 1).Value = title;
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Font.FontSize = 14;
            currentRow += 2;

            string[] headers = { "", "I₂", "II₂", "III₂", "IV₂", "Динамика" };

            foreach (var board in boards)
            {
                int boardStartRow = currentRow;

                var subjectCell = worksheet.Cell(currentRow, 1);
                subjectCell.Value = board.SubjectName;
                subjectCell.Style.Font.Bold = true;
                subjectCell.Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Range(currentRow, 1, currentRow, headers.Length).Merge();
                currentRow++;

                int headerRowNumber = currentRow;
                var headerCell = worksheet.Cell(headerRowNumber, 1);
                headerCell.InsertData(headers, true);

                // **ИСПРАВЛЕНИЕ: Применяем стиль к диапазону шапки**
                var headerRange = worksheet.Range(headerRowNumber, 1, headerRowNumber, headers.Length);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#DDEBF7");
                currentRow++;

                int boardDataStartRow = currentRow;

                foreach (var metric in board.Metrics)
                {
                    worksheet.Cell(currentRow, 1).Value = metric.Type;
                    worksheet.Cell(currentRow, 2).Value = metric.I2;
                    worksheet.Cell(currentRow, 3).Value = metric.II2;
                    worksheet.Cell(currentRow, 4).Value = metric.III2;
                    worksheet.Cell(currentRow, 5).Value = metric.IV2;
                    worksheet.Cell(currentRow, 6).Value = metric.Y;
                    currentRow++;
                }

                var boardDataRange = worksheet.Range(boardDataStartRow, 1, currentRow - 1, headers.Length);
                boardDataRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");

                // Используем boardStartRow, чтобы включить и серую шапку предмета в рамку
                var fullBoardRange = worksheet.Range(boardStartRow, 1, currentRow - 1, headers.Length);
                fullBoardRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
                fullBoardRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

                currentRow++;
            }

            currentRow++;
        }

        #endregion
    }
}