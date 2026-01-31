using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Globalization;

namespace Ynost.Models
{
    public partial class AcademicYearResult : ObservableObject, IChangeTrackable
    {
        [ObservableProperty]
        private Guid _id = Guid.NewGuid();

        [ObservableProperty]
        private Guid _teacherId;

        [ObservableProperty]
        private string _group = string.Empty;

        [ObservableProperty]
        private string _academicPeriod = string.Empty;

        [ObservableProperty]
        private string _subject = string.Empty;

        private string _avgSem1 = string.Empty;
        public string AvgSem1
        {
            get => _avgSem1;
            set
            {
                if (SetProperty(ref _avgSem1, value))
                {
                    OnPropertyChanged(nameof(DynamicsSem));
                }
            }
        }

        [ObservableProperty]
        private string _resultATest = string.Empty;

        private string _avgSem2 = string.Empty;
        public string AvgSem2
        {
            get => _avgSem2;
            set
            {
                if (SetProperty(ref _avgSem2, value))
                {
                    OnPropertyChanged(nameof(DynamicsSem));
                }
            }
        }

        public string DynamicsSem
        {
            get
            {
                var sem1Value = TryParseValue(AvgSem1);
                var sem2Value = TryParseValue(AvgSem2);

                if (sem1Value.HasValue && sem2Value.HasValue)
                {
                    decimal difference = sem2Value.Value - sem1Value.Value;
                    return difference.ToString("+0.##;-0.##;0", CultureInfo.CurrentCulture);
                }

                return string.Empty;
            }
            private set
            {
                // Setter for Dapper/JSON
            }
        }

        private string _avgSuccessRate = string.Empty;
        public string AvgSuccessRate
        {
            get => _avgSuccessRate;
            set
            {
                if (SetProperty(ref _avgSuccessRate, value))
                {
                    OnPropertyChanged(nameof(DynamicsAvgSuccessRate));
                }
            }
        }

        private string _avgSuccessRateSem2 = string.Empty;
        public string AvgSuccessRateSem2
        {
            get => _avgSuccessRateSem2;
            set
            {
                if (SetProperty(ref _avgSuccessRateSem2, value))
                {
                    OnPropertyChanged(nameof(DynamicsAvgSuccessRate));
                }
            }
        }

        public string DynamicsAvgSuccessRate
        {
            get
            {
                var v1 = TryParseValue(AvgSuccessRate);
                var v2 = TryParseValue(AvgSuccessRateSem2);

                if (v1.HasValue && v2.HasValue)
                {
                    decimal difference = v2.Value - v1.Value;
                    return difference.ToString("+0.##;-0.##;0", CultureInfo.CurrentCulture);
                }
                return string.Empty;
            }
            private set { /* Setter for Dapper/JSON */ }
        }

        private string _avgQualityRate = string.Empty;
        public string AvgQualityRate
        {
            get => _avgQualityRate;
            set
            {
                if (SetProperty(ref _avgQualityRate, value))
                {
                    OnPropertyChanged(nameof(DynamicsAvgQualityRate));
                }
            }
        }

        private string _avgQualityRateSem2 = string.Empty;
        public string AvgQualityRateSem2
        {
            get => _avgQualityRateSem2;
            set
            {
                if (SetProperty(ref _avgQualityRateSem2, value))
                {
                    OnPropertyChanged(nameof(DynamicsAvgQualityRate));
                }
            }
        }

        public string DynamicsAvgQualityRate
        {
            get
            {
                var v1 = TryParseValue(AvgQualityRate);
                var v2 = TryParseValue(AvgQualityRateSem2);

                if (v1.HasValue && v2.HasValue)
                {
                    decimal difference = v2.Value - v1.Value;
                    return difference.ToString("+0.##;-0.##;0", CultureInfo.CurrentCulture);
                }
                return string.Empty;
            }
            private set { /* Setter for Dapper/JSON */ }
        }

        // === ДОБАВИТЬ ЭТО ===
        [ObservableProperty]
        private string _intermediate = string.Empty;
        // ====================

        private string _entrySouRate = string.Empty;
        public string EntrySouRate
        {
            get => _entrySouRate;
            set
            {
                if (SetProperty(ref _entrySouRate, value))
                {
                    OnPropertyChanged(nameof(DynamicsSouRate)); // Уведомляем об изменении динамики
                }
            }
        }

        private string _exitSouRate = string.Empty;
        public string ExitSouRate
        {
            get => _exitSouRate;
            set
            {
                if (SetProperty(ref _exitSouRate, value))
                {
                    OnPropertyChanged(nameof(DynamicsSouRate)); // Уведомляем об изменении динамики
                }
            }
        }

        /// <summary>
        /// Динамика СОУ (Итоговый - Входной).
        /// </summary>
        public string DynamicsSouRate
        {
            get
            {
                var v1 = TryParseValue(EntrySouRate);
                var v2 = TryParseValue(ExitSouRate);

                if (v1.HasValue && v2.HasValue)
                {
                    decimal difference = v2.Value - v1.Value;
                    return difference.ToString("+0.##;-0.##;0", CultureInfo.CurrentCulture);
                }
                return string.Empty;
            }
            private set { /* Setter for Dapper/JSON */ }
        }

        [ObservableProperty]
        private string _link = string.Empty;

        [ObservableProperty]
        private int _version = 1;

        [ObservableProperty]
        [System.Text.Json.Serialization.JsonIgnore]
        private bool _isConflicting;

        private decimal? TryParseValue(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (decimal.TryParse(value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            return null;
        }
    }
}