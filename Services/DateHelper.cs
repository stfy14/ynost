// File: Services/DateHelper.cs
using System;

namespace Ynost.Services
{
    /// <summary>
    /// Вспомогательный класс для работы с датами, специфичными для учебного процесса.
    /// </summary>
    public static class DateHelper
    {
        /// <summary>
        /// Возвращает текущий учебный год в формате "ГГГГ-ГГГГ".
        /// Учебный год начинается 1 сентября.
        /// </summary>
        /// <returns>Строка, представляющая учебный год.</returns>
        public static string GetCurrentAcademicYear()
        {
            DateTime today = DateTime.Now;

            // Правило: Учебный год начинается 1 сентября.
            // Если текущий месяц ДО сентября (январь-август), 
            // то учебный год начался в ПРОШЛОМ календарном году.
            int startYear = today.Month < 9 ? today.Year - 1 : today.Year;

            int endYear = startYear + 1;

            return $"{startYear}-{endYear}";
        }
    }
}