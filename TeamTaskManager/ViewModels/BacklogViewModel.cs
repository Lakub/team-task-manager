using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TeamTaskManager.Helpers;
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

        public TaskStatus Status { get; set; }
        public Brush StatusColor => StyleHelper.GetTaskStatusColor(Status);

        public TaskPriority Priority { get; set; }
        public string PriorityDisplay => Priority.ToString();
        public Brush PriorityColor => StyleHelper.GetTaskPriorityColor(Priority);

        public TaskType Type { get; set; }
        public string TypeDisplay => Type.ToString();
        public Brush TypeBadgeBg => StyleHelper.GetTaskTypeStyle(Type).Bg;
        public Brush TypeBadgeFg => StyleHelper.GetTaskTypeStyle(Type).Fg;
    }

    public partial class BacklogViewModel : ObservableObject
    {
        private readonly IBacklogService _backlogService;
        private readonly int? _sprintId;
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

        public BacklogViewModel(IBacklogService backlogService, int projectId, int? sprintId)
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
            var project = await _backlogService.GetProjectAsync(_projectId);
            if (project != null)
            {
                ProjectKey = project.Key;
                CanManageProject = UserHelper.HasAdminPowers() ||
                                  project.ProjectUsers.Any(pu => pu.UserId == App.CurrentUser?.Id
                                                              && (pu.Role == UserRole.Manager || pu.Role == UserRole.Owner));
            }

            if (_sprintId.HasValue)
            {
                var sprint = await _backlogService.GetSprintAsync(_sprintId.Value);
                if (sprint != null)
                {
                    SprintName = sprint.Name;
                    IsActive = sprint.Status == SprintStatus.Active;
                    IsPlanned = sprint.Status == SprintStatus.Planned;
                    HasSprint = true;
                }
            }

            OnPropertyChanged(string.Empty);
            await LoadTasksAsync();
        }

        private async System.Threading.Tasks.Task LoadTasksAsync()
        {
            SprintTasks.Clear();

            if (_sprintId.HasValue)
            {
                var sprintTasks = await _backlogService.GetActiveSprintTasksAsync(_sprintId.Value);
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
            }

            var backlogTasks = await _backlogService.GetBacklogTasksAsync(_projectId);
            BacklogTasks.Clear();
            foreach (var t in backlogTasks)
            {
                // nie pokazujemy taskow, ktore sa juz w sprincie
                if (SprintTasks.Any(st => st.TaskId == t.Id)) continue;

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

        public async System.Threading.Tasks.Task MoveToSprint(BacklogTaskItem? item)
        {
            if (!_sprintId.HasValue) return;

            if (!CanEditSprint)
            {
                MessageBox.Show("Nie masz uprawnień do tej akcji.", "Brak uprawnień", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (item == null) return;

            // czy jest juz w sprincie
            if (SprintTasks.Any(t => t.TaskId == item.TaskId)) return;

            await _backlogService.AddTaskToSprintAsync(_sprintId.Value, item.TaskId);
            await LoadTasksAsync();
        }

        public async System.Threading.Tasks.Task RemoveFromSprint(BacklogTaskItem? item)
        {
            if (!_sprintId.HasValue) return;

            if (!CanEditSprint)
            {
                MessageBox.Show("Nie masz uprawnień do tej akcji.", "Brak uprawnień", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (item == null) return;

            await _backlogService.RemoveTaskFromSprintAsync(_sprintId.Value, item.TaskId);
            await LoadTasksAsync();
        }

        private void OpenSprintReport()
        {
            if (!_sprintId.HasValue) return;

            var reportView = new SprintReportView(_sprintId.Value, _projectId);
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
    }
}