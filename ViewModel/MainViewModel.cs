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
        [NotifyCanExecuteChangedFor(nameof(DeleteStaffCommand))]
        private TeacherViewModel? selectedTeacher;

        public IAsyncRelayCommand RetryConnectionCommand { get; }
        public IAsyncRelayCommand SaveChangesCommand { get; }
        public IRelayCommand ToggleLoginCommand { get; }
        public IRelayCommand AddStaffCommand { get; }
        public IAsyncRelayCommand DeleteStaffCommand { get; }


        public MainViewModel()
        {
            _db = App.Db;

            RetryConnectionCommand = new AsyncRelayCommand(LoadDataAsync, CanRetryConnection);
            SaveChangesCommand = new AsyncRelayCommand(ExecuteSaveChanges, CanExecuteSaveChanges);
            ToggleLoginCommand = new RelayCommand(ExecuteToggleLogin);
            AddStaffCommand = new RelayCommand(ExecuteAddStaff, () => CanEditData);
            DeleteStaffCommand = new AsyncRelayCommand(ExecuteDeleteStaff, CanExecuteDeleteStaff);

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

            PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(CanEditData))
                {
                    AddStaffCommand.NotifyCanExecuteChanged();
                    DeleteStaffCommand.NotifyCanExecuteChanged();
                }
            };

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

            // Добавляем проверку на null и сортировку по FullName
            if (newTeacherModels != null)
            {
                var sortedModels = newTeacherModels.OrderBy(t => t.FullName);
                foreach (var model in sortedModels)
                {
                    Teachers.Add(new TeacherViewModel(model));
                }
            }

            if (previouslySelectedTeacherId.HasValue)
            {
                SelectedTeacher = Teachers.FirstOrDefault(tvm => tvm.Id == previouslySelectedTeacherId.Value);
            }

            if (SelectedTeacher == null && Teachers.Any())
            {
                SelectedTeacher = Teachers[0];
            }
        }

        private void ExecuteAddStaff()
        {
            var newTeacher = new Teacher { FullName = "Новый преподаватель" };
            var newTeacherVm = new TeacherViewModel(newTeacher);
            Teachers.Add(newTeacherVm);
            SelectedTeacher = newTeacherVm;
            // Важно: новый преподаватель будет сохранен в БД только после нажатия "Сохранить"
        }

        private async Task ExecuteDeleteStaff()
        {
            if (SelectedTeacher == null) return;

            var result = MessageBox.Show($"Вы уверены, что хотите удалить преподавателя '{SelectedTeacher.FullName}' и все связанные с ним данные?",
                                         "Подтверждение удаления",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            // Если преподаватель еще не сохранен в БД (Id пустой), просто удаляем из списка
            if (SelectedTeacher.Id == Guid.Empty)
            {
                Teachers.Remove(SelectedTeacher);
                return;
            }

            bool success = await _db.DeleteTeacherAsync(SelectedTeacher.Id);
            if (success)
            {
                Teachers.Remove(SelectedTeacher);
                MessageBox.Show("Преподаватель успешно удален.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(_db.LastError ?? "Не удалось удалить преподавателя из базы данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // НОВЫЙ МЕТОД: Определяет, активна ли кнопка "Удалить"
        private bool CanExecuteDeleteStaff()
        {
            return SelectedTeacher != null && CanEditData;
        }

        // Убрали Dispose, так как NetworkAvailabilityService удален
    }
}