using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Ynost.Models;
using Ynost.ViewModels;

namespace Ynost.Services
{
    public class GoogleSheetImportService
    {
        private const string SheetUrl = "https://docs.google.com/spreadsheets/d/1yj4BobZpipi1TOQSLFWiGmX6kqXknwDF75O-cFpyupw/export?format=xlsx&gid=1821723602";

        // === НАСТРОЙКИ КООРДИНАТ (по скриншоту) ===
        private const int ROW_DATA_START = 3;      // Данные начинаются с 3-й строки (после шапки)
        private const int COL_TEACHER = 1;         // A - ФИО преподавателя
        private const int COL_SUBJECT = 2;         // B - Предмет
        private const int COL_METRIC = 3;          // C - Вид контроля
        private const int COL_AGGREGATE_VALUE = 51;// AY - "Средний СОУ, %"

        /// <summary>
        /// Скачивает файл Google Sheets в виде массива байтов.
        /// </summary>
        public async Task<byte[]> DownloadSheetAsync()
        {
            try
            {
                using var client = new HttpClient();
                return await client.GetByteArrayAsync(SheetUrl);
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "DOWNLOAD-SHEET");
                return null;
            }
        }

        /// <summary>
        /// Ищет в скачанном файле все уникальные полные имена, содержащие поисковый запрос.
        /// </summary>
        public List<string> FindCandidates(byte[] fileData, string query)
        {
            var candidates = new HashSet<string>();
            if (fileData == null) return new List<string>();

            try
            {
                using var stream = new MemoryStream(fileData);
                using var workbook = new XLWorkbook(stream);
                var sheet = workbook.Worksheets.FirstOrDefault();
                if (sheet == null) return new List<string>();

                var rows = sheet.RowsUsed(r => r.RowNumber() >= ROW_DATA_START);
                string currentTeacher = "";

                foreach (var row in rows)
                {
                    // Логика объединенных ячеек: если ячейка пустая, берем значение предыдущей
                    string cellValue = row.Cell(COL_TEACHER).GetString().Trim();
                    if (!string.IsNullOrWhiteSpace(cellValue))
                    {
                        currentTeacher = cellValue;
                    }

                    if (!string.IsNullOrEmpty(currentTeacher))
                    {
                        // Проверяем вхождение (без учета регистра)
                        if (currentTeacher.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            candidates.Add(currentTeacher);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "FIND-CANDIDATES");
            }
            return candidates.ToList();
        }

        /// <summary>
        /// Парсит данные для конкретного преподавателя, используя значения из итоговой колонки AY.
        /// </summary>
        public bool ParseDataForExactName(byte[] fileData, TeacherViewModel teacherVm, string exactFullName)
        {
            if (fileData == null) return false;

            try
            {
                using var stream = new MemoryStream(fileData);
                using var workbook = new XLWorkbook(stream);
                var sheet = workbook.Worksheets.FirstOrDefault();
                if (sheet == null) return false;

                // Буфер для накопления данных по каждому предмету
                var buffer = new Dictionary<string, AcademicYearResult>();

                string currentTeacher = "";
                string currentSubject = "";

                var rows = sheet.RowsUsed(r => r.RowNumber() >= ROW_DATA_START);
                bool teacherFoundInSheet = false;

                foreach (var row in rows)
                {
                    // Обновляем текущего преподавателя (логика для объединенных ячеек)
                    string rowTeacher = row.Cell(COL_TEACHER).GetString().Trim();
                    if (!string.IsNullOrWhiteSpace(rowTeacher)) currentTeacher = rowTeacher;

                    // Парсим только если текущее имя в Excel ТОЧНО совпадает с выбранным
                    if (string.IsNullOrEmpty(currentTeacher) ||
                        !string.Equals(currentTeacher, exactFullName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    teacherFoundInSheet = true; // Мы нашли преподавателя

                    // Обновляем текущий предмет
                    string rowSubject = row.Cell(COL_SUBJECT).GetString().Trim();
                    if (!string.IsNullOrWhiteSpace(rowSubject)) currentSubject = rowSubject;
                    if (string.IsNullOrEmpty(currentSubject)) continue;

                    // Получаем или создаем объект AcademicYearResult для текущего предмета
                    if (!buffer.ContainsKey(currentSubject))
                    {
                        buffer[currentSubject] = new AcademicYearResult
                        {
                            TeacherId = teacherVm.Id,
                            AcademicPeriod = DateHelper.GetCurrentAcademicYear(),
                            Subject = currentSubject,
                            Group = "" // Группа не используется в этой логике парсинга
                        };
                    }
                    var resultForSubject = buffer[currentSubject];

                    // Читаем метрику (колонка C) и значение (колонка AY)
                    string metricName = row.Cell(COL_METRIC).GetString().Trim().ToLower();
                    string metricValue = row.Cell(COL_AGGREGATE_VALUE).GetString().Trim();

                    // Пропускаем пустые значения и ошибки Excel типа #DIV/0!
                    if (string.IsNullOrWhiteSpace(metricValue) || metricValue.StartsWith("#"))
                    {
                        continue;
                    }

                    // Сопоставляем название метрики со свойством в нашей модели
                    if (metricName.Contains("средний балл"))
                        resultForSubject.AvgSem1 = metricValue;
                    else if (metricName.Contains("успеваемость"))
                        resultForSubject.AvgSuccessRate = metricValue;
                    else if (metricName.Contains("% кач. зн. по предмету"))
                        resultForSubject.AvgQualityRate = metricValue;
                    else if (metricName.Contains("соу (%) по предмету"))
                        resultForSubject.Intermediate = metricValue;
                    else if (metricName.Contains("входной"))
                        resultForSubject.EntrySouRate = metricValue;
                    else if (metricName.Contains("итоговый"))
                        resultForSubject.ExitSouRate = metricValue;
                }

                // После прохода по всем строкам, переносим данные из буфера в ViewModel
                if (buffer.Count > 0)
                {
                    foreach (var finalResult in buffer.Values)
                    {
                        teacherVm.AcademicResults.Add(finalResult);
                    }
                    return true; // Данные успешно распарсены
                }

                return teacherFoundInSheet; // Возвращаем true, если учитель был найден, даже если данных не было
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "PARSE-DATA-COLUMN-AY");
                return false;
            }
        }
    }
}