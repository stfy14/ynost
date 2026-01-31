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
        [NotifyCanExecuteChangedFor(nameof(ValidateAndFillDataCommand))]
        [NotifyCanExecuteChangedFor(nameof(ImportFromExcelCommand))]
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
        [NotifyCanExecuteChangedFor(nameof(ValidateAndFillDataCommand))]
        [NotifyCanExecuteChangedFor(nameof(ImportFromExcelCommand))]
        private TeacherViewModel? selectedTeacher;

        public IAsyncRelayCommand RetryConnectionCommand { get; }
        public IAsyncRelayCommand SaveChangesCommand { get; }
        public IRelayCommand ToggleLoginCommand { get; }
        public IRelayCommand AddStaffCommand { get; }
        public IAsyncRelayCommand DeleteStaffCommand { get; }
        public IAsyncRelayCommand ExportToExcelCommand { get; }
        public IAsyncRelayCommand ImportFromExcelCommand { get; }
        public IAsyncRelayCommand ValidateAndFillDataCommand { get; }
        public IAsyncRelayCommand ValidateAllTeachersCommand { get; }


        public MainViewModel()
        {
            _db = App.Db;

            RetryConnectionCommand = new AsyncRelayCommand(LoadDataAsync, CanRetryConnection);
            SaveChangesCommand = new AsyncRelayCommand(ExecuteSaveChanges, CanExecuteSaveChanges);
            ToggleLoginCommand = new RelayCommand(ExecuteToggleLogin);
            AddStaffCommand = new RelayCommand(ExecuteAddStaff, () => CanEditData);
            DeleteStaffCommand = new AsyncRelayCommand(ExecuteDeleteStaff, CanExecuteDeleteStaff);
            ExportToExcelCommand = new AsyncRelayCommand(ExportDataToExcel, CanExportDataToExcel);
            ImportFromExcelCommand = new AsyncRelayCommand(ImportDataFromExcel, () => SelectedTeacher != null && CanEditData);
            ValidateAndFillDataCommand = new AsyncRelayCommand(ExecuteValidateAndFill, () => SelectedTeacher != null && CanEditData );
            ValidateAllTeachersCommand = new AsyncRelayCommand(ExecuteValidateAllTeachers, () => CanEditData && Teachers.Count > 0);

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
                    ImportFromExcelCommand.NotifyCanExecuteChanged();
                    ValidateAndFillDataCommand.NotifyCanExecuteChanged(); // Для одного
                    ValidateAllTeachersCommand.NotifyCanExecuteChanged(); // Для всех (новая)
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

        private async Task ImportDataFromExcel()
        {
            if (SelectedTeacher == null) return;

            var result = MessageBox.Show(
                $"ВНИМАНИЕ! Импорт полностью заменит текущие данные для преподавателя '{SelectedTeacher.FullName}' данными из Excel.\n\n" +
                "Вы хотите продолжить?",
                "Подтверждение импорта",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            var dialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsm;*.xlsx",
                Title = "Импорт портфолио преподавателя"
            };

            if (dialog.ShowDialog() == true)
            {
                IsLoading = true;
                StatusText = "Импорт данных из Excel...";

                try
                {
                    var importService = new ExcelImportService();

                    // Выполняем импорт (парсинг + обновление коллекций)
                    // ObservableCollection в TeacherViewModel вызовет события изменения,
                    // что активирует трекер изменений (HasChanges = true)
                    await Task.Run(() =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            importService.ImportTeacherData(dialog.FileName, SelectedTeacher);
                        });
                    });

                    StatusText = "Импорт выполнен. Нажмите 'Сохранить'.";
                    MessageBox.Show(
                        "Данные успешно загружены из файла.\n\n" +
                        "Изменения отображены на экране, но еще НЕ сохранены в БД.\n" +
                        "Нажмите кнопку 'СОХРАНИТЬ', чтобы применить изменения.",
                        "Импорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    Logger.Write(ex, "IMPORT-EXCEL-MAIN");
                    MessageBox.Show($"Ошибка при импорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText = "Ошибка импорта.";
                }
                finally
                {
                    IsLoading = false;
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

        private async Task ExecuteValidateAndFill()
        {
            if (SelectedTeacher == null) return;

            // Защита: работает только если учитель сохранен (имеет имя для поиска)
            if (string.IsNullOrWhiteSpace(SelectedTeacher.FullName) || SelectedTeacher.FullName == "Новый преподаватель")
            {
                MessageBox.Show("Сначала введите корректное ФИО преподавателя и сохраните его.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Вы хотите запустить ВАЛИДАЦИЮ для: {SelectedTeacher.FullName}?\n\n" +
                "Программа скачает данные из Google Sheets и заполнит ТОЛЬКО пустые ячейки.\n" +
                "Существующие данные НЕ будут изменены.",
                "Валидация данных",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            IsLoading = true;
            StatusText = "Валидация: Скачивание таблицы...";

            try
            {
                // 1. Создаем "Призрака" для парсинга
                var tempId = Guid.NewGuid();
                var ghostVm = new TeacherViewModel(new Teacher { Id = tempId, FullName = SelectedTeacher.FullName, Version = 1 });

                // 2. Скачиваем и парсим
                var service = new GoogleSheetImportService();
                byte[] fileData = await service.DownloadSheetAsync();

                if (fileData == null)
                {
                    MessageBox.Show("Не удалось скачать таблицу.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Парсим данные СТРОГО по имени текущего преподавателя
                StatusText = "Валидация: Поиск и разбор данных...";
                bool found = await Task.Run(() => service.ParseDataForExactName(fileData, ghostVm, SelectedTeacher.FullName));

                if (!found)
                {
                    MessageBox.Show($"Данные для '{SelectedTeacher.FullName}' не найдены в Google Sheets.", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    StatusText = "Валидация: Заполнение пропусков...";

                    // 3. МАГИЯ: Заполняем только пустые ячейки
                    int filledCount = FillEmptyFields(ghostVm, SelectedTeacher);

                    if (filledCount > 0)
                    {
                        MessageBox.Show($"Валидация завершена!\nЗаполнено пустых полей/строк: {filledCount}.\n\nНе забудьте нажать 'Сохранить'.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Валидация завершена. Новых данных для заполнения пустых мест не найдено.\n(Все данные либо уже есть, либо отсутствуют в таблице).", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при валидации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Write(ex, "VALIDATION");
            }
            finally
            {
                IsLoading = false;
                StatusText = IsDatabaseConnected ? "Готово." : ConnectionStatusText;
            }
        }

        // === НОВЫЙ МЕТОД ДЛЯ МАССОВОЙ ВАЛИДАЦИИ ===
        private async Task ExecuteValidateAllTeachers()
        {
            if (!IsDatabaseConnected) return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите проверить ВСЕХ преподавателей ({Teachers.Count} чел.)?\n\n" +
                "Программа скачает таблицу и заполнит пустые ячейки.\n" +
                "Существующие данные не изменятся.",
                "Массовая валидация",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            IsLoading = true;
            StatusText = "Массовая валидация: Скачивание таблицы...";

            try
            {
                var service = new GoogleSheetImportService();
                byte[] fileData = await service.DownloadSheetAsync();

                if (fileData == null)
                {
                    MessageBox.Show("Не удалось скачать таблицу.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int totalChanges = 0;
                int teachersAffected = 0;

                await Task.Run(() =>
                {
                    var list = Teachers.ToList(); // Копия списка для перебора
                    int count = 0;

                    foreach (var teacher in list)
                    {
                        count++;
                        Application.Current.Dispatcher.Invoke(() =>
                            StatusText = $"Обработка {count}/{list.Count}: {teacher.FullName}...");

                        if (string.IsNullOrWhiteSpace(teacher.FullName) || teacher.FullName == "Новый преподаватель") continue;

                        // Создаем временную VM для парсинга
                        var ghostVm = new TeacherViewModel(new Teacher { Id = Guid.NewGuid(), FullName = teacher.FullName, Version = 1 });

                        // Парсим
                        bool found = service.ParseDataForExactName(fileData, ghostVm, teacher.FullName);

                        if (found)
                        {
                            // Применяем изменения в UI потоке
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                int changes = FillEmptyFields(ghostVm, teacher);
                                if (changes > 0)
                                {
                                    totalChanges += changes;
                                    teachersAffected++;
                                }
                            });
                        }
                    }
                });

                MessageBox.Show($"Готово!\nОбновлено учителей: {teachersAffected}\nЗаполнено полей: {totalChanges}\n\nНажмите 'Сохранить'.",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                StatusText = "Готово.";
            }
        }

        /// <summary>
        /// Сравнивает данные из Excel (source) с текущими данными (target).
        /// Заполняет ТОЛЬКО пустые поля в target. Не перезаписывает существующие данные.
        /// Добавляет новые строки, если их не было.
        /// </summary>
        private int FillEmptyFields(TeacherViewModel source, TeacherViewModel target)
        {
            int changes = 0;

            // --- 1. AcademicResults (Ключ: Предмет + Группа) ---
            for (int i = 0; i < source.AcademicResults.Count; i++)
            {
                var src = source.AcademicResults[i];
                var tgt = target.AcademicResults.FirstOrDefault(x => x.Subject == src.Subject && x.Group == src.Group);
                if (tgt == null)
                {
                    var newItem = new AcademicYearResult(); CopyProperties(src, newItem); newItem.TeacherId = target.Id;
                    SetNewlyFilledFlag(newItem, true);
                    int index = FindInsertionIndex(target.AcademicResults, source.AcademicResults, i, (s, t) => s.Subject == t.Subject && s.Group == t.Group);
                    target.AcademicResults.Insert(index, newItem);
                    changes++;
                }
                else
                {
                    bool rowChanged = false;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.AvgSem1)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.AvgSem2)) > 0;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.AvgSuccessRate)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.AvgSuccessRateSem2)) > 0;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.AvgQualityRate)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.AvgQualityRateSem2)) > 0;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.EntrySouRate)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.ExitSouRate)) > 0;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Intermediate)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Link)) > 0;
                    if (rowChanged) { SetNewlyFilledFlag(tgt, true); changes++; }
                }
            }

            // --- 1A. IntermediateAssessments (Ключ: Предмет + Учебный год) ---
            for (int i = 0; i < source.IntermediateAssessments.Count; i++)
            {
                var src = source.IntermediateAssessments[i];
                var tgt = target.IntermediateAssessments.FirstOrDefault(x => x.Subject == src.Subject && x.AcademicYear == src.AcademicYear);
                if (tgt == null)
                {
                    var newItem = new IntermediateAssessment(); CopyProperties(src, newItem); newItem.TeacherId = target.Id;
                    SetNewlyFilledFlag(newItem, true);
                    int index = FindInsertionIndex(target.IntermediateAssessments, source.IntermediateAssessments, i, (s, t) => s.Subject == t.Subject && s.AcademicYear == t.AcademicYear);
                    target.IntermediateAssessments.Insert(index, newItem);
                    changes++;
                }
                else
                {
                    bool rowChanged = false;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.AvgScore)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Quality)) > 0;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Sou)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Link)) > 0;
                    if (rowChanged) { SetNewlyFilledFlag(tgt, true); changes++; }
                }
            }

            // --- 2. GiaResults (Ключ: Предмет + Группа) ---
            for (int i = 0; i < source.GiaResults.Count; i++)
            {
                var src = source.GiaResults[i];
                var tgt = target.GiaResults.FirstOrDefault(x => x.Subject == src.Subject && x.Group == src.Group);
                if (tgt == null)
                {
                    var newItem = new GiaResult(); CopyProperties(src, newItem); newItem.TeacherId = target.Id;
                    SetNewlyFilledFlag(newItem, true);
                    int index = FindInsertionIndex(target.GiaResults, source.GiaResults, i, (s, t) => s.Subject == t.Subject && s.Group == t.Group);
                    target.GiaResults.Insert(index, newItem);
                    changes++;
                }
                else
                {
                    bool rowChanged = false;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.TotalParticipants)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Count5)) > 0;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Count4)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Count3)) > 0;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Count2)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.AvgScore)) > 0;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Link)) > 0;
                    if (rowChanged) { SetNewlyFilledFlag(tgt, true); changes++; }
                }
            }

            // --- 3. DemoExamResults (Ключ: Предмет + Группа) ---
            for (int i = 0; i < source.DemoExamResults.Count; i++)
            {
                var src = source.DemoExamResults[i];
                var tgt = target.DemoExamResults.FirstOrDefault(x => x.Subject == src.Subject && x.Group == src.Group);
                if (tgt == null)
                {
                    var newItem = new DemoExamResult(); CopyProperties(src, newItem); newItem.TeacherId = target.Id;
                    SetNewlyFilledFlag(newItem, true);
                    int index = FindInsertionIndex(target.DemoExamResults, source.DemoExamResults, i, (s, t) => s.Subject == t.Subject && s.Group == t.Group);
                    target.DemoExamResults.Insert(index, newItem);
                    changes++;
                }
                else
                {
                    bool rowChanged = false;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.TotalParticipants)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Count5)) > 0;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Count4)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Count3)) > 0;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Count2)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.AvgScore)) > 0;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Link)) > 0;
                    if (rowChanged) { SetNewlyFilledFlag(tgt, true); changes++; }
                }
            }

            // --- 4. IndependentAssessments (Ключ: Название + Дата) ---
            for (int i = 0; i < source.IndependentAssessments.Count; i++)
            {
                var src = source.IndependentAssessments[i];
                var tgt = target.IndependentAssessments.FirstOrDefault(x => x.AssessmentName == src.AssessmentName && x.AssessmentDate == src.AssessmentDate);
                if (tgt == null)
                {
                    var newItem = new IndependentAssessment(); CopyProperties(src, newItem); newItem.TeacherId = target.Id;
                    SetNewlyFilledFlag(newItem, true);
                    int index = FindInsertionIndex(target.IndependentAssessments, source.IndependentAssessments, i, (s, t) => s.AssessmentName == t.AssessmentName && s.AssessmentDate == t.AssessmentDate);
                    target.IndependentAssessments.Insert(index, newItem);
                    changes++;
                }
                else
                {
                    bool rowChanged = false;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.ClassSubject)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.StudentsTotal)) > 0;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.StudentsParticipated)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.StudentsPassed)) > 0;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Count5)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Count4)) > 0;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Count3)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Count2)) > 0;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Link)) > 0;
                    if (rowChanged) { SetNewlyFilledFlag(tgt, true); changes++; }
                }
            }

            // --- 5. SelfDeterminations (Ключ: Название мероприятия) ---
            for (int i = 0; i < source.SelfDeterminations.Count; i++)
            {
                var src = source.SelfDeterminations[i];
                var tgt = target.SelfDeterminations.FirstOrDefault(x => x.Name == src.Name);
                if (tgt == null)
                {
                    var newItem = new SelfDeterminationActivity(); CopyProperties(src, newItem); newItem.TeacherId = target.Id;
                    SetNewlyFilledFlag(newItem, true);
                    int index = FindInsertionIndex(target.SelfDeterminations, source.SelfDeterminations, i, (s, t) => s.Name == t.Name);
                    target.SelfDeterminations.Insert(index, newItem);
                    changes++;
                }
                else
                {
                    bool rowChanged = false;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Level)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Role)) > 0;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Link)) > 0;
                    if (rowChanged) { SetNewlyFilledFlag(tgt, true); changes++; }
                }
            }

            // --- 6. StudentOlympiads (Ключ: Событие + Участник) ---
            for (int i = 0; i < source.StudentOlympiads.Count; i++)
            {
                var src = source.StudentOlympiads[i];
                var tgt = target.StudentOlympiads.FirstOrDefault(x => x.Name == src.Name && x.Cadet == src.Cadet);
                if (tgt == null)
                {
                    var newItem = new StudentOlympiad(); CopyProperties(src, newItem); newItem.TeacherId = target.Id;
                    SetNewlyFilledFlag(newItem, true);
                    int index = FindInsertionIndex(target.StudentOlympiads, source.StudentOlympiads, i, (s, t) => s.Name == t.Name && s.Cadet == t.Cadet);
                    target.StudentOlympiads.Insert(index, newItem);
                    changes++;
                }
                else
                {
                    bool rowChanged = false;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Level)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Form)) > 0;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Result)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Link)) > 0;
                    if (rowChanged) { SetNewlyFilledFlag(tgt, true); changes++; }
                }
            }

            // --- 7. JuryActivities (Ключ: Мероприятие + Дата) ---
            for (int i = 0; i < source.JuryActivities.Count; i++)
            {
                var src = source.JuryActivities[i];
                var tgt = target.JuryActivities.FirstOrDefault(x => x.Name == src.Name && x.EventDate == src.EventDate);
                if (tgt == null)
                {
                    var newItem = new JuryActivity(); CopyProperties(src, newItem); newItem.TeacherId = target.Id;
                    SetNewlyFilledFlag(newItem, true);
                    int index = FindInsertionIndex(target.JuryActivities, source.JuryActivities, i, (s, t) => s.Name == t.Name && s.EventDate == t.EventDate);
                    target.JuryActivities.Insert(index, newItem);
                    changes++;
                }
                else
                {
                    bool rowChanged = false;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Level)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Link)) > 0;
                    if (rowChanged) { SetNewlyFilledFlag(tgt, true); changes++; }
                }
            }

            // --- 8. MasterClasses (Ключ: Наименование + Дата) ---
            for (int i = 0; i < source.MasterClasses.Count; i++)
            {
                var src = source.MasterClasses[i];
                var tgt = target.MasterClasses.FirstOrDefault(x => x.Name == src.Name && x.EventDate == src.EventDate);
                if (tgt == null)
                {
                    var newItem = new MasterClass(); CopyProperties(src, newItem); newItem.TeacherId = target.Id;
                    SetNewlyFilledFlag(newItem, true);
                    int index = FindInsertionIndex(target.MasterClasses, source.MasterClasses, i, (s, t) => s.Name == t.Name && s.EventDate == t.EventDate);
                    target.MasterClasses.Insert(index, newItem);
                    changes++;
                }
                else
                {
                    bool rowChanged = false;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Level)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Link)) > 0;
                    if (rowChanged) { SetNewlyFilledFlag(tgt, true); changes++; }
                }
            }

            // --- 9. Speeches (Ключ: Наименование + Дата) ---
            for (int i = 0; i < source.Speeches.Count; i++)
            {
                var src = source.Speeches[i];
                var tgt = target.Speeches.FirstOrDefault(x => x.Name == src.Name && x.EventDate == src.EventDate);
                if (tgt == null)
                {
                    var newItem = new Speech(); CopyProperties(src, newItem); newItem.TeacherId = target.Id;
                    SetNewlyFilledFlag(newItem, true);
                    int index = FindInsertionIndex(target.Speeches, source.Speeches, i, (s, t) => s.Name == t.Name && s.EventDate == t.EventDate);
                    target.Speeches.Insert(index, newItem);
                    changes++;
                }
                else
                {
                    bool rowChanged = false;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Level)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Link)) > 0;
                    if (rowChanged) { SetNewlyFilledFlag(tgt, true); changes++; }
                }
            }

            // --- 10. Publications (Ключ: Заголовок) ---
            for (int i = 0; i < source.Publications.Count; i++)
            {
                var src = source.Publications[i];
                var tgt = target.Publications.FirstOrDefault(x => x.Title == src.Title);
                if (tgt == null)
                {
                    var newItem = new Publication(); CopyProperties(src, newItem); newItem.TeacherId = target.Id;
                    SetNewlyFilledFlag(newItem, true);
                    int index = FindInsertionIndex(target.Publications, source.Publications, i, (s, t) => s.Title == t.Title);
                    target.Publications.Insert(index, newItem);
                    changes++;
                }
                else
                {
                    bool rowChanged = false;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Level)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Date)) > 0;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Link)) > 0;
                    if (rowChanged) { SetNewlyFilledFlag(tgt, true); changes++; }
                }
            }

            // --- 11. ExperimentalProjects (Ключ: Название проекта) ---
            for (int i = 0; i < source.ExperimentalProjects.Count; i++)
            {
                var src = source.ExperimentalProjects[i];
                var tgt = target.ExperimentalProjects.FirstOrDefault(x => x.Name == src.Name);
                if (tgt == null)
                {
                    var newItem = new ExperimentalProject(); CopyProperties(src, newItem); newItem.TeacherId = target.Id;
                    SetNewlyFilledFlag(newItem, true);
                    int index = FindInsertionIndex(target.ExperimentalProjects, source.ExperimentalProjects, i, (s, t) => s.Name == t.Name);
                    target.ExperimentalProjects.Insert(index, newItem);
                    changes++;
                }
                else
                {
                    bool rowChanged = false;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Date)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Link)) > 0;
                    if (rowChanged) { SetNewlyFilledFlag(tgt, true); changes++; }
                }
            }

            // --- 12. Mentorships (Ключ: ФИО стажера) ---
            for (int i = 0; i < source.Mentorships.Count; i++)
            {
                var src = source.Mentorships[i];
                var tgt = target.Mentorships.FirstOrDefault(x => x.Trainee == src.Trainee);
                if (tgt == null)
                {
                    var newItem = new Mentorship(); CopyProperties(src, newItem); newItem.TeacherId = target.Id;
                    SetNewlyFilledFlag(newItem, true);
                    int index = FindInsertionIndex(target.Mentorships, source.Mentorships, i, (s, t) => s.Trainee == t.Trainee);
                    target.Mentorships.Insert(index, newItem);
                    changes++;
                }
                else
                {
                    bool rowChanged = false;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.OrderNo)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.OrderDate)) > 0;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Link)) > 0;
                    if (rowChanged) { SetNewlyFilledFlag(tgt, true); changes++; }
                }
            }

            // --- 13. ProgramSupports (Ключ: Имя программы) ---
            for (int i = 0; i < source.ProgramSupports.Count; i++)
            {
                var src = source.ProgramSupports[i];
                var tgt = target.ProgramSupports.FirstOrDefault(x => x.ProgramName == src.ProgramName);
                if (tgt == null)
                {
                    var newItem = new ProgramMethodSupport(); CopyProperties(src, newItem); newItem.TeacherId = target.Id;
                    SetNewlyFilledFlag(newItem, true);
                    int index = FindInsertionIndex(target.ProgramSupports, source.ProgramSupports, i, (s, t) => s.ProgramName == t.ProgramName);
                    target.ProgramSupports.Insert(index, newItem);
                    changes++;
                }
                else
                {
                    bool rowChanged = false;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Link)) > 0;
                    if (rowChanged) { SetNewlyFilledFlag(tgt, true); changes++; }
                }
            }

            // --- 14. ProfessionalCompetitions (Ключ: Название конкурса + Дата) ---
            for (int i = 0; i < source.ProfessionalCompetitions.Count; i++)
            {
                var src = source.ProfessionalCompetitions[i];
                var tgt = target.ProfessionalCompetitions.FirstOrDefault(x => x.Name == src.Name && x.EventDate == src.EventDate);
                if (tgt == null)
                {
                    var newItem = new ProfessionalCompetition(); CopyProperties(src, newItem); newItem.TeacherId = target.Id;
                    SetNewlyFilledFlag(newItem, true);
                    int index = FindInsertionIndex(target.ProfessionalCompetitions, source.ProfessionalCompetitions, i, (s, t) => s.Name == t.Name && s.EventDate == t.EventDate);
                    target.ProfessionalCompetitions.Insert(index, newItem);
                    changes++;
                }
                else
                {
                    bool rowChanged = false;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Level)) > 0; rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Achievement)) > 0;
                    rowChanged |= CopyIfEmpty(src, tgt, nameof(src.Link)) > 0;
                    if (rowChanged) { SetNewlyFilledFlag(tgt, true); changes++; }
                }
            }

            return changes;
        }

        #region [Helpers for Filling]

        // Устанавливает флаг IsNewlyFilled через рефлексию
        private void SetNewlyFilledFlag(object item, bool value)
        {
            var prop = item.GetType().GetProperty("IsNewlyFilled");
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(item, value);
            }
        }

        /// <summary>
        /// Находит правильный индекс для вставки нового элемента в коллекцию,
        /// сохраняя относительный порядок, как в исходном списке (sourceCollection).
        /// </summary>
        private int FindInsertionIndex<T>(
            ObservableCollection<T> targetCollection,
            IList<T> sourceCollection,
            int currentSourceIndex,
            Func<T, T, bool> areEqual) where T : class
        {
            // Идем НАЗАД от текущей позиции в исходном списке
            for (int i = currentSourceIndex - 1; i >= 0; i--)
            {
                var prevSourceItem = sourceCollection[i];
                // Ищем предыдущий элемент из Excel в нашей текущей коллекции на экране
                var anchorInTarget = targetCollection.FirstOrDefault(t => areEqual(prevSourceItem, t));
                if (anchorInTarget != null)
                {
                    // Нашли "якорь"! Вставляем наш новый элемент сразу после него.
                    return targetCollection.IndexOf(anchorInTarget) + 1;
                }
            }
            // Если мы не нашли ни одного предыдущего элемента, значит, это первый. Вставляем в начало.
            return 0;
        }

        // -------------------------------------------------------------
        // Вспомогательные методы
        // -------------------------------------------------------------

        /// <summary>
        /// Копирует значение строкового свойства из source в target, 
        /// ТОЛЬКО ЕСЛИ в target оно пустое/null, а в source есть значение.
        /// </summary>
        private int CopyIfEmpty(object source, object target, string propName)
        {
            var prop = source.GetType().GetProperty(propName);
            if (prop == null) return 0;

            var sourceVal = prop.GetValue(source)?.ToString();
            var targetVal = prop.GetValue(target)?.ToString();

            // Если у нас (target) пусто, а в источнике (source) есть данные -> копируем
            if (string.IsNullOrWhiteSpace(targetVal) && !string.IsNullOrWhiteSpace(sourceVal))
            {
                prop.SetValue(target, sourceVal);
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Копирует все свойства из одного объекта в другой (для создания новых записей).
        /// Игнорирует ID и служебные поля.
        /// </summary>
        private void CopyProperties(object source, object target)
        {
            foreach (var prop in source.GetType().GetProperties())
            {
                // Копируем только то, что можно писать, и не является служебным ID
                if (prop.CanWrite &&
                    prop.Name != "Id" &&
                    prop.Name != "TeacherId" &&
                    prop.Name != "TeachId" &&
                    prop.Name != "Version" &&
                    prop.Name != "IsConflicting")
                {
                    var val = prop.GetValue(source);
                    prop.SetValue(target, val);
                }
            }
        }

        /// <summary>
        /// Находит правильный индекс для вставки нового элемента в коллекцию,
        /// сохраняя относительный порядок, как в исходном списке (sourceCollection).
        /// </summary>
        /// <param name="targetCollection">Коллекция, в которую нужно вставить элемент.</param>
        /// <param name="sourceCollection">Полный список из Excel, служащий эталоном порядка.</param>
        /// <param name="currentSourceIndex">Индекс элемента в sourceCollection, который мы хотим вставить.</param>
        /// <param name="areEqual">Функция для сравнения двух элементов на идентичность.</param>
        private int FindInsertionIndex<T>(
            ObservableCollection<T> targetCollection,
            List<T> sourceCollection,
            int currentSourceIndex,
            Func<T, T, bool> areEqual) where T : class
        {
            // Идем НАЗАД от текущей позиции в исходном списке
            for (int i = currentSourceIndex - 1; i >= 0; i--)
            {
                var prevSourceItem = sourceCollection[i];

                // Ищем предыдущий элемент из Excel в нашей текущей коллекции на экране
                var anchorInTarget = targetCollection.FirstOrDefault(t => areEqual(prevSourceItem, t));

                if (anchorInTarget != null)
                {
                    // Нашли "якорь"! Значит, наш новый элемент нужно вставить сразу после него.
                    return targetCollection.IndexOf(anchorInTarget) + 1;
                }
            }

            // Если мы прошли весь цикл и не нашли ни одного предыдущего элемента,
            // значит, это первый элемент из списка, который мы добавляем. Вставляем его в начало.
            return 0;
        }
    }
    #endregion
}