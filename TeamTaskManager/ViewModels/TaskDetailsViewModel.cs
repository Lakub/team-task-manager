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
using System.ComponentModel;

namespace TeamTaskManager.ViewModels
{
    public partial class TaskDetailsViewModel : ExpandableTextItem
    {
        private readonly ITaskService _taskService;
        private readonly int _taskId;
        private Task? _task;

        public TaskDetailsViewModel(ITaskService taskService, int taskId) : base(lineHeight: 22, maxLines: 4)
        {
            _taskService = taskService;
            _taskId = taskId;

            AddCommentCommand = new AsyncRelayCommand(AddCommentAsync);
            AddReplyCommand = new AsyncRelayCommand<CommentItem>(AddReplyAsync);
            CancelReplyCommand = new RelayCommand<CommentItem>(CancelReply);
            LogWorkCommand = new RelayCommand(LogWork);
            PopOutCommand = new RelayCommand(() =>
            {
                var window = new TaskDetailsWindow(_taskId);
                window.Show();
            });
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
            var comments = await _taskService.GetNonReplyCommentsByTaskIdAsync(_taskId);
            Comments.Clear();
            foreach (var comment in comments.OrderByDescending(c => c.CreatedAt))
            {
                // agregujemy wszystkie odpowiedzi odpowiedzi itp do jednego threada bo inaczej by szybko sie miejsce skonczylo
                var commentItem = new CommentItem(comment);
                AggregateReplies(comment, commentItem.Replies);
                commentItem.Replies = new ObservableCollection<CommentItem>(commentItem.Replies.OrderBy(c => c.CreatedAt));
                Comments.Add(commentItem);
            }

            OnPropertyChanged(string.Empty);
        }

        private async System.Threading.Tasks.Task LoadWorklogsAsync()
        {
            var worklogs = await _taskService.GetWorklogsByTaskIdAsync(_taskId);
            Worklogs.Clear();
            foreach (var worklog in worklogs.OrderByDescending(w => w.LoggedAt))
            {
                Worklogs.Add(new WorklogItem(worklog));
            }

            OnPropertyChanged(string.Empty);
        }

        public ICommand LogWorkCommand { get; }
        public ICommand AddCommentCommand { get; }
        public ICommand AddReplyCommand { get; }
        public ICommand CancelReplyCommand { get; }
        public ICommand PopOutCommand { get; }

        public bool IsPoppedOut { get; set; } = false;

        public string WindowTitle => _task != null ? $"Szczegóły zadania {TaskKey}" : "Szczegóły zadania";

        // te jak na jirze sa PRJKT-2137
        public string TaskKey => _task != null ? $"{_task.Project.Key}-{_task.PerProjectId}" : string.Empty;

        public string Title => _task?.Title ?? string.Empty;
        public string Description => _task?.Description ?? string.Empty;
        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
        public bool HasNoDescription => !HasDescription;

        // ostatni sprint
        public string SprintName
        {
            get
            {
                if (_task?.SprintTasks == null || !_task.SprintTasks.Any()) return "Brak";
                var latest = _task.SprintTasks
                    .Where(st => st.Sprint != null)
                    .OrderByDescending(st => st.Sprint!.StartDate)
                    .FirstOrDefault();
                return latest?.Sprint?.Name ?? "Brak";
            }
        }

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
        public string NewCommentContent { get; set; } = string.Empty;
        public bool ShowAddComment => !string.IsNullOrWhiteSpace(NewCommentContent);
        public string NewReplyContent { get; set; } = string.Empty;

        // worklogi
        public ObservableCollection<WorklogItem> Worklogs { get; } = new();

        private void LogWork()
        {
            var createWorklogWindow = new CreateWorklogWindow(_taskId);
            if (createWorklogWindow.ShowDialog() == true)
            {
                _ = LoadWorklogsAsync();
            }
        }

