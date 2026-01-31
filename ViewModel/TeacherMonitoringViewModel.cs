// File: ViewModels/TeacherMonitoringViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Ynost.Extensions;
using Ynost.Models;
using Ynost.Services;

namespace Ynost.ViewModels
{
    public partial class TeacherMonitoringViewModel : ObservableObject
    {
        private readonly DatabaseService _db;

        #region 0. Лог и базовые свойства
        public ObservableCollection<string> LogEntries { get; } = new();
        private void Log(string msg) =>
            Application.Current.Dispatcher.Invoke(() => LogEntries.Insert(0, $"{DateTime.Now:HH:mm:ss}  {msg}"));

        public ObservableCollection<Teach> TeachList { get; } = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteTeachCommand))]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        [NotifyCanExecuteChangedFor(nameof(ReloadMonitoringCommand))]
        [NotifyCanExecuteChangedFor(nameof(ExportToExcelCommand))]
        private Teach? _selectedTeach;
        #endregion

        #region 2. Коллекции мониторинга и выбранные элементы
        public ObservableCollection<AcademicYearResultTeacher> AcademicYearResults { get; } = new();
        public ObservableCollection<AcademicResultTeacher> AcademicResults { get; } = new();
        public ObservableCollection<GiaResultTeacher> GiaResults { get; } = new();
        public ObservableCollection<OgeResultTeacher> OgeResults { get; } = new();
        public ObservableCollection<IndependentAssessmentTeacher> IndependentAssessments { get; } = new();
        public ObservableCollection<SelfDeterminationTeacher> SelfDeterminations { get; } = new();
        public ObservableCollection<StudentOlympiadTeacher> StudentOlympiads { get; } = new();
        public ObservableCollection<JuryActivityTeacher> JuryActivities { get; } = new();
        public ObservableCollection<MasterClassTeacher> MasterClasses { get; } = new();
        public ObservableCollection<SpeechTeacher> Speeches { get; } = new();
        public ObservableCollection<PublicationTeacher> Publications { get; } = new();
        public ObservableCollection<ExperimentalProjectTeacher> ExperimentalProjects { get; } = new();
        public ObservableCollection<MentorshipTeacher> Mentorships { get; } = new();
        public ObservableCollection<ProgramSupportTeacher> ProgramSupports { get; } = new();
        public ObservableCollection<ProfessionalCompetitionTeacher> ProfessionalCompetitions { get; } = new();

        [ObservableProperty] private AcademicYearResultTeacher? _selectedAcademicYear;
        [ObservableProperty] private AcademicResultTeacher? _selectedAcademicResult;
        [ObservableProperty] private GiaResultTeacher? _selectedGiaResult;
        [ObservableProperty] private OgeResultTeacher? _selectedOgeResult;
        [ObservableProperty] private IndependentAssessmentTeacher? _selectedIndependentAssessment;
        [ObservableProperty] private SelfDeterminationTeacher? _selectedSelfDetermination;
        [ObservableProperty] private StudentOlympiadTeacher? _selectedStudentOlympiad;
        [ObservableProperty] private JuryActivityTeacher? _selectedJuryActivity;
        [ObservableProperty] private MasterClassTeacher? _selectedMasterClass;
        [ObservableProperty] private SpeechTeacher? _selectedSpeech;
        [ObservableProperty] private PublicationTeacher? _selectedPublication;
        [ObservableProperty] private ExperimentalProjectTeacher? _selectedExperimentalProject;
        [ObservableProperty] private MentorshipTeacher? _selectedMentorship;
        [ObservableProperty] private ProgramSupportTeacher? _selectedProgramSupport;
        [ObservableProperty] private ProfessionalCompetitionTeacher? _selectedProfessionalCompetition;
        #endregion

        #region 3. Команды (основные)
        public IAsyncRelayCommand LoadCommand { get; }
        public IAsyncRelayCommand SaveCommand { get; }
        public IRelayCommand SelectTeacherCommand { get; }
        public IRelayCommand ReloadMonitoringCommand { get; }
        public IAsyncRelayCommand AddTeachCommand { get; }
        public IAsyncRelayCommand DeleteTeachCommand { get; }
        public IAsyncRelayCommand ExportToExcelCommand { get; }
        #endregion

        #region 3.1 Команды (+/- для таблиц)
        public IRelayCommand AddAcademicYearResultCommand { get; private set; } = null!;
        public IRelayCommand DeleteAcademicYearResultCommand { get; private set; } = null!;
        public IRelayCommand AddAcademicResultCommand { get; private set; } = null!;
        public IRelayCommand DeleteAcademicResultCommand { get; private set; } = null!;
        public IRelayCommand AddGiaResultCommand { get; private set; } = null!;
        public IRelayCommand DeleteGiaResultCommand { get; private set; } = null!;
        public IRelayCommand AddOgeResultCommand { get; private set; } = null!;
        public IRelayCommand DeleteOgeResultCommand { get; private set; } = null!;
        public IRelayCommand AddIndependentAssessmentCommand { get; private set; } = null!;
        public IRelayCommand DeleteIndependentAssessmentCommand { get; private set; } = null!;
        public IRelayCommand AddSelfDeterminationCommand { get; private set; } = null!;
        public IRelayCommand DeleteSelfDeterminationCommand { get; private set; } = null!;
        public IRelayCommand AddStudentOlympiadCommand { get; private set; } = null!;
        public IRelayCommand DeleteStudentOlympiadCommand { get; private set; } = null!;
        public IRelayCommand AddJuryActivityCommand { get; private set; } = null!;
        public IRelayCommand DeleteJuryActivityCommand { get; private set; } = null!;
        public IRelayCommand AddMasterClassCommand { get; private set; } = null!;
        public IRelayCommand DeleteMasterClassCommand { get; private set; } = null!;
        public IRelayCommand AddSpeechCommand { get; private set; } = null!;
        public IRelayCommand DeleteSpeechCommand { get; private set; } = null!;
        public IRelayCommand AddPublicationCommand { get; private set; } = null!;
        public IRelayCommand DeletePublicationCommand { get; private set; } = null!;
        public IRelayCommand AddExperimentalProjectCommand { get; private set; } = null!;
        public IRelayCommand DeleteExperimentalProjectCommand { get; private set; } = null!;
        public IRelayCommand AddMentorshipCommand { get; private set; } = null!;
        public IRelayCommand DeleteMentorshipCommand { get; private set; } = null!;
        public IRelayCommand AddProgramSupportCommand { get; private set; } = null!;
        public IRelayCommand DeleteProgramSupportCommand { get; private set; } = null!;
        public IRelayCommand AddProfessionalCompetitionCommand { get; private set; } = null!;
        public IRelayCommand DeleteProfessionalCompetitionCommand { get; private set; } = null!;
        public IAsyncRelayCommand ImportFromExcelCommand { get; }
        public IRelayCommand<SubjectBoard> DeleteSpecificBoardCommand { get; }
        #endregion

        #region 1.1 Итоги по предметам (Boards)
        public ObservableCollection<YearlySubjectGroup> YearlyBoards { get; } = new();
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteBoardCommand))]
        private SubjectBoard? _selectedBoard;
        public IRelayCommand AddBoardCommand { get; }
        public IRelayCommand DeleteBoardCommand { get; }
        private string CurrentYear => SelectedAcademicYear?.AcademicYear ?? DateTime.Now.ToString("yyyy–yyyy");
        #endregion

        #region 4. Конструктор
        public TeacherMonitoringViewModel() : this(App.Db) { }

        public TeacherMonitoringViewModel(DatabaseService db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));

            LoadCommand = new AsyncRelayCommand(LoadAllAsync);
            SaveCommand = new AsyncRelayCommand(SaveAsync, () => SelectedTeach != null && AuthService.CanEdit);
            SelectTeacherCommand = new RelayCommand<Teach?>(t => SelectedTeach = t);
            ReloadMonitoringCommand = new RelayCommand(async () => await ReloadAsync(), () => SelectedTeach != null);
            AddTeachCommand = new AsyncRelayCommand(ExecuteAddTeach);
            DeleteTeachCommand = new AsyncRelayCommand(ExecuteDeleteTeach, () => SelectedTeach != null);
            ExportToExcelCommand = new AsyncRelayCommand(ExportMonitoringDataToExcel, () => SelectedTeach != null);
            ImportFromExcelCommand = new AsyncRelayCommand(ImportDataFromExcel, () => SelectedTeach != null);

            AddBoardCommand = new RelayCommand(() =>
            {
                var firstYearGroup = YearlyBoards.FirstOrDefault();
                string currentAcademicYear = DateHelper.GetCurrentAcademicYear();

                if (firstYearGroup == null || firstYearGroup.Year != currentAcademicYear)
                {
                    firstYearGroup = new YearlySubjectGroup { Year = currentAcademicYear };
                    YearlyBoards.Insert(0, firstYearGroup);
                }

                // Создаем новый предмет
                var newBoard = new SubjectBoard { SubjectName = "Новый предмет" };
                firstYearGroup.SubjectBoards.Add(newBoard);

                // ВАЖНО: Сразу делаем его выбранным, чтобы кнопка "Удалить" загорелась
                SelectedBoard = newBoard;
            });

            DeleteBoardCommand = new RelayCommand(() =>
            {
                if (SelectedBoard == null) return;
                // Находим родительскую группу (год) и удаляем предмет из нее
                foreach (var yearGroup in YearlyBoards)
                {
                    // ObservableCollection поддерживает метод Contains и Remove, всё будет работать
                    if (yearGroup.SubjectBoards.Contains(SelectedBoard))
                    {
                        yearGroup.SubjectBoards.Remove(SelectedBoard);
                        SelectedBoard = null;
                        break;
                    }
                }
            }, () => SelectedBoard != null);

            DeleteSpecificBoardCommand = new RelayCommand<SubjectBoard>(sb =>
            {
                if (sb == null) return;

                // Ищем, в каком году лежит этот предмет и удаляем его
                foreach (var group in YearlyBoards)
                {
                    if (group.SubjectBoards.Contains(sb))
                    {
                        group.SubjectBoards.Remove(sb);
                        // Если в году не осталось предметов, можно удалить и сам год (по желанию)
                        if (group.SubjectBoards.Count == 0)
                        {
                            YearlyBoards.Remove(group);
                        }
                        break;
                    }
                }
            });

            RegisterSection(AcademicYearResults, () => new AcademicYearResultTeacher(), nameof(SelectedAcademicYear));
            RegisterSection(AcademicResults, () => new AcademicResultTeacher(), nameof(SelectedAcademicResult));
            RegisterSection(GiaResults, () => new GiaResultTeacher(), nameof(SelectedGiaResult));
            RegisterSection(OgeResults, () => new OgeResultTeacher(), nameof(SelectedOgeResult));
            RegisterSection(IndependentAssessments, () => new IndependentAssessmentTeacher(), nameof(SelectedIndependentAssessment));
            RegisterSection(SelfDeterminations, () => new SelfDeterminationTeacher(), nameof(SelectedSelfDetermination));
            RegisterSection(StudentOlympiads, () => new StudentOlympiadTeacher(), nameof(SelectedStudentOlympiad));
            RegisterSection(JuryActivities, () => new JuryActivityTeacher(), nameof(SelectedJuryActivity));
            RegisterSection(MasterClasses, () => new MasterClassTeacher(), nameof(SelectedMasterClass));
            RegisterSection(Speeches, () => new SpeechTeacher(), nameof(SelectedSpeech));
            RegisterSection(Publications, () => new PublicationTeacher(), nameof(SelectedPublication));
            RegisterSection(ExperimentalProjects, () => new ExperimentalProjectTeacher(), nameof(SelectedExperimentalProject));
            RegisterSection(Mentorships, () => new MentorshipTeacher(), nameof(SelectedMentorship));
            RegisterSection(ProgramSupports, () => new ProgramSupportTeacher(), nameof(SelectedProgramSupport));
            RegisterSection(ProfessionalCompetitions, () => new ProfessionalCompetitionTeacher(), nameof(SelectedProfessionalCompetition));
        }
        #endregion

        #region 5. Обработчики событий и частичные методы
        partial void OnSelectedTeachChanged(Teach? oldValue, Teach? newValue)
        {
            _ = ReloadAsync();
            ImportFromExcelCommand.NotifyCanExecuteChanged(); // <--- Добавить это
        }

        partial void OnSelectedAcademicYearChanged(AcademicYearResultTeacher? oldValue, AcademicYearResultTeacher? newValue)
        {
            _ = LoadBoardsAsync();
        }
        #endregion

        #region 6. Методы загрузки и сохранения
        private async Task LoadAllAsync()
        {
            try
            {
                Log("Запускаю загрузку учителей…");
                var teaches = await _db.LoadAllTeachesAsync();

                TeachList.Clear();
                foreach (var t in teaches ?? Enumerable.Empty<Teach>())
                    TeachList.Add(t);

                Log($"✔ Загрузили {TeachList.Count} учителей");
                SelectedTeach = TeachList.FirstOrDefault();
            }
            catch (Exception ex) { Log($"❌ {ex.GetType().Name}: {ex.Message}"); }
        }

        private async Task ReloadAsync()
        {
            ClearAllSections();
            if (SelectedTeach == null)
            {
                Log("Нет выбранного учителя, мониторинг не загружаю.");
                return;
            }

            var id = SelectedTeach.Id;
            Log($"Загружаю мониторинг для {SelectedTeach.FullName}");

            AcademicYearResults.AddRange(await _db.LoadAcademicYearResultsTeacherAsync(id));
            AcademicResults.AddRange(await _db.LoadAcademicResultsTeacherAsync(id));
            GiaResults.AddRange(await _db.LoadGiaResultsTeacherAsync(id));
            OgeResults.AddRange(await _db.LoadOgeResultsTeacherAsync(id));
            IndependentAssessments.AddRange(await _db.LoadIndependentAssessmentsTeacherAsync(id));
            SelfDeterminations.AddRange(await _db.LoadSelfDeterminationsTeacherAsync(id));
            StudentOlympiads.AddRange(await _db.LoadStudentOlympiadsTeacherAsync(id));
            JuryActivities.AddRange(await _db.LoadJuryActivitiesTeacherAsync(id));
            MasterClasses.AddRange(await _db.LoadMasterClassesTeacherAsync(id));
            Speeches.AddRange(await _db.LoadSpeechesTeacherAsync(id));
            Publications.AddRange(await _db.LoadPublicationsTeacherAsync(id));
            ExperimentalProjects.AddRange(await _db.LoadExperimentalProjectsTeacherAsync(id));
            Mentorships.AddRange(await _db.LoadMentorshipsTeacherAsync(id));
            ProgramSupports.AddRange(await _db.LoadProgramSupportsTeacherAsync(id));
            ProfessionalCompetitions.AddRange(await _db.LoadProfessionalCompetitionsTeacherAsync(id));

            await LoadBoardsAsync();

            Log("✔ Мониторинг загружен");
        }

        private void ClearAllSections()
        {
            AcademicYearResults.Clear(); AcademicResults.Clear(); GiaResults.Clear(); OgeResults.Clear();
            IndependentAssessments.Clear(); SelfDeterminations.Clear(); StudentOlympiads.Clear(); JuryActivities.Clear();
            MasterClasses.Clear(); Speeches.Clear(); Publications.Clear(); ExperimentalProjects.Clear();
            Mentorships.Clear(); ProgramSupports.Clear(); ProfessionalCompetitions.Clear();
            YearlyBoards.Clear();
        }

        private async Task SaveAsync()
        {
            if (SelectedTeach == null) return;

            Log("💾 Сохраняю мониторинг…");

            bool ok = await _db.SaveTeacherMonitoringAsync(
                SelectedTeach.Id,
                AcademicYearResults, AcademicResults, GiaResults, OgeResults,
                IndependentAssessments, SelfDeterminations, StudentOlympiads,
                JuryActivities, MasterClasses, Speeches, Publications,
                ExperimentalProjects, Mentorships, ProgramSupports, ProfessionalCompetitions);

            if (ok)
            {
                await SaveBoardsAsync();
                Log("✔ Мониторинг сохранён");
                await ReloadAsync();
            }
            else
            {
                Log($"❌ Ошибка при сохранении → {_db.LastError}");
            }
        }
        #endregion

        #region 7. Логика для таблицы 1.1 (Boards)
        private async Task LoadBoardsAsync()
        {
            YearlyBoards.Clear();
            if (SelectedTeach == null) return;

            // 1. Загружаем АБСОЛЮТНО ВСЕ метрики для этого учителя, игнорируя текущий год системы
            // Убедись, что в DatabaseService есть метод LoadAllSubjectQuarterMetricsForTeacherAsync
            var allMetrics = await _db.LoadAllSubjectQuarterMetricsForTeacherAsync(SelectedTeach.Id);

            // 2. Группируем полученные данные по AcademicYear
            // (например, отдельно "2023-2024", отдельно "2025–2025")
            var groupedByYear = allMetrics.GroupBy(m => m.AcademicYear);

            // 3. Сортируем года по убыванию (новые сверху) и создаем группы для UI
            foreach (var yearGroup in groupedByYear.OrderByDescending(g => g.Key))
            {
                var yearlyGroup = new YearlySubjectGroup { Year = yearGroup.Key };

                // Внутри года группируем по предметам
                var groupedBySubject = yearGroup.GroupBy(m => m.Subject);

                foreach (var subjectGroup in groupedBySubject)
                {
                    var sb = new SubjectBoard { SubjectName = subjectGroup.Key };

                    // Локальная функция для заполнения ячеек
                    void put(string q, string type, Func<SubjectQuarterMetric, string> pick)
                    {
                        var rec = subjectGroup.FirstOrDefault(r => r.Quarter == q);
                        if (rec == null) return;

                        // Находим нужную строку (Кач/Усп/СОУ) в UI
                        var row = sb.Metrics.FirstOrDefault(r => r.Type == type);
                        if (row != null)
                        {
                            // Через рефлексию пишем в свойства I2, II2...
                            row.GetType().GetProperty(q)?.SetValue(row, pick(rec));
                        }
                    }

                    // Проходим по четвертям/итогам
                    foreach (var q in new[] { "I2", "II2", "III2", "IV2", "Y" })
                    {
                        put(q, "кач", m => m.Kach);
                        put(q, "усп", m => m.Usp);
                        put(q, "СОУ", m => m.Sou);
                    }

                    yearlyGroup.SubjectBoards.Add(sb);
                }

                YearlyBoards.Add(yearlyGroup);
            }
        }

        private async Task SaveBoardsAsync()
        {
            if (SelectedTeach == null) return;

            // Проходим по каждой годовой группе, которая есть на экране
            foreach (var yearGroup in YearlyBoards)
            {
                var listToSave = new List<SubjectQuarterMetric>();

                // Собираем данные внутри этого года
                foreach (var sb in yearGroup.SubjectBoards)
                {
                    foreach (var q in new[] { "I2", "II2", "III2", "IV2", "Y" })
                    {
                        string kach = GetCell(sb, "кач", q);
                        string usp = GetCell(sb, "усп", q);
                        string sou = GetCell(sb, "СОУ", q);

                        // Если все пусто, не сохраняем пустую запись
                        if (string.IsNullOrWhiteSpace(kach) &&
                            string.IsNullOrWhiteSpace(usp) &&
                            string.IsNullOrWhiteSpace(sou))
                            continue;

                        listToSave.Add(new SubjectQuarterMetric
                        {
                            Id = Guid.NewGuid(),
                            TeachId = SelectedTeach.Id,
                            AcademicYear = yearGroup.Year, // <--- БЕРЕМ ГОД ИЗ ГРУППЫ
                            Subject = sb.SubjectName,
                            Quarter = q,
                            Kach = kach,
                            Usp = usp,
                            Sou = sou
                        });
                    }
                }

                // Сохраняем конкретно этот год. 
                // Метод SaveSubjectQuarterMetricsAsync удалит старые записи ЗА ЭТОТ ГОД и вставит новые.
                // Записи за другие года не пострадают.
                bool ok = await _db.SaveSubjectQuarterMetricsAsync(SelectedTeach.Id, yearGroup.Year, listToSave);

                if (ok) Log($"✔ Таблица 1.1 за {yearGroup.Year} сохранена");
            }

            // Перезагружаем, чтобы убедиться, что всё легло корректно
            await LoadBoardsAsync();
        }

        private static string GetCell(SubjectBoard sb, string rowType, string q)
        {
            var row = sb.Metrics.First(r => r.Type == rowType);
            return (row.GetType().GetProperty(q)!.GetValue(row) ?? "").ToString()!;
        }
        #endregion

        #region 8. Логика экспорта в Excel
        private async Task ExportMonitoringDataToExcel()
        {
            if (SelectedTeach == null) return;

            var dialog = new SaveFileDialog
            {
                Filter = "Excel Macro-Enabled Workbook (*.xlsm)|*.xlsm",
                FileName = $"{SelectedTeach.FullName}-Учитель.xlsm",
                Title = "Сохранить данные мониторинга"
            };

            if (dialog.ShowDialog() == true)
            {
                Log("Начинаю экспорт данных в Excel...");
                try
                {
                    string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExcelTemplate/template.xlsm");
                    if (!File.Exists(templatePath))
                    {
                        MessageBox.Show("Файл шаблона 'template.xlsm' не найден.", "Ошибка экспорта", MessageBoxButton.OK, MessageBoxImage.Error);
                        Log("Ошибка: файл шаблона не найден.");
                        return;
                    }

                    var exportService = new ExcelExportService();
                    await Task.Run(() => exportService.ExportMonitoringData(this, templatePath, dialog.FileName));

                    Log("✔ Экспорт успешно завершен.");
                    MessageBox.Show($"Данные успешно экспортированы в файл:\n{dialog.FileName}", "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    Log($"❌ Ошибка при экспорте: {ex.Message}");
                    MessageBox.Show($"Произошла ошибка при экспорте: {ex.Message}", "Ошибка экспорта", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task ImportDataFromExcel()
        {
            if (SelectedTeach == null) return;

            var dialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsm;*.xlsx",
                Title = "Импорт данных мониторинга"
            };

            if (dialog.ShowDialog() == true)
            {
                Log("Начинаю импорт данных из Excel...");
                try
                {
                    var importService = new ExcelImportService();

                    // Выполняем импорт в UI потоке или через Task.Run, но с осторожностью к ObservableCollection
                    // Т.к. ClosedXML грузит CPU, лучше в Task, но обновление коллекций должно быть в UI.
                    // В данном сервисе мы наполняем коллекции переданной VM. Если делать это из фона, нужен Dispatcher.
                    // Проще всего для начала сделать синхронно (или обернуть в Task с Dispatcher внутри сервиса, но сервис не должен знать о UI).

                    // Вариант: Чтение в структуру данных, потом применение к VM. 
                    // Но для простоты запустим в Task и используем Application.Current.Dispatcher внутри метода, если нужно, 
                    // НО ObservableCollection в WPF 4.5+ позволяет обновление из других потоков если включить BindingOperations.EnableCollectionSynchronization.
                    // Самый надежный способ без усложнений:

                    await Task.Run(() =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            // Запускаем импорт прямо внутри Invoke, чтобы безопасно менять коллекции
                            importService.ImportMonitoringData(dialog.FileName, this);
                        });
                    });

                    Log("✔ Импорт успешно завершен. Не забудьте нажать 'Сохранить'!");
                    MessageBox.Show("Данные загружены из файла.\n\nВНИМАНИЕ: Данные пока только на экране.\nНажмите 'Сохранить', чтобы записать их в базу данных.",
                        "Импорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    Log($"❌ Ошибка при импорте: {ex.Message}");
                    MessageBox.Show($"Произошла ошибка при импорте: {ex.Message}", "Ошибка импорта", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        #endregion

        #region 9. CRUD операции для Учителей
        private async Task ExecuteAddTeach()
        {
            var newTeach = await _db.AddTeachAsync("Новый учитель");
            if (newTeach != null)
            {
                TeachList.Add(newTeach);
                SelectedTeach = newTeach;
                Log("✔ Новый учитель добавлен. Вы можете изменить его имя.");
            }
            else
            {
                Log($"❌ Ошибка при добавлении учителя: {_db.LastError}");
                MessageBox.Show(_db.LastError, "Ошибка добавления", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExecuteDeleteTeach()
        {
            if (SelectedTeach == null) return;

            var result = MessageBox.Show($"Вы уверены, что хотите удалить учителя '{SelectedTeach.FullName}' и все данные его мониторинга?",
                                         "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            bool success = await _db.DeleteTeacherAsync(SelectedTeach.Id);
            if (success)
            {
                TeachList.Remove(SelectedTeach);
                Log("✔ Учитель и его данные удалены.");
            }
            else
            {
                Log($"❌ Ошибка при удалении учителя: {_db.LastError}");
                MessageBox.Show(_db.LastError, "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task UpdateTeachNameAsync(Guid teachId, string newName)
        {
            Log($"Сохранение нового имени для ID: {teachId}...");
            bool success = await _db.UpdateTeachNameAsync(teachId, newName);
            if (success)
            {
                Log("✔ Имя учителя успешно обновлено.");
                var teacherInList = TeachList.FirstOrDefault(t => t.Id == teachId);
                if (teacherInList != null)
                {
                    teacherInList.FullName = newName;
                }
            }
            else
            {
                Log($"❌ Ошибка при обновлении имени: {_db.LastError}");
                MessageBox.Show(_db.LastError, "Ошибка обновления", MessageBoxButton.OK, MessageBoxImage.Error);
                await LoadAllAsync();
            }
        }
        #endregion

        #region 10. Вспомогательные методы
        private void SetGuidProp(object target, string propName, Guid value) =>
            target.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance)
                          ?.SetValue(target, value);

        private void RegisterSection<TRow>(ObservableCollection<TRow> collection, Func<TRow> factory, string selectedPropertyName)
    where TRow : class, new()
        {
            string name = typeof(TRow).Name.Replace("Teacher", string.Empty);

            // --- ЛОГИКА ДОБАВЛЕНИЯ ---
            var addCmd = new RelayCommand(() =>
            {
                if (SelectedTeach == null) return;

                // 1. Создаем и заполняем строку
                var row = factory();
                SetGuidProp(row, "Id", Guid.NewGuid());
                SetGuidProp(row, "TeachId", SelectedTeach.Id);
                SetGuidProp(row, "TeacherId", SelectedTeach.Id); // На случай путаницы в моделях

                // 2. Добавляем в коллекцию
                collection.Add(row);

                // 3. !!! ГЛАВНОЕ ИСПРАВЛЕНИЕ: Автоматически выбираем добавленную строку !!!
                // Это активирует кнопку "Удалить" мгновенно.
                GetType().GetProperty(selectedPropertyName)?.SetValue(this, row);

                Log($"➕ {name} добавлен");
            });

            // Привязываем созданную команду к свойству ViewModel (например, AddGiaResultCommand)
            GetType().GetProperty($"Add{name}Command")!.SetValue(this, addCmd);

            // --- ЛОГИКА УДАЛЕНИЯ ---
            var selProp = GetType().GetProperty(selectedPropertyName)!;

            var delCmd = new RelayCommand(() =>
            {
                // Получаем текущий выбранный элемент через рефлексию
                var sel = (TRow?)selProp.GetValue(this);
                if (sel != null)
                {
                    collection.Remove(sel);
                    selProp.SetValue(this, null); // Сбрасываем выделение
                    Log($"🗑 {name} удалён");
                }
            }, () =>
            {
                // Условие активности кнопки: что-то должно быть выбрано
                return selProp.GetValue(this) != null;
            });

            GetType().GetProperty($"Delete{name}Command")!.SetValue(this, delCmd);

            // Подписываемся на изменение свойства Selected..., чтобы дергать кнопку Удалить
            PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == selectedPropertyName)
                {
                    // Как только сменилось выделение, проверяем, активна ли кнопка
                    delCmd.NotifyCanExecuteChanged();
                }
            };
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string dump = e.Exception.ToString();
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string full = Path.Combine(desktop, $"Ynost_UI_crash_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            try { File.WriteAllText(full, dump); }
            catch { /* ignore */ }
            MessageBox.Show(dump.Substring(0, Math.Min(500, dump.Length)), "UI-crash");
            e.Handled = true;
        }
        #endregion

        public class YearlySubjectGroup
        {
            public string Year { get; set; } = string.Empty;
            public ObservableCollection<SubjectBoard> SubjectBoards { get; } = new();
        }
    }
}   