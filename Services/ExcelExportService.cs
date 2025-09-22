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
                worksheet.Range(currentRow, 1, currentRow, 5).Merge();
                currentRow += 3;

                WriteSimpleTable(worksheet, ref currentRow, "1. Итоговые результаты успеваемости (аттестат)", teacherVm.AcademicResults,
                    new[] { "Учебный год", "Предмет", "Ср. балл 1 сем.", "Ср. балл 2 сем.", "Динамика сем.", "Успеваемость 1 сем.", "Успеваемость 2 сем.", "Динамика успев.", "Качество 1 сем.", "Качество 2 сем.", "Динамика кач.", "СОУ Входной", "СОУ Итоговый", "Динамика СОУ", "Ссылка" });

                WriteSimpleTable(worksheet, ref currentRow, "1А. Результаты промежуточной аттестации", teacherVm.IntermediateAssessments,
                    new[] { "Учебный год", "Предмет", "СР БАЛ", "КАЧЕСТВО", "СОУ", "Ссылка" });

                WriteSimpleTable(worksheet, ref currentRow, "2. Результаты государственной итоговой аттестации (ГИА)", teacherVm.GiaResults,
                    new[] { "Предмет", "Группа", "Кол-во участников", "Кол-во «5»", "Кол-во «4»", "Кол-во «3»", "Кол-во «2»", "Средний балл", "Ссылка" });

                WriteSimpleTable(worksheet, ref currentRow, "3. Результаты государственной аттестации (ДЭ)", teacherVm.DemoExamResults,
                    new[] { "Наименование компетенции", "Группа", "Кол-во участников", "Кол-во «5»", "Кол-во «4»", "Кол-во «3»", "Кол-во «2»", "Средний балл", "Ссылка" });

                WriteSimpleTable(worksheet, ref currentRow, "4. Результаты освоения по итогам независимой оценки качества образования", teacherVm.IndependentAssessments,
                    new[] { "Вид НОКО (организация)", "Дата", "Класс/Предмет", "Кол-во всего", "Кол-во участвовавших", "Кол-во справившихся", "Кол-во 5", "Кол-во 4", "Кол-во 3", "Кол-во 2", "Ссылка" });

                WriteSimpleTable(worksheet, ref currentRow, "5. Деятельность по раннему самоопределению и профессиональной ориентации", teacherVm.SelfDeterminations,
                    new[] { "Уровень", "Наименование мероприятия", "Роль педагога", "Ссылка" });

                WriteSimpleTable(worksheet, ref currentRow, "6. Участие обучающихся в олимпиадах, конкурсах, фестивалях, соревнованиях", teacherVm.StudentOlympiads,
                    new[] { "Уровень", "Наименование События", "Форма участия", "ФИО участника/кадета", "Результат", "Ссылка" });

                WriteSimpleTable(worksheet, ref currentRow, "7. Деятельность в качестве члена жюри олимпиад, конкурсов", teacherVm.JuryActivities,
                    new[] { "Уровень", "Наименование мероприятия", "Дата", "Ссылка" });

                WriteSimpleTable(worksheet, ref currentRow, "8. Проведение мастер-классов, открытых занятий, мероприятий", teacherVm.MasterClasses,
                    new[] { "Уровень", "Наименование", "Дата", "Ссылка" });

                WriteSimpleTable(worksheet, ref currentRow, "9. Наличие выступлений на научно-практических конференциях", teacherVm.Speeches,
                    new[] { "Уровень", "Наименование", "Дата", "Ссылка" });

                WriteSimpleTable(worksheet, ref currentRow, "10. Наличие публикации", teacherVm.Publications,
                    new[] { "Уровень", "Наименование", "Дата", "Ссылка" });

                WriteSimpleTable(worksheet, ref currentRow, "11. Участие в экспериментальной и инновационной деятельности", teacherVm.ExperimentalProjects,
                    new[] { "Наименование проекта", "Дата", "Ссылка" });

                WriteSimpleTable(worksheet, ref currentRow, "12. Наставническая деятельность", teacherVm.Mentorships,
                    new[] { "ФИО стажера", "Номер приказа", "Дата приказа", "Ссылка" });

                WriteSimpleTable(worksheet, ref currentRow, "13. Разработка программно-методического сопровождения", teacherVm.ProgramSupports,
                    new[] { "Наименование рабочей программы", "Контрольно-измерительные материалы", "Ссылка" });

                WriteSimpleTable(worksheet, ref currentRow, "14. Участие в профессиональных конкурсах", teacherVm.ProfessionalCompetitions,
                    new[] { "Уровень", "Наименование конкурса", "Достижение", "Дата", "Ссылка" });

                worksheet.Columns().AdjustToContents();
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
                worksheet.Range(currentRow, 1, currentRow, 5).Merge();
                currentRow += 3;

                // --- Заполняем все таблицы мониторинга, включая вложенную 1.1 ---

                WriteBoardsTable(worksheet, ref currentRow, "1.1 Итоги освоения образовательных программ — по предметам", monitoringVm.Boards);

                WriteSimpleTable(worksheet, ref currentRow, "2. Результаты ГИА (ЕГЭ)", monitoringVm.GiaResults,
                    new[] { "Предмет", "Класс", "По списку", "Участвовали", "81–99 баллов, %", "61–80 баллов, %", "0–60 баллов, %", "Ниже мин., %", "Ссылка" });

                WriteSimpleTable(worksheet, ref currentRow, "3. Результаты ГИА (ОГЭ)", monitoringVm.OgeResults,
                    new[] { "Предмет", "Класс", "По списку", "Участвовали", "«5»", "«4»", "«3»", "«2»", "Ср. балл", "Ссылка" });

                WriteSimpleTable(worksheet, ref currentRow, "4. Результаты независимой оценки качества", monitoringVm.IndependentAssessments,
                    new[] { "Процедура", "Дата", "Класс/Предмет", "Всего", "Участники, чел.", "Участники, %", "Справились, чел.", "Справились, %", "Ссылка" });

                WriteSimpleTable(worksheet, ref currentRow, "5. Самоопределение и профориентация", monitoringVm.SelfDeterminations,
                    new[] { "Уровень", "Мероприятие", "Роль", "Ссылка" });

                WriteSimpleTable(worksheet, ref currentRow, "6. Участие в олимпиадах и конкурсах", monitoringVm.StudentOlympiads,
                    new[] { "Уровень", "Событие", "Форма", "Кадет", "Результат", "Ссылка" });

                WriteSimpleTable(worksheet, ref currentRow, "7. Деятельность в качестве члена жюри", monitoringVm.JuryActivities,
                    new[] { "Уровень", "Событие", "Дата", "Ссылка" });

                WriteSimpleTable(worksheet, ref currentRow, "8. Проведение мастер-классов и мероприятий", monitoringVm.MasterClasses,
                    new[] { "Уровень", "Название", "Дата", "Ссылка" });

                WriteSimpleTable(worksheet, ref currentRow, "9. Выступления на конференциях и семинарах", monitoringVm.Speeches,
                    new[] { "Уровень", "Название", "Дата", "Ссылка" });

                WriteSimpleTable(worksheet, ref currentRow, "10. Публикации", monitoringVm.Publications,
                    new[] { "Уровень", "Название", "Дата", "Ссылка" });

                WriteSimpleTable(worksheet, ref currentRow, "11. Экспериментальная и инновационная деятельность", monitoringVm.ExperimentalProjects,
                    new[] { "Проект", "Дата", "Ссылка" });

                WriteSimpleTable(worksheet, ref currentRow, "12. Наставничество", monitoringVm.Mentorships,
                    new[] { "Стажёр", "Приказ №", "Дата", "Ссылка" });

                WriteSimpleTable(worksheet, ref currentRow, "13. Методическое сопровождение", monitoringVm.ProgramSupports,
                    new[] { "Программа", "Есть материалы", "Ссылка" });

                WriteSimpleTable(worksheet, ref currentRow, "14. Участие в профессиональных конкурсах", monitoringVm.ProfessionalCompetitions,
                    new[] { "Уровень", "Конкурс", "Достижение", "Дата", "Ссылка" });

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(outputPath);
            }
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Записывает на лист Excel простую таблицу (заголовок, шапка, данные).
        /// </summary>
        private void WriteSimpleTable<T>(IXLWorksheet worksheet, ref int currentRow, string title, IEnumerable<T> data, string[] headers) where T : class
        {
            if (data == null || !data.Any()) return;

            worksheet.Cell(currentRow, 1).Value = title;
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Font.FontSize = 14;
            worksheet.Range(currentRow, 1, currentRow, headers.Length).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            currentRow++;

            var headerCell = worksheet.Cell(currentRow, 1);
            headerCell.InsertData(headers, true); // Вставляем шапку как одну строку
            var headerRow = worksheet.Row(currentRow);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#DDEBF7");
            currentRow++;

            if (data.Any())
            {
                worksheet.Cell(currentRow, 1).InsertData(data);
                int dataRowsCount = data.Count();

                var range = worksheet.Range(currentRow - 1, 1, currentRow + dataRowsCount - 1, headers.Length);
                range.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
                range.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

                currentRow += dataRowsCount;
            }

            currentRow += 2;
        }

        /// <summary>
        /// Специальный метод для записи вложенной таблицы 1.1 из мониторинга.
        /// </summary>
        private void WriteBoardsTable(IXLWorksheet worksheet, ref int currentRow, string title, ObservableCollection<SubjectBoard> boards)
        {
            if (boards == null || !boards.Any()) return;

            worksheet.Cell(currentRow, 1).Value = title;
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Font.FontSize = 14;
            worksheet.Range(currentRow, 1, currentRow, 7).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            currentRow += 2;

            string[] headers = { "", "I₂", "II₂", "III₂", "IV₂", "Динамика" };

            foreach (var board in boards)
            {
                // Название предмета
                var subjectCell = worksheet.Cell(currentRow, 1);
                subjectCell.Value = board.SubjectName;
                subjectCell.Style.Font.Bold = true;
                subjectCell.Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Range(currentRow, 1, currentRow, headers.Length).Merge();
                currentRow++;

                // Шапка
                var headerCell = worksheet.Cell(currentRow, 1);
                headerCell.InsertData(headers, true);
                worksheet.Row(currentRow).Style.Font.Bold = true;
                currentRow++;

                // Данные (3 строки: кач, усп, СОУ)
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

                // Рамка для этой под-таблицы
                var range = worksheet.Range(currentRow - 4, 1, currentRow - 1, headers.Length);
                range.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
                range.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

                currentRow++; // Дополнительный отступ между предметами
            }

            currentRow++;
        }

        #endregion
    }
}