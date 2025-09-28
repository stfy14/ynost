using CommunityToolkit.Mvvm.ComponentModel;
using Ynost.Models;
using System;

namespace Ynost.Models;

public partial class Speech : ObservableObject, IChangeTrackable
{
    [ObservableProperty]
    private Guid id = Guid.NewGuid();
    [ObservableProperty]
    private Guid teacherId;
    [ObservableProperty]
    private string level = string.Empty;
    [ObservableProperty]
    private string name = string.Empty;
    [ObservableProperty]
    private string eventDate = string.Empty;
    [ObservableProperty]
    private string link = string.Empty;

    [ObservableProperty]
    private int version = 1;

    [ObservableProperty]
    [System.Text.Json.Serialization.JsonIgnore]
    private bool _isConflicting;
}
