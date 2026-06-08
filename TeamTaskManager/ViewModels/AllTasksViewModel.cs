using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using TeamTaskManager.Helpers;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Models.Enums;
using TeamTaskManager.Services;
using TeamTaskManager.Views;
using TaskStatus = TeamTaskManager.Models.Enums.TaskStatus;

namespace TeamTaskManager.ViewModels
{
    public partial class AllTasksViewModel : INotifyPropertyChanged
    {
        private readonly IAllTasksService _allTasksService;
        private readonly IBacklogService _backlogService;
        private readonly int _projectId;

        public ICommand CreateTaskCommand { get; }
        public ICommand SelectTaskCommand { get; }
        public ICommand ToggleSortCommand { get; }
        public ICommand OpenTaskInWindowCommand { get; }

        public bool CanManageProject { get; private set; }

        private string _selectedFilter = "All";
        public string SelectedFilter
        {
            get => _selectedFilter;
            set { _selectedFilter = value; OnPropertyChanged(); ApplyFilterSort(); }
        }

        private bool _sortAscending = true;
        public bool SortAscending
        {
            get => _sortAscending;
            set { _sortAscending = value; OnPropertyChanged(); OnPropertyChanged(nameof(SortArrow)); ApplyFilterSort(); }
        }

        public string SortArrow => _sortAscending ? "↑" : "↓";

        public ObservableCollection<FilterOption> FilterOptions { get; } = new()
        {
            new() { Label = "Wszystkie", Value = "All" },
            new() { Label = "Otwarte", Value = "Open" },
            new() { Label = "W trakcie", Value = "InProgress" },
            new() { Label = "Zamknięte", Value = "Closed" },
            new() { Label = "Błędy", Value = "Bug" },
            new() { Label = "Funkcje", Value = "Feature" },
            new() { Label = "Zadania", Value = "Task" },
            new() { Label = "Wysoki priorytet", Value = "High" },
            new() { Label = "Średni priorytet", Value = "Medium" },
            new() { Label = "Niski priorytet", Value = "Low" },
        };

        private List<BacklogTaskItem> _allTasks = new();
        public ObservableCollection<BacklogTaskItem> FilteredTasks { get; } = new();

        private BacklogTaskItem? _selectedTask;
        public BacklogTaskItem? SelectedTask
        {
            get => _selectedTask;
            set
            {
                _selectedTask = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedTask));
                OnPropertyChanged(nameof(SelectedTaskId));
                SelectedTaskChanged?.Invoke(_selectedTask);
            }
        }

        public bool HasSelectedTask => _selectedTask != null;
        public int? SelectedTaskId => _selectedTask?.TaskId;

        public event Action<BacklogTaskItem?>? SelectedTaskChanged;

        private string _projectKey = "";

        public AllTasksViewModel(IAllTasksService allTasksService, IBacklogService backlogService, int projectId)
        {
            _allTasksService = allTasksService;
            _backlogService = backlogService;
            _projectId = projectId;

            CreateTaskCommand = new RelayCommand(CreateTask);
            SelectTaskCommand = new RelayCommand<BacklogTaskItem?>(item => SelectedTask = item);
            ToggleSortCommand = new RelayCommand(() => SortAscending = !SortAscending);
            OpenTaskInWindowCommand = new RelayCommand<BacklogTaskItem?>(OpenTaskInWindow);
        }

        public async System.Threading.Tasks.Task InitializeAsync()
        {
            var project = await _backlogService.GetProjectAsync(_projectId);
            if (project != null)
            {
                _projectKey = project.Key;
                CanManageProject = UserHelper.HasAdminPowers() ||
                    project.ProjectUsers.Any(pu => pu.UserId == App.CurrentUser?.Id
                        && (pu.Role == UserRole.Manager || pu.Role == UserRole.Owner));
            }

            OnPropertyChanged(nameof(CanManageProject));
            await LoadTasksAsync();
        }

        public async System.Threading.Tasks.Task LoadTasksAsync()
        {
            var tasks = await _allTasksService.GetAllProjectTasksAsync(_projectId);
            _allTasks = tasks.Select(t => new BacklogTaskItem
            {
                TaskId = t.Id,
                Title = t.Title,
                PerProjectId = t.PerProjectId,
                Key = $"{_projectKey}-{t.PerProjectId}",
                Type = t.Type,
                Priority = t.Priority,
                Status = t.Status
            }).ToList();

            ApplyFilterSort();
        }

        private void ApplyFilterSort()
        {
            IEnumerable<BacklogTaskItem> result = _allTasks;

            result = _selectedFilter switch
            {
                "Open" => result.Where(t => t.Status == TaskStatus.Open),
                "InProgress" => result.Where(t => t.Status == TaskStatus.InProgress),
                "Closed" => result.Where(t => t.Status == TaskStatus.Closed),
                "Bug" => result.Where(t => t.Type == TaskType.Bug),
                "Feature" => result.Where(t => t.Type == TaskType.Feature),
                "Task" => result.Where(t => t.Type == TaskType.Task),
                "High" => result.Where(t => t.Priority == TaskPriority.High),
                "Medium" => result.Where(t => t.Priority == TaskPriority.Medium),
                "Low" => result.Where(t => t.Priority == TaskPriority.Low),
                _ => result
            };

            result = _sortAscending
                ? result.OrderBy(t => t.PerProjectId)
                : result.OrderByDescending(t => t.PerProjectId);

            FilteredTasks.Clear();
            foreach (var t in result)
                FilteredTasks.Add(t);

            OnPropertyChanged(nameof(FilteredTasks));
        }

        private void CreateTask()
        {
            if (!CanManageProject)
            {
                MessageBox.Show("Nie masz uprawnień do tej akcji.", "Brak uprawnień", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var createTaskWindow = new CreateTaskWindow(_projectId);
            if (createTaskWindow.ShowDialog() == true)
            {
                _ = LoadTasksAsync();
            }
        }

        private void OpenTaskInWindow(BacklogTaskItem? item)
        {
            if (item == null) return;
            var win = new TaskDetailsWindow(item.TaskId);
            win.Show();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
