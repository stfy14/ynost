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
using System.IO;
using Microsoft.Win32;
using System.Data;

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
        [NotifyCanExecuteChangedFor(nameof(ExportToExcelCommand))]
        private TeacherViewModel? selectedTeacher;

        public IAsyncRelayCommand RetryConnectionCommand { get; }
        public IAsyncRelayCommand SaveChangesCommand { get; }
        public IRelayCommand ToggleLoginCommand { get; }
        public IRelayCommand AddStaffCommand { get; }
        public IAsyncRelayCommand DeleteStaffCommand { get; }
        public IAsyncRelayCommand ExportToExcelCommand { get; }


        public MainViewModel()
        {
            _db = App.Db;

            RetryConnectionCommand = new AsyncRelayCommand(LoadDataAsync, CanRetryConnection);
            SaveChangesCommand = new AsyncRelayCommand(ExecuteSaveChanges, CanExecuteSaveChanges);
            ToggleLoginCommand = new RelayCommand(ExecuteToggleLogin);
            AddStaffCommand = new RelayCommand(ExecuteAddStaff, () => CanEditData);
            DeleteStaffCommand = new AsyncRelayCommand(ExecuteDeleteStaff, CanExecuteDeleteStaff);
            ExportToExcelCommand = new AsyncRelayCommand(ExportDataToExcel, CanExportDataToExcel);

            // Начальная установка статусов
            if (Settings.Default.RememberLastUser && !string.IsNullOrEmpty(Settings.Default.LastUsername))
            {
                string savedUser = Settings.Default.LastUsername;
                string savedPass = Settings.Default.LastPassword;

                if (savedUser == "admin" && savedPass == "admin")
                {
                    IsLoggedIn = true; 
                    CurrentUserRole = LoginResultRole.Editor;
                    AuthService.CurrentUserRole = CurrentUserRole;
                    StatusText = $"Автоматический вход как {savedUser}. Загрузка данных...";
                    ConnectionStatusText = StatusText;
                    _ = LoadDataAsync();
                }
                else if (savedUser == "view" && savedPass == "view")
                {
                    IsLoggedIn = true; 
                    CurrentUserRole = LoginResultRole.Viewer;
                    AuthService.CurrentUserRole = CurrentUserRole;
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
                    IsLoggedIn = false; 
                    CurrentUserRole = LoginResultRole.None;
                    AuthService.CurrentUserRole = LoginResultRole.None;
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
                    IsLoggedIn = true; 
                    CurrentUserRole = loginVmContext.AuthenticatedUserRole;
                    AuthService.CurrentUserRole = CurrentUserRole;
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
            return IsLoggedIn && !IsLoading && !IsSavingData && !RetryConnectionCommand.IsRunning;
        }

        private async Task ExecuteSaveChanges()
        {
            if (SelectedTeacher == null) return;
            if (!IsDatabaseConnected)
            {
                MessageBox.Show("Нет соединения с БД.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            IsSavingData = true;
            StatusText = $"Сохранение: {SelectedTeacher.FullName}...";

            try
            {
                // === ЛОГИКА ДЛЯ НОВЫХ ПРЕПОДАВАТЕЛЕЙ ===
                if (SelectedTeacher.IsNew)
                {
                    // 1. Физически создаем запись в таблице teachers
                    // AddTeachAsync делает INSERT и возвращает реальный ID и правильное Имя
                    var realInfo = await _db.AddTeachAsync(SelectedTeacher.FullName);

                    if (realInfo == null) throw new Exception("Не удалось создать запись в БД (AddTeachAsync вернул null).");

                    // 2. Подменяем временный ID на реальный во всей ViewModel
                    // (Метод ReplaceTeacherIdInVm мы писали ранее, он должен быть в классе)
                    ReplaceTeacherIdInVm(SelectedTeacher, realInfo.Id);

                    // 3. Снимаем флаг новизны
                    SelectedTeacher.IsNew = false;

                    // Теперь у нас есть реальный ID в базе, и можно сохранять дочерние таблицы (оценки и т.д.)
                }
                // ========================================

                // Стандартное сохранение (обновляет имя, сохраняет все списки оценок)
                await _db.SaveTeacherChangesAsync(SelectedTeacher);

                SelectedTeacher.ClearAllChanges();
                StatusText = "Успешно сохранено.";
                MessageBox.Show("Данные успешно сохранены в БД.", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (DBConcurrencyException)
            {
                MessageBox.Show("Конфликт версий! Кто-то изменил данные параллельно.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                // Тут логика перезагрузки...
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Критическая ошибка сохранения:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsSavingData = false;
                // Можно перезагрузить список, чтобы убедиться, что всё ок, 
                // но тогда сбросится выделение.
                // await LoadDataAsync(); 
            }
        }

        private bool CanExecuteSaveChanges()
        {
            return SelectedTeacher != null && CanEditData && !IsSavingData;
        }

        public async Task LoadDataAsync()
        {
            // --- НАЧАЛО НОВЫХ ИЗМЕНЕНИЙ ---

            // Шаг 1: Проверяем, нужно ли показывать диалоговое окно.
            // Оно появляется, только если соединение с БД уже есть (т.е. это "Обновление", а не "Повтор").
            if (IsDatabaseConnected && SelectedTeacher != null && SelectedTeacher.HasChanges)
            {
                var result = MessageBox.Show(
                    "Вы уверены, что хотите обновить данные?\n" +
                    "Все несохраненные изменения будут потеряны.",
                    "Подтверждение обновления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            // --- КОНЕЦ НОВЫХ ИЗМЕНЕНИЙ ---

            // Дальнейший код выполняется, только если:
            // 1. Соединения с БД не было (и окно не показывалось).
            // 2. Соединение было, и пользователь нажал "Да".

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
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    IsDatabaseConnected = false;
                    List<Teacher>? cachedModels = await _db.LoadFromCacheAsync();
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
            ((AsyncRelayCommand)RetryConnectionCommand).NotifyCanExecuteChanged();
            ((AsyncRelayCommand)SaveChangesCommand).NotifyCanExecuteChanged();
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

        private async void ExecuteAddStaff()
        {
            // 1. Спрашиваем пользователя про режим
            var result = MessageBox.Show(
                "Использовать импорт из Google Sheets?\n\n" +
                "Да — ввести имя и найти данные.\n" +
                "Нет — создать пустую карточку.",
                "Создание преподавателя",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Cancel) return;

            bool needImport = (result == MessageBoxResult.Yes);
            string finalName = "Новый преподаватель";

            // Создаем "призрака" (VM в памяти)
            var tempId = Guid.NewGuid();
            var vm = new TeacherViewModel(new Teacher { Id = tempId, FullName = finalName, Version = 1 })
            {
                IsNew = true // Флаг для кнопки Сохранить
            };

            if (needImport)
            {
                // Ввод имени для поиска
                var inputWin = new InputNameWindow { Owner = Application.Current.MainWindow };
                if (inputWin.ShowDialog() != true) return;

                string query = inputWin.EnteredName;

                // Включаем индикатор загрузки
                IsLoading = true;

                // === СТАТУС 1: СКАЧИВАНИЕ ===
                StatusText = "Скачивание таблицы...";

                var service = new GoogleSheetImportService();
                byte[] fileData = await service.DownloadSheetAsync();

                if (fileData == null)
                {
                    IsLoading = false;
                    StatusText = "Ошибка загрузки.";
                    MessageBox.Show("Не удалось скачать таблицу Google Sheets.", "Ошибка сети", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // === СТАТУС 2: ПОИСК ИМЕНИ ===
                StatusText = $"Поиск имени '{query}'...";

                // Запускаем поиск в фоновом потоке, чтобы UI не вис
                List<string> candidates = await Task.Run(() => service.FindCandidates(fileData, query));

                // Временно скрываем загрузку, если нужно показать диалог
                IsLoading = false;

                string selectedFullName = null;

                if (candidates.Count == 0)
                {
                    MessageBox.Show($"По запросу '{query}' ничего не найдено.\nБудет создана пустая карточка.",
                        "Не найдено", MessageBoxButton.OK, MessageBoxImage.Information);
                    selectedFullName = query;
                }
                else if (candidates.Count == 1)
                {
                    // Нашли ровно одного
                    selectedFullName = candidates[0];
                }
                else // Нашли несколько
                {
                    var selectWin = new TeacherSelectionWindow(candidates) { Owner = Application.Current.MainWindow };
                    if (selectWin.ShowDialog() == true)
                    {
                        selectedFullName = selectWin.SelectedName;
                    }
                    else
                    {
                        return; // Отмена выбора
                    }
                }

                // Применяем выбранное имя
                finalName = selectedFullName;
                vm.FullName = finalName;

                // Если есть совпадение в таблице — парсим данные
                if (candidates.Contains(selectedFullName))
                {
                    // Снова включаем загрузку
                    IsLoading = true;

                    // === СТАТУС 3: ЗАПОЛНЕНИЕ ДАННЫХ ===
                    StatusText = $"Заполнение данных для \"{selectedFullName}\"...";

                    bool parsed = await Task.Run(() => service.ParseDataForExactName(fileData, vm, selectedFullName));

                    IsLoading = false;

                    if (parsed)
                        StatusText = "Данные загружены (не сохранены).";
                    else
                        StatusText = "Данные не найдены (только имя).";
                }
                else
                {
                    StatusText = "Создан новый преподаватель.";
                }
            }
            else
            {
                StatusText = "Создан новый преподаватель (ручной режим).";
            }

            // Добавляем в UI
            Teachers.Add(vm);
            SelectedTeacher = vm;
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
        private bool CanExportDataToExcel()
        {
            return SelectedTeacher != null;
        }

        private async Task ExportDataToExcel()
        {
            if (SelectedTeacher == null) return;

            var dialog = new SaveFileDialog
            {
                Filter = "Excel Macro-Enabled Workbook (*.xlsm)|*.xlsm",
                FileName = $"{SelectedTeacher.FullName}-Преподаватель.xlsm",
                Title = "Сохранить данные преподавателя"
            };

            if (dialog.ShowDialog() == true)
            {
                StatusText = "Экспорт данных в Excel...";
                IsLoading = true; // Можно использовать существующий флаг для индикации

                try
                {
                    string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExcelTemplate/template.xlsm");
                    if (!File.Exists(templatePath))
                    {
                        MessageBox.Show("Файл шаблона 'template.xlsm' не найден.", "Ошибка экспорта", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var exportService = new ExcelExportService();
                    // Выполняем ресурсоемкую операцию в фоновом потоке
                    await Task.Run(() => exportService.ExportTeacherData(SelectedTeacher, templatePath, dialog.FileName));

                    MessageBox.Show($"Данные успешно экспортированы в файл:\n{dialog.FileName}", "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    Logger.Write(ex, "EXPORT-EXCEL");
                    MessageBox.Show($"Произошла ошибка при экспорте: {ex.Message}", "Ошибка экспорта", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                    StatusText = "Экспорт завершен.";
                }
            }
        }

        /// <summary>
        /// Меняет ID учителя и всех его дочерних записей с временного на реальный.
        /// </summary>
        private void ReplaceTeacherIdInVm(TeacherViewModel vm, Guid realId)
        {
            vm.Model.Id = realId;
            foreach (var item in vm.AcademicResults) item.TeacherId = realId;
            foreach (var item in vm.IntermediateAssessments) item.TeacherId = realId;
            foreach (var item in vm.GiaResults) item.TeacherId = realId;
            foreach (var item in vm.DemoExamResults) item.TeacherId = realId;
            foreach (var item in vm.IndependentAssessments) item.TeacherId = realId;
            foreach (var item in vm.SelfDeterminations) item.TeacherId = realId;
            foreach (var item in vm.StudentOlympiads) item.TeacherId = realId;
            foreach (var item in vm.JuryActivities) item.TeacherId = realId;
            foreach (var item in vm.MasterClasses) item.TeacherId = realId;
            foreach (var item in vm.Speeches) item.TeacherId = realId;
            foreach (var item in vm.Publications) item.TeacherId = realId;
            foreach (var item in vm.ExperimentalProjects) item.TeacherId = realId;
            foreach (var item in vm.Mentorships) item.TeacherId = realId;
            foreach (var item in vm.ProgramSupports) item.TeacherId = realId;
            foreach (var item in vm.ProfessionalCompetitions) item.TeacherId = realId;
        }
    }
}