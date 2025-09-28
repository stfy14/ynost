using CommunityToolkit.Mvvm.ComponentModel;
using Ynost.Models;
using System;

namespace Ynost.Models;

public partial class DemoExamResult : ObservableObject, IChangeTrackable
{
    [ObservableProperty]
    private Guid _id = Guid.NewGuid();
    [ObservableProperty]
    private Guid _teacherId;
    [ObservableProperty]
    private string _subject = string.Empty;
    [ObservableProperty]
    private string _group = string.Empty;
    [ObservableProperty]
    private string _totalParticipants = string.Empty;
    [ObservableProperty]
    private string _count5 = string.Empty;
    [ObservableProperty]
    private string _count4 = string.Empty;
    [ObservableProperty]
    private string _count3 = string.Empty;
    [ObservableProperty]
    private string _count2 = string.Empty;
    [ObservableProperty]
    private string _avgScore = string.Empty;
    [ObservableProperty]
    private string _link = string.Empty;

    [ObservableProperty]
    private int _version = 1;

    [ObservableProperty]
    [System.Text.Json.Serialization.JsonIgnore]
    private bool _isConflicting;
}
