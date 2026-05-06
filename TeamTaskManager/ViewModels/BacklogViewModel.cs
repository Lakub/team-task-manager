using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TeamTaskManager.Helpers;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Models.Enums;
using TeamTaskManager.Services;
using TeamTaskManager.Views;

namespace TeamTaskManager.ViewModels
{
    public class BacklogTaskItem
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public TaskType Type { get; set; }
        public TaskPriority Priority { get; set; }

        public string TypeDisplay => Type.ToString();
        public string PriorityDisplay => Priority.ToString();

        public Brush PriorityColor => Priority switch
        {
            TaskPriority.High => new SolidColorBrush(Color.FromRgb(0xB9, 0x1C, 0x1C)),
            TaskPriority.Medium => new SolidColorBrush(Color.FromRgb(0xB4, 0x53, 0x09)),
            _ => new SolidColorBrush(Color.FromRgb(0x06, 0x5F, 0x46))
        };

        public Brush TypeBadgeBg => Type == TaskType.Bug
            ? new SolidColorBrush(Color.FromRgb(0xFE, 0xF2, 0xF2))
            : new SolidColorBrush(Color.FromRgb(0xEF, 0xF6, 0xFF));

        public Brush TypeBadgeFg => Type == TaskType.Bug
            ? new SolidColorBrush(Color.FromRgb(0xB9, 0x1C, 0x1C))
            : new SolidColorBrush(Color.FromRgb(0x1D, 0x4E, 0xD8));
    }

    public partial class BacklogViewModel : ObservableObject
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

        [ObservableProperty]
        private string sprintName = string.Empty;

        public bool IsBacklogOnly => _sprintId <= 0;
        public Visibility SprintVisibility => IsBacklogOnly ? Visibility.Collapsed : Visibility.Visible;

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
            }

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
                    Type = t.Type,
                    Priority = t.Priority
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
                    Type = t.Type,
                    Priority = t.Priority
                });
            }
        }

        private async System.Threading.Tasks.Task MoveToSprint(BacklogTaskItem item)
        {
            await _backlogService.AddTaskToSprintAsync(_sprintId, item.TaskId);
            await LoadTasksAsync();
        }

        private async System.Threading.Tasks.Task RemoveFromSprint(BacklogTaskItem item)
        {
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