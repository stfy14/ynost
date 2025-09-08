using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ynost.Models;

namespace Ynost.ViewModels
{
    public partial class TeacherViewModel : ObservableObject
    {
        // ─────────────────────────────────────────────────────────────────────────
        // 1) Храним ссылку на доменную модель Teacher:
        private readonly Teacher _model;

        public TeacherViewModel(Teacher model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));

            // Инициализируем коллекции дочерних сущностей из _model:
            AcademicResults = new ObservableCollection<AcademicYearResult>(_model.AcademicResults);
            GiaResults = new ObservableCollection<GiaResult>(_model.GiaResults);
            DemoExamResults = new ObservableCollection<DemoExamResult>(_model.DemoExamResults);
            IndependentAssessments = new ObservableCollection<IndependentAssessment>(_model.IndependentAssessments);
            SelfDeterminations = new ObservableCollection<SelfDeterminationActivity>(_model.SelfDeterminations);
            StudentOlympiads = new ObservableCollection<StudentOlympiad>(_model.StudentOlympiads);
            JuryActivities = new ObservableCollection<JuryActivity>(_model.JuryActivities);
            MasterClasses = new ObservableCollection<MasterClass>(_model.MasterClasses);
            Speeches = new ObservableCollection<Speech>(_model.Speeches);
            Publications = new ObservableCollection<Publication>(_model.Publications);
            ExperimentalProjects = new ObservableCollection<ExperimentalProject>(_model.ExperimentalProjects);
            Mentorships = new ObservableCollection<Mentorship>(_model.Mentorships);
            ProgramSupports = new ObservableCollection<ProgramMethodSupport>(_model.ProgramSupports);
            ProfessionalCompetitions = new ObservableCollection<ProfessionalCompetition>(_model.ProfessionalCompetitions);
        }

        // ─────────────────────────────────────────────────────────────────────────
        // 2) Публичное свойство, чтобы MainViewModel мог передать именно оригинальный Teacher в SaveAllAsync:
        public Teacher Model => _model;

        // 3) Несколько «одноуровневых» свойств для отображения в UI:
        public Guid Id => _model.Id;
        public string FullName => _model.FullName;
        // (Если хотите, можете сделать сеттер для FullName и редактировать его из UI,
        // но тогда нужно будет вызывать OnPropertyChanged(nameof(FullName)).)

        // ─────────────────────────────────────────────────────────────────────────
        #region 1. Итоговые результаты успеваемости (AcademicYearResult)

        public ObservableCollection<AcademicYearResult> AcademicResults { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteAcademicResultCommand))]
        private AcademicYearResult? _selectedAcademicResult;

        [RelayCommand]
        private void AddAcademicResult()
        {
            // При добавлении новой записи обязательно заполняем TeacherId:
            string y1 = DateTime.Now.Year.ToString();
            string y2 = (DateTime.Now.Year + 1).ToString();

            AcademicResults.Add(new AcademicYearResult
            {
                Id = Guid.NewGuid(),
                TeacherId = _model.Id,
                Group = "Группа",
                AcademicPeriod = $"{y1}-{y2}",
                Subject = "Новый предмет",
                AvgSem1 = string.Empty,
                AvgSem2 = string.Empty,
                DynamicsSem = string.Empty,
                AvgSuccessRate = string.Empty,
                DynamicsAvgSuccessRate = string.Empty,
                AvgQualityRate = string.Empty,
                DynamicsAvgQualityRate = string.Empty,
                EntrySouRate = string.Empty,
                ExitSouRate= string.Empty,
                Link = string.Empty
            });
        }

        [RelayCommand(CanExecute = nameof(CanDeleteAcademicResult))]
        private void DeleteAcademicResult()
        {
            if (SelectedAcademicResult != null)
                AcademicResults.Remove(SelectedAcademicResult);
        }

        private bool CanDeleteAcademicResult()
            => SelectedAcademicResult != null;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region 2. Результаты ГИА (GiaResult)

        public ObservableCollection<GiaResult> GiaResults { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteGiaResultCommand))]
        private GiaResult? _selectedGiaResult;

        [RelayCommand]
        private void AddGiaResult()
        {
            GiaResults.Add(new GiaResult
            {
                Id = Guid.NewGuid(),
                TeacherId = _model.Id,
                Subject = "Предмет ГИА",
                Group = "Группа",
                TotalParticipants = "0",
                Count5 = string.Empty,
                Count4 = string.Empty,
                Count3 = string.Empty,
                Count2 = string.Empty,
                AvgScore = string.Empty,
                Link = string.Empty
            });
        }

        [RelayCommand(CanExecute = nameof(CanDeleteGiaResult))]
        private void DeleteGiaResult()
        {
            if (SelectedGiaResult != null)
                GiaResults.Remove(SelectedGiaResult);
        }

        private bool CanDeleteGiaResult()
            => SelectedGiaResult != null;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region 3. Результаты демонстрационного экзамена (DemoExamResult)

        public ObservableCollection<DemoExamResult> DemoExamResults { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteDemoExamResultCommand))]
        private DemoExamResult? _selectedDemoExamResult;

        [RelayCommand]
        private void AddDemoExamResult()
        {
            DemoExamResults.Add(new DemoExamResult
            {
                Id = Guid.NewGuid(),
                TeacherId = _model.Id,
                Subject = "Компетенция ДЭ",
                Group = "Группа",
                TotalParticipants = "0",
                Count5 = string.Empty,
                Count4 = string.Empty,
                Count3 = string.Empty,
                Count2 = string.Empty,
                AvgScore = string.Empty,
                Link = string.Empty
            });
        }

        [RelayCommand(CanExecute = nameof(CanDeleteDemoExamResult))]
        private void DeleteDemoExamResult()
        {
            if (SelectedDemoExamResult != null)
                DemoExamResults.Remove(SelectedDemoExamResult);
        }

        private bool CanDeleteDemoExamResult()
            => SelectedDemoExamResult != null;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region 4. Независимая оценка (IndependentAssessment)

        public ObservableCollection<IndependentAssessment> IndependentAssessments { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteIndependentAssessmentCommand))]
        private IndependentAssessment? _selectedIndependentAssessment;

        [RelayCommand]
        private void AddIndependentAssessment()
        {
            IndependentAssessments.Add(new IndependentAssessment
            {
                Id = Guid.NewGuid(),
                TeacherId = _model.Id,
                AssessmentName = "Вид оценки",
                AssessmentDate = DateTime.Now.ToString("dd.MM.yyyy"),
                ClassSubject = "Класс/Предмет",
                StudentsTotal = string.Empty,
                StudentsParticipated = string.Empty,
                StudentsPassed = string.Empty,
                Count5 = string.Empty,
                Count4 = string.Empty,
                Count3 = string.Empty,
                Link = string.Empty
            });
        }

        [RelayCommand(CanExecute = nameof(CanDeleteIndependentAssessment))]
        private void DeleteIndependentAssessment()
        {
            if (SelectedIndependentAssessment != null)
                IndependentAssessments.Remove(SelectedIndependentAssessment);
        }

        private bool CanDeleteIndependentAssessment()
            => SelectedIndependentAssessment != null;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region 5. Деятельность по самоопределению (SelfDeterminationActivity)

        public ObservableCollection<SelfDeterminationActivity> SelfDeterminations { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteSelfDeterminationCommand))]
        private SelfDeterminationActivity? _selectedSelfDetermination;

        [RelayCommand]
        private void AddSelfDetermination()
        {
            SelfDeterminations.Add(new SelfDeterminationActivity
            {
                Id = Guid.NewGuid(),
                TeacherId = _model.Id,
                Level = "Уровень",
                Name = "Мероприятие",
                Role = "Роль",
                Link = string.Empty
            });
        }

        [RelayCommand(CanExecute = nameof(CanDeleteSelfDetermination))]
        private void DeleteSelfDetermination()
        {
            if (SelectedSelfDetermination != null)
                SelfDeterminations.Remove(SelectedSelfDetermination);
        }

        private bool CanDeleteSelfDetermination()
            => SelectedSelfDetermination != null;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region 6. Олимпиады студентов (StudentOlympiad)

        public ObservableCollection<StudentOlympiad> StudentOlympiads { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteStudentOlympiadCommand))]
        private StudentOlympiad? _selectedStudentOlympiad;

        [RelayCommand]
        private void AddStudentOlympiad()
        {
            StudentOlympiads.Add(new StudentOlympiad
            {
                Id = Guid.NewGuid(),
                TeacherId = _model.Id,
                Level = "Уровень",
                Name = "Название",
                Form = "Форма",
                Cadet = "Ученик",
                Result = "Результат",
                Link = string.Empty
            });
        }

        [RelayCommand(CanExecute = nameof(CanDeleteStudentOlympiad))]
        private void DeleteStudentOlympiad()
        {
            if (SelectedStudentOlympiad != null)
                StudentOlympiads.Remove(SelectedStudentOlympiad);
        }

        private bool CanDeleteStudentOlympiad()
            => SelectedStudentOlympiad != null;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region 7. Работа в жюри (JuryActivity)

        public ObservableCollection<JuryActivity> JuryActivities { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteJuryActivityCommand))]
        private JuryActivity? _selectedJuryActivity;

        [RelayCommand]
        private void AddJuryActivity()
        {
            JuryActivities.Add(new JuryActivity
            {
                Id = Guid.NewGuid(),
                TeacherId = _model.Id,
                Level = "Уровень",
                Name = "Событие",
                EventDate = DateTime.Now.ToString("dd.MM.yyyy"),
                Link = string.Empty
            });
        }

        [RelayCommand(CanExecute = nameof(CanDeleteJuryActivity))]
        private void DeleteJuryActivity()
        {
            if (SelectedJuryActivity != null)
                JuryActivities.Remove(SelectedJuryActivity);
        }

        private bool CanDeleteJuryActivity()
            => SelectedJuryActivity != null;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region 8. Мастер-классы (MasterClass)

        public ObservableCollection<MasterClass> MasterClasses { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteMasterClassCommand))]
        private MasterClass? _selectedMasterClass;

        [RelayCommand]
        private void AddMasterClass()
        {
            MasterClasses.Add(new MasterClass
            {
                Id = Guid.NewGuid(),
                TeacherId = _model.Id,
                Level = "Уровень",
                Name = "Тема",
                EventDate = DateTime.Now.ToString("dd.MM.yyyy"),
                Link = string.Empty
            });
        }

        [RelayCommand(CanExecute = nameof(CanDeleteMasterClass))]
        private void DeleteMasterClass()
        {
            if (SelectedMasterClass != null)
                MasterClasses.Remove(SelectedMasterClass);
        }

        private bool CanDeleteMasterClass()
            => SelectedMasterClass != null;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region 9. Выступления (Speech)

        public ObservableCollection<Speech> Speeches { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteSpeechCommand))]
        private Speech? _selectedSpeech;

        [RelayCommand]
        private void AddSpeech()
        {
            Speeches.Add(new Speech
            {
                Id = Guid.NewGuid(),
                TeacherId = _model.Id,
                Level = "Уровень",
                Name = "Тема",
                EventDate = DateTime.Now.ToString("dd.MM.yyyy"),
                Link = string.Empty
            });
        }

        [RelayCommand(CanExecute = nameof(CanDeleteSpeech))]
        private void DeleteSpeech()
        {
            if (SelectedSpeech != null)
                Speeches.Remove(SelectedSpeech);
        }

        private bool CanDeleteSpeech()
            => SelectedSpeech != null;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region 10. Публикации (Publication)

        public ObservableCollection<Publication> Publications { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeletePublicationCommand))]
        private Publication? _selectedPublication;

        [RelayCommand]
        private void AddPublication()
        {
            Publications.Add(new Publication
            {
                Id = Guid.NewGuid(),
                TeacherId = _model.Id,
                Level = "Уровень",
                Title = "Название",
                Date = DateTime.Now.ToString("dd.MM.yyyy"), // ← раньше PubDate, теперь Date
                Link = string.Empty
            });
        }

        [RelayCommand(CanExecute = nameof(CanDeletePublication))]
        private void DeletePublication()
        {
            if (SelectedPublication != null)
                Publications.Remove(SelectedPublication);
        }

        private bool CanDeletePublication()
            => SelectedPublication != null;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region 11. Экспериментальные проекты (ExperimentalProject)

        public ObservableCollection<ExperimentalProject> ExperimentalProjects { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteExperimentalProjectCommand))]
        private ExperimentalProject? _selectedExperimentalProject;

        [RelayCommand]
        private void AddExperimentalProject()
        {
            ExperimentalProjects.Add(new ExperimentalProject
            {
                Id = Guid.NewGuid(),
                TeacherId = _model.Id,
                Name = "Название проекта",
                Date = DateTime.Now.ToString("dd.MM.yyyy"), // ← раньше ProjDate, теперь Date
                Link = string.Empty
            });
        }

        [RelayCommand(CanExecute = nameof(CanDeleteExperimentalProject))]
        private void DeleteExperimentalProject()
        {
            if (SelectedExperimentalProject != null)
                ExperimentalProjects.Remove(SelectedExperimentalProject);
        }

        private bool CanDeleteExperimentalProject()
            => SelectedExperimentalProject != null;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region 12. Наставничество (Mentorship)

        public ObservableCollection<Mentorship> Mentorships { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteMentorshipCommand))]
        private Mentorship? _selectedMentorship;

        [RelayCommand]
        private void AddMentorship()
        {
            Mentorships.Add(new Mentorship
            {
                Id = Guid.NewGuid(),
                TeacherId = _model.Id,
                Trainee = "Наставляемый",
                OrderNo = "Приказ",                                 // ← раньше Order, теперь OrderNo
                OrderDate = DateTime.Now.ToString("dd.MM.yyyy"),
                Link = string.Empty
            });
        }

        [RelayCommand(CanExecute = nameof(CanDeleteMentorship))]
        private void DeleteMentorship()
        {
            if (SelectedMentorship != null)
                Mentorships.Remove(SelectedMentorship);
        }

        private bool CanDeleteMentorship()
            => SelectedMentorship != null;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region 13. Программно-методическое сопровождение (ProgramMethodSupport)

        public ObservableCollection<ProgramMethodSupport> ProgramSupports { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteProgramSupportCommand))]
        private ProgramMethodSupport? _selectedProgramSupport;

        [RelayCommand]
        private void AddProgramSupport()
        {
            ProgramSupports.Add(new ProgramMethodSupport
            {
                Id = Guid.NewGuid(),
                TeacherId = _model.Id,
                ProgramName = "Название программы",
                HasControlMaterials = false,
                Link = string.Empty
            });
        }

        [RelayCommand(CanExecute = nameof(CanDeleteProgramSupport))]
        private void DeleteProgramSupport()
        {
            if (SelectedProgramSupport != null)
                ProgramSupports.Remove(SelectedProgramSupport);
        }

        private bool CanDeleteProgramSupport()
            => SelectedProgramSupport != null;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region 14. Профессиональные конкурсы (ProfessionalCompetition)

        public ObservableCollection<ProfessionalCompetition> ProfessionalCompetitions { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteProfessionalCompetitionCommand))]
        private ProfessionalCompetition? _selectedProfessionalCompetition;

        [RelayCommand]
        private void AddProfessionalCompetition()
        {
            ProfessionalCompetitions.Add(new ProfessionalCompetition
            {
                Id = Guid.NewGuid(),
                TeacherId = _model.Id,
                Level = "Уровень",
                Name = "Конкурс",
                Achievement = "Достижение",
                EventDate = DateTime.Now.ToString("dd.MM.yyyy"),
                Link = string.Empty
            });
        }

        [RelayCommand(CanExecute = nameof(CanDeleteProfessionalCompetition))]
        private void DeleteProfessionalCompetition()
        {
            if (SelectedProfessionalCompetition != null)
                ProfessionalCompetitions.Remove(SelectedProfessionalCompetition);
        }

        private bool CanDeleteProfessionalCompetition()
            => SelectedProfessionalCompetition != null;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        // 5) SyncToModel — копирует всё из ViewModel обратно в _model перед Save:
        public void SyncToModel()
        {
            // 1) Полностью очищаем текущие коллекции в доменной модели:
            _model.AcademicResults.Clear();
            _model.GiaResults.Clear();
            _model.DemoExamResults.Clear();
            _model.IndependentAssessments.Clear();
            _model.SelfDeterminations.Clear();
            _model.StudentOlympiads.Clear();
            _model.JuryActivities.Clear();
            _model.MasterClasses.Clear();
            _model.Speeches.Clear();
            _model.Publications.Clear();
            _model.ExperimentalProjects.Clear();
            _model.Mentorships.Clear();
            _model.ProgramSupports.Clear();
            _model.ProfessionalCompetitions.Clear();

            // 2) Переносим записи из ObservableCollection обратно в доменную модель:
            foreach (var a in AcademicResults) _model.AcademicResults.Add(a);
            foreach (var g in GiaResults) _model.GiaResults.Add(g);
            foreach (var d in DemoExamResults) _model.DemoExamResults.Add(d);
            foreach (var i in IndependentAssessments) _model.IndependentAssessments.Add(i);
            foreach (var s in SelfDeterminations) _model.SelfDeterminations.Add(s);
            foreach (var o in StudentOlympiads) _model.StudentOlympiads.Add(o);
            foreach (var j in JuryActivities) _model.JuryActivities.Add(j);
            foreach (var m in MasterClasses) _model.MasterClasses.Add(m);
            foreach (var p in Speeches) _model.Speeches.Add(p);
            foreach (var u in Publications) _model.Publications.Add(u);
            foreach (var e in ExperimentalProjects) _model.ExperimentalProjects.Add(e);
            foreach (var me in Mentorships) _model.Mentorships.Add(me);
            foreach (var ps in ProgramSupports) _model.ProgramSupports.Add(ps);
            foreach (var pc in ProfessionalCompetitions) _model.ProfessionalCompetitions.Add(pc);
        }
    }
}
