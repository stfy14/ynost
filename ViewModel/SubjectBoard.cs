using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Ynost.ViewModels   // <-- та же область имён, что и у TeacherMonitoringViewModel
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

        public string I2 { get; set; }
        public string II2 { get; set; }
        public string III2 { get; set; }
        public string IV2 { get; set; }
        public string Y { get; set; }
    }
}
