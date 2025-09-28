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
        #endregion

        #region 1.1 Итоги по предметам (Boards)
        public ObservableCollection<SubjectBoard> Boards { get; } = new();
        [ObservableProperty] private SubjectBoard? _selectedBoard;
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

            AddBoardCommand = new RelayCommand(() => Boards.Add(new SubjectBoard()));
            DeleteBoardCommand = new RelayCommand(() =>
            {
                if (SelectedBoard != null) Boards.Remove(SelectedBoard);
                SelectedBoard = Boards.FirstOrDefault();
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
            Boards.Clear();
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
            Boards.Clear();
            if (SelectedTeach == null) return;

            var raw = await _db.LoadSubjectQuarterMetricsAsync(SelectedTeach.Id, CurrentYear);

            foreach (var grp in raw.GroupBy(r => r.Subject))
            {
                var sb = new SubjectBoard { SubjectName = grp.Key };

                void put(string q, string type, Func<SubjectQuarterMetric, string> pick)
                {
                    var rec = grp.FirstOrDefault(r => r.Quarter == q);
                    if (rec == null) return;
                    var row = sb.Metrics.First(r => r.Type == type);
                    row.GetType().GetProperty(q)!.SetValue(row, pick(rec));
                }

                foreach (var q in new[] { "I2", "II2", "III2", "IV2", "Y" })
                {
                    put(q, "кач", m => m.Kach);
                    put(q, "усп", m => m.Usp);
                    put(q, "СОУ", m => m.Sou);
                }
                Boards.Add(sb);
            }
        }

        private async Task SaveBoardsAsync()
        {
            if (SelectedTeach == null) return;

            var list = new List<SubjectQuarterMetric>();

            foreach (var sb in Boards)
            {
                foreach (var q in new[] { "I2", "II2", "III2", "IV2", "Y" })
                {
                    string kach = GetCell(sb, "кач", q);
                    string usp = GetCell(sb, "усп", q);
                    string sou = GetCell(sb, "СОУ", q);

                    if (string.IsNullOrWhiteSpace(kach) &&
                        string.IsNullOrWhiteSpace(usp) &&
                        string.IsNullOrWhiteSpace(sou))
                        continue;

                    list.Add(new SubjectQuarterMetric
                    {
                        Id = Guid.NewGuid(),
                        TeachId = SelectedTeach.Id,
                        AcademicYear = CurrentYear,
                        Subject = sb.SubjectName,
                        Quarter = q,
                        Kach = kach,
                        Usp = usp,
                        Sou = sou
                    });
                }
            }

            bool ok = await _db.SaveSubjectQuarterMetricsAsync(SelectedTeach.Id, CurrentYear, list);
            if (ok) Log($"✔ 1.1 сохранён ({list.Count} строк)");
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

            bool success = await _db.DeleteTeachAsync(SelectedTeach.Id);
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

            var addCmd = new RelayCommand(() =>
            {
                if (SelectedTeach == null) return;
                var row = factory();
                SetGuidProp(row, "Id", Guid.NewGuid());
                SetGuidProp(row, "TeachId", SelectedTeach.Id);
                SetGuidProp(row, "TeacherId", SelectedTeach.Id);
                collection.Add(row);
                Log($"➕ {name} добавлен");
            });
            GetType().GetProperty($"Add{name}Command")!.SetValue(this, addCmd);

            var selProp = GetType().GetProperty(selectedPropertyName)!;
            var delCmd = new RelayCommand(() =>
            {
                var sel = (TRow?)selProp.GetValue(this);
                if (sel != null)
                {
                    collection.Remove(sel);
                    selProp.SetValue(this, null);
                    Log($"🗑 {name} удалён");
                }
            }, () => selProp.GetValue(this) != null);
            GetType().GetProperty($"Delete{name}Command")!.SetValue(this, delCmd);

            PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == selectedPropertyName)
                    delCmd.NotifyCanExecuteChanged();
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
    }
}   