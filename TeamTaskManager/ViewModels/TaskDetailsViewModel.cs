using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TeamTaskManager.Helpers;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Services;
using TeamTaskManager.Views;
using Task = TeamTaskManager.Models.Entities.Task;
using TaskStatus = TeamTaskManager.Models.Enums.TaskStatus;

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
            DeleteCommentCommand = new AsyncRelayCommand<CommentItem>(DeleteCommentAsync);

            EditCommentCommand = new AsyncRelayCommand<CommentItem>(EditCommentAsync);
            CancelEditCommentCommand = new RelayCommand<CommentItem>(CancelEditComment);

            AddReplyCommand = new AsyncRelayCommand<CommentItem>(AddReplyAsync);
            CancelReplyCommand = new RelayCommand<CommentItem>(CancelReply);

            LogWorkCommand = new RelayCommand(LogWork);
            DeleteWorklogCommand = new AsyncRelayCommand<WorklogItem>(DeleteWorklogAsync);
            EditWorklogCommand = new RelayCommand<WorklogItem>(EditWorklog);

            PopOutCommand = new RelayCommand(PopOut);
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


        // otwierania w nowym oknie
        public ICommand PopOutCommand { get; }
        public bool IsPoppedOut { get; set; } = false;
        private void PopOut()
        {
            var window = new TaskDetailsWindow(_taskId);
            window.Show();
        }

        public string WindowTitle => _task != null ? $"Szczegóły zadania {TaskKey}" : "Szczegóły zadania";


        // generalne info o tasku

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
        public Brush TypeBadgeBg => StyleHelper.GetTaskTypeStyle(_task?.Type).Bg;
        public Brush TypeBadgeFg => StyleHelper.GetTaskTypeStyle(_task?.Type).Fg;

        public string StatusDisplay => StyleHelper.GetTaskStatusDisplay(_task?.Status);
        public Brush StatusBadgeBg => StyleHelper.GetTaskStatusStyle(_task?.Status).Bg;
        public Brush StatusBadgeFg => StyleHelper.GetTaskStatusStyle(_task?.Status).Fg;

        public string PriorityDisplay => _task?.Priority.ToString() ?? string.Empty;
        public Brush PriorityColor => StyleHelper.GetTaskPriorityColor(_task?.Priority);

        // assignee i reporter
        public string AssigneeName => _task?.Assignee?.FullName ?? "Nieprzypisane";
        public string AssigneeInitials => StyleHelper.GetInitials(_task?.Assignee?.FullName);
        public Brush AssigneeAvatarBg { get; set; } = new SolidColorBrush(Color.FromRgb(224, 231, 255));
        public Brush AssigneeAvatarFg { get; set; } = new SolidColorBrush(Color.FromRgb(67, 56, 202));

        public string ReporterName => _task?.Reporter?.FullName ?? "Nieprzypisane";
        public string ReporterInitials => StyleHelper.GetInitials(_task?.Reporter?.FullName);
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
                if (Worklogs == null || !Worklogs.Any()) return "0h";
                var totalHours = Worklogs.Sum(w => w.TimeSpent.TotalHours);
                return $"{totalHours:0.#}h";
            }
        }


        // komentarze
        public ObservableCollection<CommentItem> Comments { get; } = new();

        private async System.Threading.Tasks.Task LoadCommentsAsync()
        {
            var comments = await _taskService.GetNonReplyCommentsByTaskIdAsync(_taskId);
            Comments.Clear();
            foreach (var comment in comments.OrderByDescending(c => c.CreatedAt))
            {
                // agregujemy wszystkie odpowiedzi odpowiedzi itp do jednego threada bo inaczej by szybko sie miejsce skonczylo
                var commentItem = new CommentItem(comment);
                commentItem.HasNonDeletedDescendants = AggregateReplies(comment, commentItem.Replies);

                commentItem.Replies = new ObservableCollection<CommentItem>(commentItem.Replies.OrderBy(c => c.CreatedAt));
                Comments.Add(commentItem);
            }

            OnPropertyChanged(string.Empty);
        }

        public bool HasNoComments => Comments.Where(c => !c.IsDeleted).Count() == 0;
        // bool zwraca czy ma jakiekolwiek nieusuniete odpowiedzi
        private bool AggregateReplies(Comment comment, ObservableCollection<CommentItem> collection)
        {
            bool hasNonDeletedDescendants = false;

            if (comment.Replies == null) return false;

            foreach (var reply in comment.Replies)
            {
                var replyItem = new CommentItem(reply);
                collection.Add(replyItem);

                replyItem.HasNonDeletedDescendants = AggregateReplies(reply, collection);

                if (!replyItem.IsDeleted || replyItem.HasNonDeletedDescendants)
                    hasNonDeletedDescendants = true;
            }

            return hasNonDeletedDescendants;
        }

        // automatyczne focusowanie pola tekstu po kliknieciu dodawania komentarza
        [ObservableProperty]
        private bool _focusCommentInput;

        public string NewCommentContent { get; set; } = string.Empty;
        public ICommand AddCommentCommand { get; }
        private async System.Threading.Tasks.Task AddCommentAsync()
        {
            if (string.IsNullOrWhiteSpace(NewCommentContent))
            {
                FocusCommentInput = false;
                FocusCommentInput = true;
                return;
            }

            NewCommentContent = NewCommentContent.Trim();
            await _taskService.CreateTaskCommentAsync(NewCommentContent, _taskId, App.CurrentUser!.Id, null);

            NewCommentContent = string.Empty;
            OnPropertyChanged(nameof(NewCommentContent));

            await LoadCommentsAsync();
        }

        private CommentItem? _activeEditItem;
        public string EditCommentContent { get; set; } = string.Empty;
        public ICommand EditCommentCommand { get; }
        public ICommand CancelEditCommentCommand { get; }
        private async System.Threading.Tasks.Task EditCommentAsync(CommentItem? commentItem)
        {
            if (commentItem == null) return;
            if (commentItem.IsDeleted) return;
            if (!commentItem.CanEdit) return;

            if (commentItem.IsBeingEdited)
            {
                if (string.IsNullOrWhiteSpace(EditCommentContent))
                {
                    MessageBox.Show("Treść komentarza nie może być pusta.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                await _taskService.EditTaskCommentAsync(commentItem.Id, EditCommentContent.Trim());

                commentItem.IsBeingEdited = false;
                _activeEditItem = null;
                EditCommentContent = string.Empty;
                OnPropertyChanged(nameof(EditCommentContent));

                await LoadCommentsAsync();
                return;
            }

            if (_activeEditItem != null && _activeEditItem != commentItem)
            {
                if (_activeEditItem.Content != EditCommentContent &&
                    MessageBox.Show("Edytujesz już inny komentarz. Czy chcesz anulować tę edycję?", "Potwierdzenie",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;
                _activeEditItem.IsBeingEdited = false;
            }

            _activeEditItem = commentItem;
            commentItem.IsBeingEdited = true;
            EditCommentContent = commentItem.Content;

            OnPropertyChanged(nameof(EditCommentContent));
        }

        private void CancelEditComment(CommentItem? commentItem)
        {
            if (commentItem == null) return;

            commentItem.IsBeingEdited = false;
            _activeEditItem = null;

            EditCommentContent = string.Empty;
            OnPropertyChanged(nameof(EditCommentContent));
        }

        public ICommand DeleteCommentCommand { get; }
        private async System.Threading.Tasks.Task DeleteCommentAsync(CommentItem? commentItem)
        {
            if (commentItem == null) return;
            if (commentItem.IsDeleted) return;
            if (!commentItem.CanEdit) return;

            if (MessageBox.Show("Czy na pewno chcesz usunąć ten komentarz?", "Potwierdzenie",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            await _taskService.DeleteTaskCommentAsync(commentItem.Id);

            await LoadCommentsAsync();
        }


        // odpowiedzi
        private CommentItem? _activeReplyItem;
        public string NewReplyContent { get; set; } = string.Empty;
        public ICommand AddReplyCommand { get; }
        public ICommand CancelReplyCommand { get; }

        private async System.Threading.Tasks.Task AddReplyAsync(CommentItem? commentItem)
        {
            if (commentItem == null) return;
            if (commentItem.IsDeleted) return;

            if (commentItem.IsBeingRepliedTo)
            {
                if (string.IsNullOrWhiteSpace(NewReplyContent))
                {
                    MessageBox.Show("Treść odpowiedzi nie może być pusta.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                await _taskService.CreateTaskCommentAsync(NewReplyContent.Trim(), _taskId, App.CurrentUser!.Id, commentItem.Id);

                commentItem.IsBeingRepliedTo = false;
                _activeReplyItem = null;
                NewReplyContent = string.Empty;
                OnPropertyChanged(nameof(NewReplyContent));

                await LoadCommentsAsync();
                return;
            }

            if (_activeReplyItem != null && _activeReplyItem != commentItem)
            {
                if (NewReplyContent != $"@{_activeReplyItem.DisplayName} " &&
                    MessageBox.Show("Odpowiadasz już inny komentarz. Czy chcesz anulować tę odpowiedź?", "Potwierdzenie",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;
                _activeReplyItem.IsBeingRepliedTo = false;
            }

            _activeReplyItem = commentItem;
            commentItem.IsBeingRepliedTo = true;
            NewReplyContent = $"@{commentItem.DisplayName} ";

            OnPropertyChanged(nameof(NewReplyContent));
        }

        private void CancelReply(CommentItem? commentItem)
        {
            if (commentItem == null) return;

            commentItem.IsBeingRepliedTo = false;
            _activeReplyItem = null;

            NewReplyContent = string.Empty;
            OnPropertyChanged(nameof(NewReplyContent));
        }


        // worklogi
        public ObservableCollection<WorklogItem> Worklogs { get; } = new();

        public ICommand LogWorkCommand { get; }
        public ICommand EditWorklogCommand { get; }
        public ICommand DeleteWorklogCommand { get; }

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

        private void LogWork()
        {
            var createWorklogWindow = new CreateWorklogWindow(_taskId);
            if (createWorklogWindow.ShowDialog() == true)
            {
                _ = LoadWorklogsAsync();
            }
        }

        private void EditWorklog(WorklogItem? worklogItem)
        {
            if (worklogItem == null) return;
            if (!worklogItem.CanEdit)
            {
                MessageBox.Show("Możesz modyfikować tylko własne wpisy.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var editWorklogWindow = new EditWorklogWindow(worklogItem.Id);
            if (editWorklogWindow.ShowDialog() == true)
            {
                _ = LoadWorklogsAsync();
            }
        }

        private async System.Threading.Tasks.Task DeleteWorklogAsync(WorklogItem? worklogItem)
        {
            if (worklogItem == null) return;
            if (!worklogItem.CanEdit)
            {
                MessageBox.Show("Możesz usuwać tylko własne wpisy.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show("Czy na pewno chcesz usunąć ten wpis?", "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            await _taskService.DeleteTaskWorklogAsync(worklogItem.Id);

            await LoadWorklogsAsync();
        }
    }

    public partial class CommentItem : ExpandableTextItem
    {
        private readonly Comment _model;

        public CommentItem(Comment model) : base()
        {
            _model = model;
        }

        public int Id => _model.Id;
        public string Content => _model.IsDeleted ? "" : _model.Text;
        public string DisplayName => _model.IsDeleted ? "Nieznany Użytkownik"
                                     : (_model.Commenter?.FullName ?? "Nieznany Użytkownik");

        // widocznosc
        public bool IsDeleted => _model.IsDeleted;
        // pokazujemy usuniete tylko wtedy, gdy maja jakas nieusunieta odpowiedz
        [ObservableProperty]
        public bool _hasNonDeletedDescendants;
        public bool ShowDetails => !IsDeleted || HasNonDeletedDescendants;

        public DateTime CreatedAt => _model.CreatedAt;
        public string CreatedAtStr => _model.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");


        // edytowanie
        public bool WasEdited => _model.UpdatedAt > _model.CreatedAt;
        public string EditInformation => IsDeleted ? "(Usunięto)" : "(Edytowano)";
        public string UpdatedAtStr => (IsDeleted ? "Usunięto: " : "Zmieniono: ")
                                      + _model.UpdatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");

        public bool CanEdit => !IsDeleted && _model.CommenterId == App.CurrentUser?.Id;
        [ObservableProperty]
        private bool _isBeingEdited;

        // odpowiedzi
        public ObservableCollection<CommentItem> Replies { get; set; } = new();
        [ObservableProperty]
        private bool _isBeingRepliedTo;

        // te kolorki to przydaloby sie na przyszlosc od usera uzaleznic aby bylo troche radosci i stymulacji w szarym zyciu programisty
        public string Initials => StyleHelper.GetInitials(DisplayName);
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

        public int Id => _model.Id;
        public string Name => _model.User.FullName;
        public string Description => _model.Description;
        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

        public bool CanEdit => _model.UserId == App.CurrentUser?.Id;
        public bool WasEdited => _model.UpdatedAt > _model.LoggedAt;
        public string UpdatedAtStr => _model.UpdatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");

        public string StartedAtStr => _model.StartTime.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        public string CreatedAtStr => _model.LoggedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        public string DateToolTipStr => "Utworzono: " + CreatedAtStr
                                        + (WasEdited ? "\nZmieniono: " + UpdatedAtStr : "");

        public TimeSpan TimeSpent => _model.TimeSpent;
        public string TimeSpentStr => $"{_model.TimeSpent.TotalHours:0.#}h";

        // avatar
        public string Initials => StyleHelper.GetInitials(Name);
        public Brush AvatarBg { get; set; } = new SolidColorBrush(Color.FromRgb(224, 231, 255));
        public Brush AvatarFg { get; set; } = new SolidColorBrush(Color.FromRgb(67, 56, 202));
    }
}