using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TeamTaskManager.Helpers;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Models.Enums;
using TeamTaskManager.Services;
using TeamTaskManager.Views;
using System.Diagnostics;
using Task = TeamTaskManager.Models.Entities.Task;
using TaskStatus = TeamTaskManager.Models.Enums.TaskStatus;

namespace TeamTaskManager.ViewModels
{
    public partial class TaskDetailsViewModel : ObservableObject
    {
        private readonly ITaskService _taskService;
        private readonly int _taskId;
        private Task? _task;

        public TaskDetailsViewModel(ITaskService taskService, int taskId)
        {
            _taskService = taskService;
            _taskId = taskId;

            AddCommentCommand = new RelayCommand(AddComment);
            LogWorkCommand = new RelayCommand(LogWork);

        }

        public async System.Threading.Tasks.Task InitializeAsync()
        {
            _task = await _taskService.GetTaskByIdAsync(_taskId);

            if (_task == null)
            {
                MessageBox.Show("Nie można znaleźć zadania.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            OnPropertyChanged(string.Empty);

            await LoadCommentsAsync();
            await LoadWorklogsAsync();
        }

        private async System.Threading.Tasks.Task LoadCommentsAsync()
        {
            var comments = await _taskService.GetCommentsByTaskIdAsync(_taskId);
            Comments.Clear();
            foreach (var comment in comments.OrderByDescending(c => c.CreatedAt))
            {
                Comments.Add(new CommentItem(comment));
            }

            OnPropertyChanged(nameof(HasNoComments));
        }

        private async System.Threading.Tasks.Task LoadWorklogsAsync()
        {
            var worklogs = await _taskService.GetWorklogsByTaskIdAsync(_taskId);
            Worklogs.Clear();
            foreach (var worklog in worklogs.OrderByDescending(w => w.LoggedAt))
            {
                Worklogs.Add(new WorklogItem(worklog));
            }

            OnPropertyChanged(nameof(HasNoWorklogs));
            OnPropertyChanged(nameof(TotalTimeSpent));
        }

        public ICommand LogWorkCommand { get; }
        public ICommand AddCommentCommand { get; }

        public string WindowTitle => _task != null ? $"Szczegóły zadania {TaskKey}" : "Szczegóły zadania";

        // te jak na jirze sa PRJKT-2137
        public string TaskKey => _task != null ? $"{_task.Project.Key}-{_task.PerProjectId}" : string.Empty;

        public string Title => _task?.Title ?? string.Empty;
        public string Description => _task?.Description ?? string.Empty;
        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
        public bool HasNoDescription => !HasDescription;

        // kolorki i tekst dla typu, statusu i priorytetu
        public string TypeDisplay => _task?.Type.ToString() ?? string.Empty;
        public Brush TypeBadgeBg => _task?.Type switch
        {
            TaskType.Bug => new SolidColorBrush(Color.FromRgb(0xFE, 0xF2, 0xF2)),       // czerwony
            TaskType.Feature => new SolidColorBrush(Color.FromRgb(0xEF, 0xF6, 0xFF)),   // niebieski
            TaskType.Task => new SolidColorBrush(Color.FromRgb(0xF3, 0xE8, 0xFF)),      // fioletowy
            _ => new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0))                   // szary
        } ?? new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0));
        public Brush TypeBadgeFg => _task?.Type switch
        {
            TaskType.Bug => new SolidColorBrush(Color.FromRgb(0xB9, 0x1C, 0x1C)),       // czerwony
            TaskType.Feature => new SolidColorBrush(Color.FromRgb(0x1D, 0x4E, 0xD8)),   // niebieski
            TaskType.Task => new SolidColorBrush(Color.FromRgb(0x8B, 0x5C, 0xF6)),      // fioletowy
            _ => new SolidColorBrush(Color.FromRgb(0x2B, 0x2B, 0x2B))                   // szary
        } ?? new SolidColorBrush(Color.FromRgb(0x2B, 0x2B, 0x2B));

        public string StatusDisplay => _task?.Status switch
        {
            TaskStatus.Closed => "Zamknięte",
            TaskStatus.InProgress => "W trakcie",
            _ => "Otwarte"
        } ?? string.Empty;

        public Brush StatusBadgeBg => _task?.Status switch
        {
            TaskStatus.Closed => new SolidColorBrush(Color.FromRgb(0xD1, 0xFA, 0xE5)),      // zielony
            TaskStatus.InProgress => new SolidColorBrush(Color.FromRgb(0xDB, 0xEB, 0xFF)),  // niebieski
            TaskStatus.Open => new SolidColorBrush(Color.FromRgb(0xF3, 0xF4, 0xF6)),        // szary
            _ => new SolidColorBrush(Color.FromRgb(0xF3, 0xF4, 0xF6))                       // szary
        } ?? new SolidColorBrush(Color.FromRgb(0xF3, 0xF4, 0xF6));
        public Brush StatusBadgeFg => _task?.Status switch
        {
            TaskStatus.Closed => new SolidColorBrush(Color.FromRgb(0x06, 0x5F, 0x46)),      // zielony
            TaskStatus.InProgress => new SolidColorBrush(Color.FromRgb(0x1D, 0x4E, 0xD8)),  // niebieski
            TaskStatus.Open => new SolidColorBrush(Color.FromRgb(0x37, 0x41, 0x51)),        // szary
            _ => new SolidColorBrush(Color.FromRgb(0x37, 0x41, 0x51))                       // szary
        } ?? new SolidColorBrush(Color.FromRgb(0x37, 0x41, 0x51));

        public string PriorityDisplay => _task?.Priority.ToString() ?? string.Empty;
        public Brush PriorityColor => _task?.Priority switch
        {
            TaskPriority.High => new SolidColorBrush(Color.FromRgb(0xB9, 0x1C, 0x1C)),      // czerwony
            TaskPriority.Medium => new SolidColorBrush(Color.FromRgb(0xB4, 0x53, 0x09)),    // pomarańczowy
            TaskPriority.Low => new SolidColorBrush(Color.FromRgb(0x16, 0xA3, 0x9C)),       // morski
            _ => new SolidColorBrush(Color.FromRgb(0x2B, 0x2B, 0x2B))                       // szary
        } ?? new SolidColorBrush(Color.FromRgb(0x2B, 0x2B, 0x2B));

        // assignee i reporter
        public string AssigneeName => _task?.Assignee?.FullName ?? "Nieprzypisane";
        public string AssigneeInitials => string.IsNullOrWhiteSpace(_task?.Assignee?.FullName) ? "?" : string.Concat(_task.Assignee.FullName.Split(' ').Select(n => n[0])).ToUpper();
        public Brush AssigneeAvatarBg { get; set; } = new SolidColorBrush(Color.FromRgb(224, 231, 255));
        public Brush AssigneeAvatarFg { get; set; } = new SolidColorBrush(Color.FromRgb(67, 56, 202));

        public string ReporterName => _task?.Reporter?.FullName ?? "Nieprzypisane";
        public string ReporterInitials => string.IsNullOrWhiteSpace(_task?.Reporter?.FullName) ? "?" : string.Concat(_task.Reporter.FullName.Split(' ').Select(n => n[0])).ToUpper();
        public Brush ReporterAvatarBg { get; set; } = new SolidColorBrush(Color.FromRgb(224, 231, 255));
        public Brush ReporterAvatarFg { get; set; } = new SolidColorBrush(Color.FromRgb(67, 56, 202));

        // timestampy
        public string CreatedAtStr => _task?.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm") ?? string.Empty;
        public string UpdatedAtStr => _task?.UpdatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm") ?? string.Empty;

        // czas spedzony na zadaniu
        public string TotalTimeSpent
        {
            get
            {
                if (_task?.Worklogs == null || !_task.Worklogs.Any()) return "0h";
                var totalHours = _task?.Worklogs.Sum(w => w.TimeSpent.TotalHours);
                return $"{totalHours:0.#}h";
            }
        }

        // komentarze
        public ObservableCollection<CommentItem> Comments { get; } = new();
        public bool HasNoComments => !Comments.Any();

        // worklogi
        public ObservableCollection<WorklogItem> Worklogs { get; } = new();
        public bool HasNoWorklogs => !Worklogs.Any();

        private void LogWork()
        {
            MessageBox.Show("ni ma", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddComment()
        {
            MessageBox.Show("ni ma", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public class CommentItem
    {
        private readonly Comment _model;
        public CommentItem(Comment model) { _model = model; }

        public string Content => _model.Text;
        public string Name => _model.Commenter.FullName;
        public string CreatedAtStr => _model.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");

        // te kolorki to przydaloby sie na przyszlosc od usera uzaleznic aby bylo troche radosci i stymulacji w szarym zyciu programisty
        public string Initials => string.Concat(Name.Split(' ').Select(n => n[0])).ToUpper();
        public Brush AvatarBg { get; set; } = new SolidColorBrush(Color.FromRgb(224, 231, 255));
        public Brush AvatarFg { get; set; } = new SolidColorBrush(Color.FromRgb(67, 56, 202));
    }

    public class WorklogItem
    {
        private readonly Worklog _model;
        public WorklogItem(Worklog model) { _model = model; }
        public string Name => _model.User.FullName;
        public string CreatedAtStr => _model.LoggedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        public string TimeSpentStr => $"{_model.TimeSpent.TotalHours:0.#}h";
        public string Initials => string.Concat(Name.Split(' ').Select(n => n[0])).ToUpper();
        public Brush AvatarBg { get; set; } = new SolidColorBrush(Color.FromRgb(224, 231, 255));
        public Brush AvatarFg { get; set; } = new SolidColorBrush(Color.FromRgb(67, 56, 202));
    }
}