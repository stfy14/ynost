using CommunityToolkit.Mvvm.ComponentModel;
using Ynost.Models;
using System;

namespace Ynost.Models
{
    /// <summary>
    /// Модель для хранения результатов промежуточной аттестации.
    /// </summary>
    public partial class IntermediateAssessment : ObservableObject, IChangeTrackable
    {
        [ObservableProperty]
        private Guid _id = Guid.NewGuid();
        [ObservableProperty]
        private Guid _teacherId;
        [ObservableProperty]
        private string _academicYear = string.Empty;
        [ObservableProperty]
        private string _subject = string.Empty;
        [ObservableProperty]
        private string _avgScore = string.Empty;
        [ObservableProperty]
        private string _quality = string.Empty;
        [ObservableProperty]
        private string _sou = string.Empty;
        [ObservableProperty]
        private string _link = string.Empty;

        [ObservableProperty]
        private int _version = 1;

        [ObservableProperty]
        [System.Text.Json.Serialization.JsonIgnore]
        private bool _isConflicting;
    }
}