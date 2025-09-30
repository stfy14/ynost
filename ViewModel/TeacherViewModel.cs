using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ynost.Models;

namespace Ynost.ViewModels
{
    public partial class TeacherViewModel : ObservableObject
    {
        private readonly Teacher _model;

        // Словарь для хранения всех трекеров изменений
        private readonly Dictionary<Type, IChangeset> _changesets = new();

        public TeacherViewModel(Teacher model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));

            // Инициализация коллекций и регистрация их для отслеживания
            AcademicResults = RegisterCollection(model.AcademicResults);
            IntermediateAssessments = RegisterCollection(model.IntermediateAssessments);
            GiaResults = RegisterCollection(model.GiaResults);
            DemoExamResults = RegisterCollection(model.DemoExamResults);
            IndependentAssessments = RegisterCollection(model.IndependentAssessments);
            SelfDeterminations = RegisterCollection(model.SelfDeterminations);
            StudentOlympiads = RegisterCollection(model.StudentOlympiads);
            JuryActivities = RegisterCollection(model.JuryActivities);
            MasterClasses = RegisterCollection(model.MasterClasses);
            Speeches = RegisterCollection(model.Speeches);
            Publications = RegisterCollection(model.Publications);
            ExperimentalProjects = RegisterCollection(model.ExperimentalProjects);
            Mentorships = RegisterCollection(model.Mentorships);
            ProgramSupports = RegisterCollection(model.ProgramSupports);
            ProfessionalCompetitions = RegisterCollection(model.ProfessionalCompetitions);
        }

        public Teacher Model => _model;
        public Guid Id => _model.Id;
        public string FullName { get => _model.FullName; set => SetProperty(_model.FullName, value, _model, (m, v) => m.FullName = v); }

        #region Generic Change Tracking Logic

        /// <summary>
        /// Вспомогательный интерфейс для работы с разнотипными наборами изменений.
        /// </summary>
        public interface IChangeset
        {
            IEnumerable<IChangeTrackable> GetAddedItems();
            IEnumerable<IChangeTrackable> GetModifiedItems();
            IEnumerable<Guid> GetDeletedItemIds();
            void Clear();
        }

        /// <summary>
        /// Класс, хранящий изменения для одного типа сущностей.
        /// </summary>
        private class Changeset<T> : IChangeset where T : class, IChangeTrackable, INotifyPropertyChanged
        {
            public readonly List<T> Added = new();
            public readonly List<T> Modified = new();
            public readonly List<Guid> DeletedIds = new();

            public IEnumerable<IChangeTrackable> GetAddedItems() => Added;
            public IEnumerable<IChangeTrackable> GetModifiedItems() => Modified;
            public IEnumerable<Guid> GetDeletedItemIds() => DeletedIds;

            public void Clear()
            {
                Added.Clear();
                Modified.Clear();
                DeletedIds.Clear();
            }
        }

        /// <summary>
        /// Получает набор изменений для указанного типа.
        /// </summary>
        public IChangeset? GetChangesetForType(Type t) => _changesets.GetValueOrDefault(t);

        /// <summary>
        /// Очищает все отслеженные изменения (вызывается после успешного сохранения).
        /// </summary>
        public void ClearAllChanges()
        {
            foreach (var changeset in _changesets.Values)
            {
                changeset.Clear();
            }
        }

        /// <summary>
        /// Создает ObservableCollection, настраивает отслеживание изменений и возвращает ее.
        /// </summary>
        private ObservableCollection<T> RegisterCollection<T>(List<T> initialItems) where T : class, IChangeTrackable, INotifyPropertyChanged
        {
            var changeset = new Changeset<T>();
            _changesets[typeof(T)] = changeset;

            var collection = new ObservableCollection<T>(initialItems);

            // Отслеживание изменений в уже существующих элементах
            foreach (var item in collection)
            {
                item.PropertyChanged += (s, e) => OnItemPropertyChanged(s as T, changeset);
            }

            collection.CollectionChanged += (s, e) => OnCollectionChanged(e, changeset);

            return collection;
        }

        private void OnItemPropertyChanged<T>(T? item, Changeset<T> changeset) where T : class, IChangeTrackable, INotifyPropertyChanged
        {
            if (item == null || changeset.Added.Contains(item) || changeset.Modified.Contains(item))
                return;

            changeset.Modified.Add(item);
            OnPropertyChanged(nameof(HasChanges)); // <-- ДОБАВЛЕНО: Уведомляем, что появились изменения
        }

        private void OnCollectionChanged<T>(NotifyCollectionChangedEventArgs e, Changeset<T> changeset) where T : class, IChangeTrackable, INotifyPropertyChanged
        {
            // Элемент добавлен в коллекцию UI
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (T newItem in e.NewItems)
                {
                    changeset.Added.Add(newItem);
                    newItem.PropertyChanged += (s, a) => OnItemPropertyChanged(s as T, changeset);
                }
            }
            // Элемент удален из коллекции UI
            else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                foreach (T oldItem in e.OldItems)
                {
                    oldItem.PropertyChanged -= (s, a) => OnItemPropertyChanged(s as T, changeset);

                    // Если он был только что добавлен и еще не сохранен, просто забываем о нем
                    if (changeset.Added.Contains(oldItem))
                    {
                        changeset.Added.Remove(oldItem);
                    }
                    // Иначе (если он был в БД), добавляем его ID в список на удаление
                    else
                    {
                        if (!changeset.DeletedIds.Contains(oldItem.Id))
                        {
                            changeset.DeletedIds.Add(oldItem.Id);
                        }
                        // Также убираем его из списка измененных, если он там был
                        changeset.Modified.Remove(oldItem);
                    }
                }
            }
            OnPropertyChanged(nameof(HasChanges));
        }

        #endregion

        #region Check and Fill 
        /// <summary>
        /// Снимает подсветку конфликтов со всех записей.
        /// </summary>
        public void ClearAllHighlights()
        {
            Action<object> clearFlag = (item) =>
            {
                if (item is IChangeTrackable trackable)
                {
                    var prop = item.GetType().GetProperty("IsConflicting");
                    if (prop != null && prop.CanWrite)
                    {
                        prop.SetValue(item, false);
                    }
                }
            };

            Model.AcademicResults.ForEach(clearFlag);
            Model.IntermediateAssessments.ForEach(clearFlag);
            Model.GiaResults.ForEach(clearFlag);
            Model.DemoExamResults.ForEach(clearFlag);
            Model.IndependentAssessments.ForEach(clearFlag);
            Model.SelfDeterminations.ForEach(clearFlag);
            Model.StudentOlympiads.ForEach(clearFlag);
            Model.JuryActivities.ForEach(clearFlag);
            Model.MasterClasses.ForEach(clearFlag);
            Model.Speeches.ForEach(clearFlag);
            Model.Publications.ForEach(clearFlag);
            Model.ExperimentalProjects.ForEach(clearFlag);
            Model.Mentorships.ForEach(clearFlag);
            Model.ProgramSupports.ForEach(clearFlag);
            Model.ProfessionalCompetitions.ForEach(clearFlag);
        }

        /// <summary>
        /// Сравнивает текущие данные в ViewModel со свежими данными из БД и подсвечивает различия.
        /// </summary>
        public void HighlightConflicts(Teacher freshData)
        {
            // Сначала все очищаем
            ClearAllHighlights();

            // Сравниваем дочерние коллекции
            CompareAndHighlightCollection(this.AcademicResults, freshData.AcademicResults);
            CompareAndHighlightCollection(this.IntermediateAssessments, freshData.IntermediateAssessments);
            CompareAndHighlightCollection(this.GiaResults, freshData.GiaResults);
            CompareAndHighlightCollection(this.DemoExamResults, freshData.DemoExamResults);
            CompareAndHighlightCollection(this.IndependentAssessments, freshData.IndependentAssessments);
            CompareAndHighlightCollection(this.SelfDeterminations, freshData.SelfDeterminations);
            CompareAndHighlightCollection(this.StudentOlympiads, freshData.StudentOlympiads);
            CompareAndHighlightCollection(this.JuryActivities, freshData.JuryActivities);
            CompareAndHighlightCollection(this.MasterClasses, freshData.MasterClasses);
            CompareAndHighlightCollection(this.Speeches, freshData.Speeches);
            CompareAndHighlightCollection(this.Publications, freshData.Publications);
            CompareAndHighlightCollection(this.ExperimentalProjects, freshData.ExperimentalProjects);
            CompareAndHighlightCollection(this.Mentorships, freshData.Mentorships);
            CompareAndHighlightCollection(this.ProgramSupports, freshData.ProgramSupports);
            CompareAndHighlightCollection(this.ProfessionalCompetitions, freshData.ProfessionalCompetitions);
        }

        private void CompareAndHighlightCollection<T>(ObservableCollection<T> staleCollection, List<T> freshCollection) where T : class, IChangeTrackable
        {
            var freshLookup = freshCollection.ToDictionary(i => i.Id);

            foreach (var staleItem in staleCollection)
            {
                // Случай 1: Запись была изменена или удалена другим пользователем
                if (freshLookup.TryGetValue(staleItem.Id, out var freshItem))
                {
                    // Если версии не совпадают, значит, запись меняли.
                    if (staleItem.Version != freshItem.Version)
                    {
                        staleItem.IsConflicting = true;
                    }
                }
                // Случай 2: Запись была удалена другим пользователем
                else
                {
                    staleItem.IsConflicting = true;
                }
            }
        }
        #endregion

        /// <summary>
        /// Возвращает true, если для этого преподавателя есть несохраненные изменения.
        /// </summary>
        public bool HasChanges
        {
            get
            {
                // Проверяем все наборы изменений. Если хотя бы в одном есть запись, возвращаем true.
                return _changesets.Values.Any(changeset =>
                    changeset.GetAddedItems().Any() ||
                    changeset.GetModifiedItems().Any() ||
                    changeset.GetDeletedItemIds().Any());
            }
        }


        // ─────────────────────────────────────────────────────────────────────────
        #region 1. AcademicYearResult
        public ObservableCollection<AcademicYearResult> AcademicResults { get; }
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteAcademicResultCommand))] 
        private AcademicYearResult? _selectedAcademicResult;
        [RelayCommand] 
        private void AddAcademicResult() 
        { 
            AcademicResults.Add(new AcademicYearResult 
            { 
                TeacherId = _model.Id, 
                AcademicPeriod = $"{DateTime.Now.Year}-{DateTime.Now.Year + 1}" 
            }); 
        }
        [RelayCommand(CanExecute = nameof(CanDeleteAcademicResult))] 
        private void DeleteAcademicResult() 
        { 
            if (SelectedAcademicResult != null) 
                AcademicResults.Remove(SelectedAcademicResult); 
        }
        private bool CanDeleteAcademicResult() => SelectedAcademicResult != null;
        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region 1A. IntermediateAssessment

        public ObservableCollection<IntermediateAssessment> IntermediateAssessments { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteIntermediateAssessmentCommand))]
        private IntermediateAssessment? _selectedIntermediateAssessment;

        [RelayCommand]
        private void AddIntermediateAssessment()
        {
            IntermediateAssessments.Add(new IntermediateAssessment
            {
                TeacherId = _model.Id,
                AcademicYear = $"{DateTime.Now.Year}-{DateTime.Now.Year + 1}"
            });
        }

        [RelayCommand(CanExecute = nameof(CanDeleteIntermediateAssessment))]
        private void DeleteIntermediateAssessment()
        {
            if (SelectedIntermediateAssessment != null)
                IntermediateAssessments.Remove(SelectedIntermediateAssessment);
        }

        private bool CanDeleteIntermediateAssessment()
            => SelectedIntermediateAssessment != null;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region 2. GiaResult

        public ObservableCollection<GiaResult> GiaResults { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteGiaResultCommand))]
        private GiaResult? _selectedGiaResult;

        [RelayCommand]
        private void AddGiaResult()
        {
            GiaResults.Add(new GiaResult
            {
                TeacherId = _model.Id
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
        #region 3. DemoExamResult

        public ObservableCollection<DemoExamResult> DemoExamResults { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteDemoExamResultCommand))]
        private DemoExamResult? _selectedDemoExamResult;

        [RelayCommand]
        private void AddDemoExamResult()
        {
            DemoExamResults.Add(new DemoExamResult
            {
                TeacherId = _model.Id
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
        #region 4. IndependentAssessment

        public ObservableCollection<IndependentAssessment> IndependentAssessments { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteIndependentAssessmentCommand))]
        private IndependentAssessment? _selectedIndependentAssessment;

        [RelayCommand]
        private void AddIndependentAssessment()
        {
            IndependentAssessments.Add(new IndependentAssessment
            {
                TeacherId = _model.Id
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
        #region 5. SelfDeterminationActivity

        public ObservableCollection<SelfDeterminationActivity> SelfDeterminations { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteSelfDeterminationCommand))]
        private SelfDeterminationActivity? _selectedSelfDetermination;

        [RelayCommand]
        private void AddSelfDetermination()
        {
            SelfDeterminations.Add(new SelfDeterminationActivity
            {
                TeacherId = _model.Id
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
        #region 6. StudentOlympiad

        public ObservableCollection<StudentOlympiad> StudentOlympiads { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteStudentOlympiadCommand))]
        private StudentOlympiad? _selectedStudentOlympiad;

        [RelayCommand]
        private void AddStudentOlympiad()
        {
            StudentOlympiads.Add(new StudentOlympiad
            {
                TeacherId = _model.Id
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
        #region 7. JuryActivity

        public ObservableCollection<JuryActivity> JuryActivities { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteJuryActivityCommand))]
        private JuryActivity? _selectedJuryActivity;

        [RelayCommand]
        private void AddJuryActivity()
        {
            JuryActivities.Add(new JuryActivity
            {
                TeacherId = _model.Id
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
        #region 8. MasterClass

        public ObservableCollection<MasterClass> MasterClasses { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteMasterClassCommand))]
        private MasterClass? _selectedMasterClass;

        [RelayCommand]
        private void AddMasterClass()
        {
            MasterClasses.Add(new MasterClass
            {
                TeacherId = _model.Id
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
        #region 9. Speech

        public ObservableCollection<Speech> Speeches { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteSpeechCommand))]
        private Speech? _selectedSpeech;

        [RelayCommand]
        private void AddSpeech()
        {
            Speeches.Add(new Speech
            {
                TeacherId = _model.Id
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
        #region 10. Publication

        public ObservableCollection<Publication> Publications { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeletePublicationCommand))]
        private Publication? _selectedPublication;

        [RelayCommand]
        private void AddPublication()
        {
            Publications.Add(new Publication
            {
                TeacherId = _model.Id
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
        #region 11. ExperimentalProject

        public ObservableCollection<ExperimentalProject> ExperimentalProjects { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteExperimentalProjectCommand))]
        private ExperimentalProject? _selectedExperimentalProject;

        [RelayCommand]
        private void AddExperimentalProject()
        {
            ExperimentalProjects.Add(new ExperimentalProject
            {
                TeacherId = _model.Id
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
        #region 12. Mentorship

        public ObservableCollection<Mentorship> Mentorships { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteMentorshipCommand))]
        private Mentorship? _selectedMentorship;

        [RelayCommand]
        private void AddMentorship()
        {
            Mentorships.Add(new Mentorship
            {
                TeacherId = _model.Id
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
        #region 13. ProgramMethodSupport

        public ObservableCollection<ProgramMethodSupport> ProgramSupports { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteProgramSupportCommand))]
        private ProgramMethodSupport? _selectedProgramSupport;

        [RelayCommand]
        private void AddProgramSupport()
        {
            ProgramSupports.Add(new ProgramMethodSupport
            {
                TeacherId = _model.Id
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
        #region 14. ProfessionalCompetition

        public ObservableCollection<ProfessionalCompetition> ProfessionalCompetitions { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteProfessionalCompetitionCommand))]
        private ProfessionalCompetition? _selectedProfessionalCompetition;

        [RelayCommand]
        private void AddProfessionalCompetition()
        {
            ProfessionalCompetitions.Add(new ProfessionalCompetition
            {
                TeacherId = _model.Id
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

        // 5) SyncToModel — копирует всё из ViewModel обратно в _model перед Save:
        public void SyncToModel()
        {
            // 1) Полностью очищаем текущие коллекции в доменной модели:
            _model.AcademicResults.Clear();
            _model.IntermediateAssessments.Clear(); // ← ДОБАВЛЕНО
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
            foreach (var i in IntermediateAssessments) _model.IntermediateAssessments.Add(i); // ← ДОБАВЛЕНО
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