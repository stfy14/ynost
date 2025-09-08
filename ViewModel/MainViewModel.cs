using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Ynost.Models;
using Ynost.Services;
using Ynost.ViewModels; // Убедись, что этот using есть для LoginResultRole
using Ynost.View;    // Для LoginWindow
using Ynost.Properties; // Для Settings

namespace Ynost.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly DatabaseService _db;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowDataContent))] // Зависит от IsLoading
        [NotifyPropertyChangedFor(nameof(ShowLoginPrompt))] // Зависит от IsLoading
        [NotifyPropertyChangedFor(nameof(IsLoadingOrSaving))]
        private bool _isLoading;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowDataContent))] // Зависит от IsSavingData
        [NotifyPropertyChangedFor(nameof(ShowLoginPrompt))] // Зависит от IsSavingData
        [NotifyPropertyChangedFor(nameof(IsLoadingOrSaving))]
        [NotifyCanExecuteChangedFor(nameof(SaveChangesCommand))]
        private bool _isSavingData;

        [ObservableProperty]
        private string _statusText = "Инициализация...";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RetryConnectionCommand))]
        [NotifyCanExecuteChangedFor(nameof(SaveChangesCommand))]
        [NotifyPropertyChangedFor(nameof(CanEditData))] // CanEditData зависит от IsDatabaseConnected
        private bool _isDatabaseConnected;

        [ObservableProperty]
        private bool _isUsingCache;

        [ObservableProperty]
        private string _connectionStatusText = "Определение статуса...";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveChangesCommand))]
        private bool _canEditData;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LoginButtonText))]
        [NotifyPropertyChangedFor(nameof(UserStatusText))]
        [NotifyPropertyChangedFor(nameof(ShowLoginPrompt))]
        [NotifyPropertyChangedFor(nameof(ShowDataContent))]
        [NotifyCanExecuteChangedFor(nameof(ToggleLoginCommand))]
        [NotifyCanExecuteChangedFor(nameof(RetryConnectionCommand))]
        [NotifyPropertyChangedFor(nameof(CanEditData))] // CanEditData зависит от IsLoggedIn
        private bool _isLoggedIn;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(UserStatusText))]
        [NotifyPropertyChangedFor(nameof(CanEditData))] // CanEditData зависит от CurrentUserRole
        private LoginResultRole _currentUserRole = LoginResultRole.None;

        public string LoginButtonText => IsLoggedIn ? "Выйти" : "Войти";
        public string UserStatusText => IsLoggedIn ? $" (Роль: {CurrentUserRole})" : string.Empty;

        public bool IsLoadingOrSaving => IsLoading || IsSavingData;
        public bool ShowLoginPrompt => !IsLoggedIn && !IsLoadingOrSaving;
        public bool ShowDataContent => IsLoggedIn && !IsLoadingOrSaving;

        public ObservableCollection<TeacherViewModel> Teachers { get; } = new();
        [ObservableProperty]
        private TeacherViewModel? selectedTeacher;

        public IAsyncRelayCommand RetryConnectionCommand { get; }
        public IAsyncRelayCommand SaveChangesCommand { get; }
        public IRelayCommand ToggleLoginCommand { get; }

        public MainViewModel()
        {
            _db = App.Db;

            RetryConnectionCommand = new AsyncRelayCommand(LoadDataAsync, CanRetryConnection);
            SaveChangesCommand = new AsyncRelayCommand(ExecuteSaveChanges, CanExecuteSaveChanges);
            ToggleLoginCommand = new RelayCommand(ExecuteToggleLogin);

            // Начальная установка статусов
            if (Settings.Default.RememberLastUser && !string.IsNullOrEmpty(Settings.Default.LastUsername))
            {
                string savedUser = Settings.Default.LastUsername;
                string savedPass = Settings.Default.LastPassword;

                if (savedUser == "admin" && savedPass == "admin")
                {
                    IsLoggedIn = true; CurrentUserRole = LoginResultRole.Editor;
                    StatusText = $"Автоматический вход как {savedUser}. Загрузка данных...";
                    ConnectionStatusText = StatusText;
                    _ = LoadDataAsync();
                }
                else if (savedUser == "view" && savedPass == "view")
                {
                    IsLoggedIn = true; CurrentUserRole = LoginResultRole.Viewer;
                    StatusText = $"Автоматический вход как {savedUser}. Загрузка данных...";
                    ConnectionStatusText = StatusText;
                    _ = LoadDataAsync();
                }
                else
                {
                    Settings.Default.RememberLastUser = false; Settings.Default.LastUsername = string.Empty; Settings.Default.LastPassword = string.Empty; Settings.Default.Save();
                    StatusText = "Войдите в систему для начала работы.";
                    ConnectionStatusText = StatusText;
                }
            }
            else
            {
                StatusText = "Войдите в систему для начала работы.";
                ConnectionStatusText = StatusText;
            }
            UpdateCanEditDataProperty();
        }

        private void ExecuteToggleLogin()
        {
            if (IsLoggedIn)
            {
                var result = MessageBox.Show("Вы точно хотите выйти из аккаунта?", "Подтверждение выхода", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    IsLoggedIn = false; CurrentUserRole = LoginResultRole.None;
                    IsDatabaseConnected = false; // Сбрасываем при выходе
                    Teachers.Clear();
                    StatusText = "Вы вышли из системы. Войдите для доступа к данным.";
                    ConnectionStatusText = StatusText;
                    UpdateCanEditDataProperty();
                }
            }
            else
            {
                var loginWindow = new LoginWindow { Owner = Application.Current.MainWindow };
                var loginVmContext = loginWindow.DataContext as LoginViewModel;
                bool? dialogResult = loginWindow.ShowDialog();
                if (dialogResult == true && loginVmContext != null && loginVmContext.LoginSuccessful)
                {
                    IsLoggedIn = true; CurrentUserRole = loginVmContext.AuthenticatedUserRole;
                    IsDatabaseConnected = false; // Сбрасываем перед загрузкой
                    IsUsingCache = false;
                    _ = LoadDataAsync();
                }
            }
        }

        private void UpdateCanEditDataProperty()
        {
            CanEditData = IsDatabaseConnected && IsLoggedIn && CurrentUserRole == LoginResultRole.Editor;
        }

        private bool CanRetryConnection()
        {
            return IsLoggedIn && !IsDatabaseConnected && !IsLoading && !IsSavingData && !RetryConnectionCommand.IsRunning;
        }

        private async Task ExecuteSaveChanges()
        {
            /* 1. Проверяем соединение */
            if (!IsDatabaseConnected)
            {
                MessageBox.Show("Нет соединения с базой данных.",
                                "Сохранение",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            IsSavingData = true;
            StatusText = "Идёт сохранение данных…";
            Logger.Write("=== ExecuteSaveChanges() entered ===");

            bool ok = false;
            try
            {
                /* 2. Синхронизируем VM → Model */
                foreach (var tvm in Teachers)
                    tvm.SyncToModel();

                Logger.Write("[UI] Calling SaveAllAsync() …");

                /* 3. Сохраняем и получаем результат */
                ok = await _db.SaveAllAsync(Teachers.Select(t => t.Model));

                if (ok)
                {
                    Logger.Write("[UI] SaveAllAsync → OK");
                    MessageBox.Show("Изменения сохранены.",
                                    "Сохранение",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    await LoadDataAsync();           // перезагружаем данные
                }
                else
                {
                    /* 4. Берём подробную ошибку из DatabaseService.LastError */
                    string err = _db.LastError ?? "Не удалось сохранить данные.";
                    Logger.Write($"[UI] SaveAllAsync → FAIL: {err}");

                    MessageBox.Show(err,
                                    "Ошибка сохранения",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                /* 5. Любая непойманная выше ошибка */
                Logger.Write(ex, "ExecuteSaveChanges Exception");

                MessageBox.Show($"Произошла непредвиденная ошибка:\n{ex}",
                                "Ошибка сохранения",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            finally
            {
                IsSavingData = false;
                StatusText = ok
                    ? "Данные успешно сохранены."
                    : (IsDatabaseConnected ? "Ошибка при сохранении." : ConnectionStatusText);

                Logger.Write("=== ExecuteSaveChanges() exit ===");
            }
        }

        private bool CanExecuteSaveChanges()
        {
            return CanEditData && !IsSavingData;
        }

        public async Task LoadDataAsync()
        {
            if (!IsLoggedIn)
            {
                Teachers.Clear(); IsDatabaseConnected = false; UpdateCanEditDataProperty();
                StatusText = "Войдите в систему для загрузки данных."; ConnectionStatusText = StatusText;
                IsLoading = false; OnPropertyChanged(nameof(ShowLoginPrompt)); OnPropertyChanged(nameof(ShowDataContent));
                return;
            }

            if (IsLoading || IsSavingData || RetryConnectionCommand.IsRunning) return;

            IsLoading = true;
            StatusText = "Загрузка данных...";
            IsUsingCache = false;

            List<Teacher>? teacherModels = await _db.LoadAllAsync();

            if (teacherModels != null)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    UpdateTeachersCollection(teacherModels);
                    IsDatabaseConnected = true; IsUsingCache = false;
                    ConnectionStatusText = $"Данные успешно загружены из БД. Записей: {Teachers.Count}.";
                    StatusText = ConnectionStatusText;
                });
                await _db.SaveToCacheAsync(teacherModels);
            }
            else
            {
                await Application.Current.Dispatcher.InvokeAsync(async () => // Сделали лямбду асинхронной
                {
                    IsDatabaseConnected = false;
                    List<Teacher>? cachedModels = await _db.LoadFromCacheAsync(); // await здесь
                    if (cachedModels != null && cachedModels.Count > 0)
                    {
                        UpdateTeachersCollection(cachedModels); IsUsingCache = true;
                        ConnectionStatusText = "Ошибка соединения с БД. Отображаются данные из кеша.";
                    }
                    else
                    {
                        Teachers.Clear(); IsUsingCache = false;
                        ConnectionStatusText = "Ошибка соединения с БД. Кеш пуст или недоступен.";
                    }
                    StatusText = ConnectionStatusText;
                });
            }

            IsLoading = false;
            UpdateCanEditDataProperty();
            // Обновляем CanExecute команд явно, так как IsLoading изменился
            ((AsyncRelayCommand)RetryConnectionCommand).NotifyCanExecuteChanged();
            ((AsyncRelayCommand)SaveChangesCommand).NotifyCanExecuteChanged(); // SaveChangesCommand тоже может зависеть от IsLoading косвенно через CanEditData
            OnPropertyChanged(nameof(ShowLoginPrompt));
            OnPropertyChanged(nameof(ShowDataContent));
        }

        private void UpdateTeachersCollection(List<Teacher> newTeacherModels)
        {
            Guid? previouslySelectedTeacherId = SelectedTeacher?.Id;
            Teachers.Clear();
            if (newTeacherModels != null) { foreach (var model in newTeacherModels) Teachers.Add(new TeacherViewModel(model)); }
            if (previouslySelectedTeacherId.HasValue) { SelectedTeacher = Teachers.FirstOrDefault(tvm => tvm.Id == previouslySelectedTeacherId.Value); }
            if (SelectedTeacher == null && Teachers.Count > 0) { SelectedTeacher = Teachers[0]; }
        }

        // Убрали Dispose, так как NetworkAvailabilityService удален
    }
}