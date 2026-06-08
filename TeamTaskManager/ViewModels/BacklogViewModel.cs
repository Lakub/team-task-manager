using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TeamTaskManager.Helpers;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Models.Enums;
using TeamTaskManager.Services;
using TeamTaskManager.Views;
using TaskStatus = TeamTaskManager.Models.Enums.TaskStatus;

namespace TeamTaskManager.ViewModels
{
    public class BacklogTaskItem
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = "";
        public string Key { get; set; } = "";
        public int PerProjectId { get; set; } = 0;
        public string KeyAndTitle => $"{Key} - {Title}";
        public TaskType Type { get; set; }
        public TaskPriority Priority { get; set; }
        public TaskStatus Status { get; set; }

        public string TypeDisplay => Type.ToString();
        public string PriorityDisplay => Priority.ToString();

        public Brush PriorityColor => Priority switch
        {
            TaskPriority.High => new SolidColorBrush(Color.FromRgb(0xB9, 0x1C, 0x1C)),
            TaskPriority.Medium => new SolidColorBrush(Color.FromRgb(0xB4, 0x53, 0x09)),
            _ => new SolidColorBrush(Color.FromRgb(0x06, 0x5F, 0x46))
        };

        public Brush TypeBadgeBg => Type switch
        {
            TaskType.Bug => new SolidColorBrush(Color.FromRgb(0xFE, 0xF2, 0xF2)),       // czerwony
            TaskType.Feature => new SolidColorBrush(Color.FromRgb(0xF3, 0xE8, 0xFF)),   // fioletowy
            TaskType.Task => new SolidColorBrush(Color.FromRgb(0xEF, 0xF6, 0xFF)),      // niebieski
            _ => new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0))                   // szary
        };
        public Brush TypeBadgeFg => Type switch
        {
            TaskType.Bug => new SolidColorBrush(Color.FromRgb(0xB9, 0x1C, 0x1C)),       // czerwony
            TaskType.Feature => new SolidColorBrush(Color.FromRgb(0x8B, 0x5C, 0xF6)),   // fioletowy
            TaskType.Task => new SolidColorBrush(Color.FromRgb(0x1D, 0x4E, 0xD8)),      // niebieski
            _ => new SolidColorBrush(Color.FromRgb(0x2B, 0x2B, 0x2B))                   // szary
        };
    }

    public partial class BacklogViewModel : INotifyPropertyChanged
    {
        private readonly IBacklogService _backlogService;
        private readonly int _sprintId;
        private readonly int _projectId;

        public Action<BacklogTaskItem>? OnTaskSelected { get; set; }
        public ICommand MoveToSprintCommand { get; }
        public ICommand RemoveFromSprintCommand { get; }
        public ICommand OpenTaskCommand { get; }
        public ICommand OpenSprintReportCommand { get; }
        public ICommand CreateTaskCommand { get; }

        private string ProjectKey { get; set; } = string.Empty;
        public string SprintName { get; set; }  = string.Empty;
        public string StatusText => IsActive ? "W toku" : IsPlanned ? "Planowany" : "Zakończony";

        public bool IsActive { get; set; }
        public bool IsPlanned { get; set; }
        public bool HasSprint { get; set; } = false;
        public bool CanManageProject { get; set; } = false;

        public bool CanEditSprint => HasSprint && (IsPlanned || IsActive) && CanManageProject;

        public ObservableCollection<BacklogTaskItem> SprintTasks { get; } = new();
        public ObservableCollection<BacklogTaskItem> BacklogTasks { get; } = new();

        public BacklogViewModel(IBacklogService backlogService, int sprintId, int projectId)
        {
            _backlogService = backlogService;
            _sprintId = sprintId;
            _projectId = projectId;

            MoveToSprintCommand = new AsyncRelayCommand<BacklogTaskItem>(MoveToSprint);
            RemoveFromSprintCommand = new AsyncRelayCommand<BacklogTaskItem>(RemoveFromSprint);
            OpenTaskCommand = new RelayCommand<BacklogTaskItem?>(OpenTask);
            OpenSprintReportCommand = new RelayCommand(OpenSprintReport);
            CreateTaskCommand = new RelayCommand(CreateTask);
        }

        public async System.Threading.Tasks.Task InitializeAsync()
        {
            var sprint = await _backlogService.GetSprintAsync(_sprintId);
            if (sprint != null)
            {
                SprintName = sprint.Name;
                IsActive = sprint.Status == SprintStatus.Active;
                IsPlanned = sprint.Status == SprintStatus.Planned;
                HasSprint = true;
            }

            var project = await _backlogService.GetProjectAsync(_projectId);
            if (project != null)
            {
                ProjectKey = project.Key;
                CanManageProject = UserHelper.HasAdminPowers() ||
                                  project.ProjectUsers.Any(pu => pu.UserId == App.CurrentUser?.Id
                                                              && (pu.Role == UserRole.Manager || pu.Role == UserRole.Owner));
            }

            OnPropertyChanged(string.Empty);
            await LoadTasksAsync();
        }

        private async System.Threading.Tasks.Task LoadTasksAsync()
        {
            var sprintTasks = await _backlogService.GetActiveSprintTasksAsync(_sprintId);
            SprintTasks.Clear();
            foreach (var t in sprintTasks)
            {
                SprintTasks.Add(new BacklogTaskItem
                {
                    TaskId = t.Id,
                    Title = t.Title,
                    PerProjectId = t.PerProjectId,
                    Key = $"{ProjectKey}-{t.PerProjectId}",
                    Type = t.Type,
                    Priority = t.Priority,
                    Status = t.Status
                });
            }

            var backlogTasks = await _backlogService.GetBacklogTasksAsync(_projectId);
            BacklogTasks.Clear();
            foreach (var t in backlogTasks)
            {
                BacklogTasks.Add(new BacklogTaskItem
                {
                    TaskId = t.Id,
                    Title = t.Title,
                    PerProjectId = t.PerProjectId,
                    Key = $"{ProjectKey}-{t.PerProjectId}",
                    Type = t.Type,
                    Priority = t.Priority,
                    Status = t.Status
                });
            }

            OnPropertyChanged(string.Empty);
        }

        private async System.Threading.Tasks.Task MoveToSprint(BacklogTaskItem? item)
        {
            if (!CanEditSprint)
            {
                MessageBox.Show("Nie masz uprawnień do tej akcji.", "Brak uprawnień", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (item == null) return;

            await _backlogService.AddTaskToSprintAsync(_sprintId, item.TaskId);
            await LoadTasksAsync();
        }

        private async System.Threading.Tasks.Task RemoveFromSprint(BacklogTaskItem? item)
        {
            if (!CanEditSprint)
            {
                MessageBox.Show("Nie masz uprawnień do tej akcji.", "Brak uprawnień", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (item == null) return;

            await _backlogService.RemoveTaskFromSprintAsync(_sprintId, item.TaskId);
            await LoadTasksAsync();
        }

        private void OpenSprintReport()
        {
            var reportView = new SprintReportView(_sprintId, _projectId);
            WeakReferenceMessenger.Default.Send(new NavigationMessage(reportView));
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

        public void OpenTask(BacklogTaskItem? item)
        {
            if (item != null) OnTaskSelected?.Invoke(item);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}