using CommunityToolkit.Mvvm.ComponentModel;
using Ynost.Models;
using System;

namespace Ynost.Models;

public partial class ProgramMethodSupport : ObservableObject, IChangeTrackable
{
    [ObservableProperty]
    private Guid _id = Guid.NewGuid();
    [ObservableProperty]
    private Guid _teacherId;
    [ObservableProperty]
    private string _programName = string.Empty;
    [ObservableProperty]
    private bool _hasControlMaterials;
    [ObservableProperty]
    private string _link = string.Empty;

    [ObservableProperty]
    private int _version = 1;
    [ObservableProperty]
    [System.Text.Json.Serialization.JsonIgnore]
    private bool _isConflicting;
}