        private async System.Threading.Tasks.Task AddReplyAsync(CommentItem? commentItem)
        {
            if (commentItem == null) return;

            if (commentItem.IsBeingRepliedTo)
            {
                if (string.IsNullOrWhiteSpace(NewReplyContent))
                {
                    MessageBox.Show("Treść odpowiedzi nie może być pusta.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                NewReplyContent = NewReplyContent.Trim();
                await _taskService.CreateTaskCommentAsync(NewReplyContent, _taskId, App.CurrentUser!.Id, commentItem.Id);

                commentItem.IsBeingRepliedTo = false;
                NewReplyContent = string.Empty;
                OnPropertyChanged(nameof(NewReplyContent));

                await LoadCommentsAsync();
                return;
            }

            foreach (var item in Comments)
            {
                item.IsBeingRepliedTo = false;
                foreach (var reply in item.Replies)
                {
                    reply.IsBeingRepliedTo = false;
                }
            }

            if (commentItem != null)
            {
                commentItem.IsBeingRepliedTo = true;
                NewReplyContent = $"@{commentItem.Name} ";
            }

            OnPropertyChanged(nameof(NewReplyContent));
        }

        private void CancelReply(CommentItem? commentItem)
        {
            if (commentItem == null) return;

            commentItem.IsBeingRepliedTo = false;

            NewReplyContent = string.Empty;
            OnPropertyChanged(nameof(NewReplyContent));
        }

        private async System.Threading.Tasks.Task AddCommentAsync()
        {
            if (string.IsNullOrWhiteSpace(NewCommentContent))
            {
                MessageBox.Show("Treść komentarza nie może być pusta.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            NewCommentContent = NewCommentContent.Trim();
            await _taskService.CreateTaskCommentAsync(NewCommentContent, _taskId, App.CurrentUser!.Id, null);

            NewCommentContent = string.Empty;
            OnPropertyChanged(nameof(NewCommentContent));

            await LoadCommentsAsync();
        }

        private void AggregateReplies(Comment comment, ObservableCollection<CommentItem> collection)
        {
            foreach (var reply in comment.Replies)
            {
                var replyItem = new CommentItem(reply);
                collection.Add(replyItem);
                AggregateReplies(reply, collection);
            }
        }
    }

    public class CommentItem : ExpandableTextItem
    {
        private readonly Comment _model;

        public CommentItem(Comment model) : base()
        {
            _model = model;
        }

        public string Content => _model.Text;
        public string Name => _model.Commenter?.FullName ?? "Unknown";
        public int Id => _model.Id;
        public DateTime CreatedAt => _model.CreatedAt;
        public string CreatedAtStr => _model.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");

        // odpowiedzi
        public ObservableCollection<CommentItem> Replies { get; set; } = new();
        private bool _isBeingRepliedTo;
        public bool IsBeingRepliedTo
        {
            get => _isBeingRepliedTo;
            set { if (_isBeingRepliedTo != value) { _isBeingRepliedTo = value; OnPropertyChanged(); } }
        }

        // te kolorki to przydaloby sie na przyszlosc od usera uzaleznic aby bylo troche radosci i stymulacji w szarym zyciu programisty
        public string Initials => string.Concat(Name.Split(' ').Select(n => n[0])).ToUpper();
        public Brush AvatarBg { get; set; } = new SolidColorBrush(Color.FromRgb(224, 231, 255));
        public Brush AvatarFg { get; set; } = new SolidColorBrush(Color.FromRgb(67, 56, 202));
    }

    public class WorklogItem : ExpandableTextItem
    {
        private readonly Worklog _model;
        public WorklogItem(Worklog model) : base()
        {
            _model = model;
        }

        public string Name => _model.User.FullName;
        public string Description => _model.Description;
        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
        public string CreatedAtStr => _model.LoggedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        public string CreatedAtFullStr => "Created: " + CreatedAtStr;
        public string StartedAtStr => _model.StartTime.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        public string TimeSpentStr => $"{_model.TimeSpent.TotalHours:0.#}h";

        // avatar
        public string Initials => string.Concat(Name.Split(' ').Select(n => n[0])).ToUpper();
        public Brush AvatarBg { get; set; } = new SolidColorBrush(Color.FromRgb(224, 231, 255));
        public Brush AvatarFg { get; set; } = new SolidColorBrush(Color.FromRgb(67, 56, 202));
    }
}