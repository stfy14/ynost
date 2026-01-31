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

        // === НАСТРОЙКИ КООРДИНАТ ===
        private const int ROW_HEADER_GROUPS = 2;
        private const int COL_TEACHER = 1; // A - ФИО
        private const int COL_SUBJECT = 2; // B - Предмет
        private const int COL_METRIC = 3; // C - Вид контроля
        private const int COL_DATA_START = 4; // D
        private const string CURRENT_YEAR = "2024-2025";

        /// <summary>
        /// 1. Скачивает файл в память (чтобы не качать дважды).
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
        /// 2. Ищет все уникальные полные имена, содержащие запрос query.
        /// </summary>
        public List<string> FindCandidates(byte[] fileData, string query)
        {
            var candidates = new HashSet<string>();
            try
            {
                using var stream = new MemoryStream(fileData);
                using var workbook = new XLWorkbook(stream);
                var sheet = workbook.Worksheets.FirstOrDefault();
                if (sheet == null) return new List<string>();

                var rows = sheet.RowsUsed(r => r.RowNumber() > ROW_HEADER_GROUPS);
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
        /// 3. Парсит данные строго для конкретного полного имени.
        /// </summary>
        public bool ParseDataForExactName(byte[] fileData, TeacherViewModel teacherVm, string exactFullName)
        {
            try
            {
                using var stream = new MemoryStream(fileData);
                using var workbook = new XLWorkbook(stream);
                var sheet = workbook.Worksheets.FirstOrDefault();
                if (sheet == null) return false;

                // 1. Читаем шапку групп
                var groupColumns = new Dictionary<int, string>();
                var headerRow = sheet.Row(ROW_HEADER_GROUPS);
                for (int c = COL_DATA_START; c <= 200; c++)
                {
                    string grName = headerRow.Cell(c).GetString().Trim();
                    if (string.IsNullOrWhiteSpace(grName))
                    {
                        if (string.IsNullOrWhiteSpace(headerRow.Cell(c + 1).GetString())) break;
                        continue;
                    }
                    groupColumns[c] = grName;
                }

                // 2. Ищем данные
                var buffer = new Dictionary<string, AcademicYearResult>();
                string currentTeacher = "";
                string currentSubject = "";

                var rows = sheet.RowsUsed(r => r.RowNumber() > ROW_HEADER_GROUPS);
                bool dataFound = false;

                foreach (var row in rows)
                {
                    string rowTeacher = row.Cell(COL_TEACHER).GetString().Trim();
                    if (!string.IsNullOrWhiteSpace(rowTeacher)) currentTeacher = rowTeacher;

                    // === СТРОГОЕ СРАВНЕНИЕ ===
                    // Мы парсим только если текущее имя в Excel ТОЧНО совпадает с выбранным
                    if (string.IsNullOrEmpty(currentTeacher) ||
                        !string.Equals(currentTeacher, exactFullName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    dataFound = true; // Мы нашли преподавателя, даже если оценок нет

                    string rowSubject = row.Cell(COL_SUBJECT).GetString().Trim();
                    if (!string.IsNullOrWhiteSpace(rowSubject)) currentSubject = rowSubject;
                    if (string.IsNullOrEmpty(currentSubject)) continue;

                    string metricName = row.Cell(COL_METRIC).GetString().Trim().ToLower();

                    foreach (var kvp in groupColumns)
                    {
                        string cellValue = row.Cell(kvp.Key).GetString().Trim();
                        if (string.IsNullOrWhiteSpace(cellValue)) continue;

                        string key = $"{currentSubject}_{kvp.Value}"; // Предмет_Группа
                        if (!buffer.ContainsKey(key))
                        {
                            buffer[key] = new AcademicYearResult
                            {
                                TeacherId = teacherVm.Id,
                                AcademicPeriod = CURRENT_YEAR,
                                Subject = currentSubject,
                                Group = kvp.Value
                            };
                        }

                        var res = buffer[key];
                        // Заполнение полей
                        if (metricName.Contains("средний балл")) res.AvgSem1 = cellValue;
                        else if (metricName.Contains("успеваемость")) res.AvgSuccessRate = cellValue;
                        else if (metricName.Contains("кач. зн") || metricName.Contains("качество")) res.AvgQualityRate = cellValue;
                        else if (metricName.Contains("входной")) res.EntrySouRate = cellValue;
                        else if (metricName.Contains("итоговый")) res.ExitSouRate = cellValue;
                        else if (metricName.Contains("соу (%)")) res.Intermediate = cellValue;
                    }
                }

                if (buffer.Count > 0)
                {
                    foreach (var item in buffer.Values)
                        teacherVm.AcademicResults.Add(item);
                    return true;
                }

                return dataFound; // Вернем true, если учитель был найден в таблице (даже если оценок нет)
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "PARSE-DATA");
                return false;
            }
        }
    }
}