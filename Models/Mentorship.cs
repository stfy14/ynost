using CommunityToolkit.Mvvm.ComponentModel;
using Ynost.Models;
using System;

namespace Ynost.Models;

public partial class Mentorship : ObservableObject, IChangeTrackable
{
    [ObservableProperty]
    private Guid _id = Guid.NewGuid();
    [ObservableProperty]
    private Guid _teacherId;
    [ObservableProperty]
    private string _trainee = string.Empty;
    [ObservableProperty]
    private string _orderNo = string.Empty;
    [ObservableProperty]
    private string _orderDate = string.Empty;
    [ObservableProperty]
    private string _link = string.Empty;

    [ObservableProperty]
    private int _version = 1;
    [ObservableProperty]
    [System.Text.Json.Serialization.JsonIgnore]
    private bool _isConflicting;
}
