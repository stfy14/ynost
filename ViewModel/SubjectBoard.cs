using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Ynost.ViewModels
{
    public class SubjectBoard : ObservableObject
    {
        public string SubjectName { get; set; } = string.Empty;

        public ObservableCollection<MetricRow> Metrics { get; } =
            new(new[]
            {
                new MetricRow { Type = "кач" },
                new MetricRow { Type = "усп" },
                new MetricRow { Type = "СОУ" }
            });
    }

    public class MetricRow : ObservableObject
    {
        public string Type { get; set; } = default!;

        private string _i2 = string.Empty;
        public string I2
        {
            get => _i2;
            set
            {
                if (SetProperty(ref _i2, value))
                {
                    OnPropertyChanged(nameof(Y)); // Уведомляем, что динамика изменилась
                }
            }
        }

        private string _ii2 = string.Empty;
        public string II2
        {
            get => _ii2;
            set
            {
                if (SetProperty(ref _ii2, value))
                {
                    OnPropertyChanged(nameof(Y)); // Уведомляем, что динамика изменилась
                }
            }
        }

        private string _iii2 = string.Empty;
        public string III2
        {
            get => _iii2;
            set
            {
                if (SetProperty(ref _iii2, value))
                {
                    OnPropertyChanged(nameof(Y)); // Уведомляем, что динамика изменилась
                }
            }
        }

        private string _iv2 = string.Empty;
        public string IV2
        {
            get => _iv2;
            set
            {
                if (SetProperty(ref _iv2, value))
                {
                    OnPropertyChanged(nameof(Y)); // Уведомляем, что динамика изменилась
                }
            }
        }

        /// <summary>
        /// Вычисляемое свойство для отображения динамики.
        /// </summary>
        public string Y
        {
            get
            {
                var v1 = TryParseValue(I2);
                var v2 = TryParseValue(II2);
                var v3 = TryParseValue(III2);
                var v4 = TryParseValue(IV2);

                decimal? difference = null;

                // Ваша формула с учётом приоритетов:
                // 1. Если 4-я четверть заполнена, считаем разницу с 3-й.
                if (v4.HasValue && v3.HasValue)
                {
                    difference = v4.Value - v3.Value;
                }
                // 2. Иначе, если 4-я пуста, а 3-я заполнена, считаем разницу со 2-й.
                else if (!v4.HasValue && v3.HasValue && v2.HasValue)
                {
                    difference = v3.Value - v2.Value;
                }
                // 3. Иначе, если 3-я и 4-я пусты, а 2-я заполнена, считаем разницу с 1-й.
                else if (!v3.HasValue && !v4.HasValue && v2.HasValue && v1.HasValue)
                {
                    difference = v2.Value - v1.Value;
                }

                // Если разница вычислена, форматируем её в строку со знаком "+" для положительных чисел.
                return difference?.ToString("+#.##;-#.##;0", CultureInfo.CurrentCulture) ?? string.Empty;
            }
            set
            {
                // Сеттер не нужен, так как свойство вычисляемое,
                // но он необходим для корректной работы привязки в некоторых сценариях.
            }
        }

        /// <summary>
        /// Вспомогательный метод для парсинга строки в число.
        /// Корректно обрабатывает как точку, так и запятую в качестве разделителя.
        /// </summary>
        private decimal? TryParseValue(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            // Заменяем запятую на точку для универсальности парсинга
            if (decimal.TryParse(value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            return null;
        }
    }
}