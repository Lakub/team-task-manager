using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Input;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Models.Enums;
using TeamTaskManager.Services;

namespace TeamTaskManager.ViewModels
{
    public partial class CreateTaskViewModel : ObservableValidator
    {
        private readonly ITaskService _taskService;
        private readonly IProjectService _projectService;
        private readonly int _projectId;

        private string _title = string.Empty;
        [Required(ErrorMessage = "Tytuł jest wymagany.")]
        public string Title
        {
            get => _title;
            set { SetProperty(ref _title, value, true); OnPropertyChanged(nameof(IsValid)); }
        }

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private TaskType _selectedType = TaskType.Feature;

        [ObservableProperty]
        private TaskPriority _selectedPriority = TaskPriority.Medium;

        [ObservableProperty]
        private User? _selectedAssignee;

        public ObservableCollection<User> ProjectMembers { get; } = new();
        public ObservableCollection<TaskType> TaskTypes { get; } = new(Enum.GetValues(typeof(TaskType)).Cast<TaskType>());
        public ObservableCollection<TaskPriority> Priorities { get; } = new(Enum.GetValues(typeof(TaskPriority)).Cast<TaskPriority>());

        public bool IsValid => !string.IsNullOrWhiteSpace(Title) && !HasErrors;

        [ObservableProperty]
        private bool _isBusy;

        public ICommand CreateCommand { get; }
        public ICommand CancelCommand { get; }

        public Action? OnSuccess { get; set; }
        public Action? OnCancel { get; set; }


        public CreateTaskViewModel(ITaskService taskService, IProjectService projectService, int projectId)
        {
            _taskService = taskService;
            _projectService = projectService;
            _projectId = projectId;

            CreateCommand = new AsyncRelayCommand(ExecuteCreateAsync);
            CancelCommand = new RelayCommand(() => OnCancel?.Invoke());
        }

        public async System.Threading.Tasks.Task InitializeAsync()
        {
            var members = await _projectService.GetMembersByProjectIdAsync(_projectId);
            ProjectMembers.Clear();
            ProjectMembers.Add(new User { Id = -1, FullName = "Nieprzypisane" });
            foreach (var m in members)
                ProjectMembers.Add(m);

            SelectedAssignee = ProjectMembers.FirstOrDefault(m => m.Id == -1);
        }

        private async System.Threading.Tasks.Task ExecuteCreateAsync()
        {
            if (IsBusy) return;

            if (!IsValid)
            {
                MessageBox.Show("Nieprawidłowe dane.", "Nieprawidłowe dane", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsBusy = true;
            try
            {
                var reporterId = App.CurrentUser?.Id ?? throw new InvalidOperationException("Brak zalogowanego użytkownika.");
                var assigneeId = SelectedAssignee?.Id == -1 ? null : SelectedAssignee?.Id;

                await _taskService.CreateTaskAsync(
                    title: Title.Trim(),
                    description: Description.Trim(),
                    type: SelectedType,
                    priority: SelectedPriority,
                    projectId: _projectId,
                    reporterId: reporterId,
                    assigneeId: assigneeId);

                MessageBox.Show("Zadanie zostało pomyślnie utworzone.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                OnSuccess?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas tworzenia zadania: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}