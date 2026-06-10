using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Enums;
using TeamTaskManager.Views;

namespace TeamTaskManager.ViewModels
{
    // ─── Panel enum ──────────────────────────────────────────────────────────────

    public enum AdminPanel { Stats, Users, Projects, Tasks, Sprints, Comments, Worklogs, Attachments, Docs }

    // ─── Pie chart slice ─────────────────────────────────────────────────────────

    public class PieSlice
    {
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; }
        public Brush Color { get; set; } = Brushes.Gray;
    }

    // ─── User role row ───────────────────────────────────────────────────────────

    public class UserRoleRow : INotifyPropertyChanged
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string LastLogin { get; set; } = string.Empty;

        private OrgRole _role;
        public OrgRole Role
        {
            get => _role;
            set { _role = value; Notify(nameof(Role)); Notify(nameof(RoleLabel)); Notify(nameof(CanPromote)); Notify(nameof(CanDemote)); }
        }

        public string RoleLabel => Role switch { OrgRole.HeadAdmin => "HeadAdmin", OrgRole.Admin => "Admin", _ => "User" };
        public bool CanPromote => Role == OrgRole.User;
        public bool CanDemote  => Role == OrgRole.Admin;
        public bool CanDelete  => Role != OrgRole.HeadAdmin && UserId != App.CurrentUser?.Id;

        public ICommand PromoteCommand { get; }
        public ICommand DemoteCommand  { get; }
        public ICommand DeleteCommand  { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void Notify(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        public UserRoleRow(Action<UserRoleRow, OrgRole> onChange, Action<UserRoleRow> onDelete)
        {
            PromoteCommand = new RelayCommand(() => onChange(this, OrgRole.Admin));
            DemoteCommand  = new RelayCommand(() => onChange(this, OrgRole.User));
            DeleteCommand  = new RelayCommand(() => onDelete(this));
        }
    }

    // ─── Data row types ──────────────────────────────────────────────────────────

    public class ProjectRow
    {
        public string Name { get; set; } = string.Empty;
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public int MembersCount { get; set; }
        public int TasksCount { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
    }

    public class TaskRow
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public string PriorityLabel { get; set; } = string.Empty;
        public string ReporterName { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public ICommand DeleteCommand { get; }

        public TaskRow(Action<TaskRow> onDelete)
        {
            DeleteCommand = new RelayCommand(() => onDelete(this));
        }
    }

    public class SprintRow : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;

        private SprintStatus _status;
        public SprintStatus Status
        {
            get => _status;
            set { _status = value; Notify(nameof(Status)); Notify(nameof(StatusLabel)); Notify(nameof(CanActivate)); Notify(nameof(CanComplete)); }
        }

        public string StatusLabel => Status switch { SprintStatus.Active => "Aktywny", SprintStatus.Completed => "Ukończony", _ => "Planowany" };
        public bool CanActivate => Status == SprintStatus.Planned;
        public bool CanComplete => Status == SprintStatus.Active;

        public ICommand ActivateCommand { get; }
        public ICommand CompleteCommand { get; }
        public ICommand DeleteCommand   { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void Notify(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        public SprintRow(Action<SprintRow, SprintStatus> onChangeStatus, Action<SprintRow> onDelete)
        {
            ActivateCommand = new RelayCommand(() => onChangeStatus(this, SprintStatus.Active));
            CompleteCommand = new RelayCommand(() => onChangeStatus(this, SprintStatus.Completed));
            DeleteCommand   = new RelayCommand(() => onDelete(this));
        }
    }

    public class CommentRow
    {
        public int Id { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
        public string TextPreview { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public ICommand DeleteCommand { get; }

        public CommentRow(Action<CommentRow> onDelete)
        {
            DeleteCommand = new RelayCommand(() => onDelete(this));
        }
    }

    public class WorklogRow
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
        public string TimeSpent { get; set; } = string.Empty;
        public string LoggedAt { get; set; } = string.Empty;
        public ICommand DeleteCommand { get; }

        public WorklogRow(Action<WorklogRow> onDelete)
        {
            DeleteCommand = new RelayCommand(() => onDelete(this));
        }
    }

    public class AttachmentRow
    {
        public string FileName { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
        public string UploaderName { get; set; } = string.Empty;
        public string UploadedAt { get; set; } = string.Empty;
    }

    public class DocRow
    {
        public string Title { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;
    }

    // ─── Main ViewModel ──────────────────────────────────────────────────────────

    public partial class HeadAdminPanelViewModel : ObservableObject
    {
        // Stat counters
        [ObservableProperty] private string accessMessage    = string.Empty;
        [ObservableProperty] private int    usersCount;
        [ObservableProperty] private int    projectsCount;
        [ObservableProperty] private int    tasksCount;
        [ObservableProperty] private int    sprintsCount;
        [ObservableProperty] private int    commentsCount;
        [ObservableProperty] private int    worklogsCount;
        [ObservableProperty] private int    attachmentsCount;
        [ObservableProperty] private int    documentationCount;

        // Access control
        private bool _accessDenied;
        public bool AccessDenied
        {
            get => _accessDenied;
            set { if (SetProperty(ref _accessDenied, value)) NotifyPanels(); }
        }

        // Current panel
        private AdminPanel _currentPanel = AdminPanel.Stats;
        public AdminPanel CurrentPanel
        {
            get => _currentPanel;
            private set { if (SetProperty(ref _currentPanel, value)) NotifyPanels(); }
        }

        // Computed visibility
        public bool HasAccess            => !AccessDenied;
        public bool ShowStats            => HasAccess && CurrentPanel == AdminPanel.Stats;
        public bool ShowUserListPanel    => HasAccess && CurrentPanel == AdminPanel.Users;
        public bool ShowProjectsPanel    => HasAccess && CurrentPanel == AdminPanel.Projects;
        public bool ShowTasksPanel       => HasAccess && CurrentPanel == AdminPanel.Tasks;
        public bool ShowSprintsPanel     => HasAccess && CurrentPanel == AdminPanel.Sprints;
        public bool ShowCommentsPanel    => HasAccess && CurrentPanel == AdminPanel.Comments;
        public bool ShowWorklogsPanel    => HasAccess && CurrentPanel == AdminPanel.Worklogs;
        public bool ShowAttachmentsPanel => HasAccess && CurrentPanel == AdminPanel.Attachments;
        public bool ShowDocsPanel        => HasAccess && CurrentPanel == AdminPanel.Docs;

        private void NotifyPanels()
        {
            OnPropertyChanged(nameof(HasAccess));
            OnPropertyChanged(nameof(ShowStats));
            OnPropertyChanged(nameof(ShowUserListPanel));
            OnPropertyChanged(nameof(ShowProjectsPanel));
            OnPropertyChanged(nameof(ShowTasksPanel));
            OnPropertyChanged(nameof(ShowSprintsPanel));
            OnPropertyChanged(nameof(ShowCommentsPanel));
            OnPropertyChanged(nameof(ShowWorklogsPanel));
            OnPropertyChanged(nameof(ShowAttachmentsPanel));
            OnPropertyChanged(nameof(ShowDocsPanel));
        }

        // Data collections
        public ObservableCollection<UserRoleRow>   UserList       { get; } = new();
        public ObservableCollection<ProjectRow>    ProjectList    { get; } = new();
        public ObservableCollection<TaskRow>       TaskList       { get; } = new();
        public ObservableCollection<SprintRow>     SprintList     { get; } = new();
        public ObservableCollection<CommentRow>    CommentList    { get; } = new();
        public ObservableCollection<WorklogRow>    WorklogList    { get; } = new();
        public ObservableCollection<AttachmentRow> AttachmentList { get; } = new();
        public ObservableCollection<DocRow>        DocList        { get; } = new();
        public ObservableCollection<PieSlice>      TaskSlices     { get; } = new();
        public ObservableCollection<PieSlice>      SprintSlices   { get; } = new();

        // Commands
        public ICommand BackToStatsCommand     { get; }
        public ICommand ShowUsersCommand       { get; }
        public ICommand ShowProjectsCommand    { get; }
        public ICommand ShowTasksCommand       { get; }
        public ICommand ShowSprintsCommand     { get; }
        public ICommand ShowCommentsCommand    { get; }
        public ICommand ShowWorklogsCommand    { get; }
        public ICommand ShowAttachmentsCommand { get; }
        public ICommand ShowDocsCommand        { get; }
        public ICommand EditProjectCommand     { get; }
        public ICommand RemoveProjectCommand   { get; }

        public HeadAdminPanelViewModel()
        {
            BackToStatsCommand     = new RelayCommand(() => CurrentPanel = AdminPanel.Stats);
            ShowUsersCommand       = new RelayCommand(() => CurrentPanel = AdminPanel.Users);
            ShowProjectsCommand    = new RelayCommand(() => { LoadProjects();    CurrentPanel = AdminPanel.Projects; });
            ShowTasksCommand       = new RelayCommand(() => { LoadTasks();       CurrentPanel = AdminPanel.Tasks; });
            ShowSprintsCommand     = new RelayCommand(() => { LoadSprints();     CurrentPanel = AdminPanel.Sprints; });
            ShowCommentsCommand    = new RelayCommand(() => { LoadComments();    CurrentPanel = AdminPanel.Comments; });
            ShowWorklogsCommand    = new RelayCommand(() => { LoadWorklogs();    CurrentPanel = AdminPanel.Worklogs; });
            ShowAttachmentsCommand = new RelayCommand(() => { LoadAttachments(); CurrentPanel = AdminPanel.Attachments; });
            ShowDocsCommand        = new RelayCommand(() => { LoadDocs();        CurrentPanel = AdminPanel.Docs; });
            EditProjectCommand = new RelayCommand<ProjectRow>((row) => {
                Models.Entities.Project? selectedProject;
                using (var ctx = new AppDbContext())
                {
                    selectedProject = ctx.Projects.FirstOrDefault(e => e.Id == row.Id);
                    if (selectedProject == null) return;
                    selectedProject.ProjectUsers = ctx.ProjectUsers.Where(e => e.ProjectId == selectedProject.Id).ToList();
                }
                var editProjectWindow = new CreateProjectWindow(true, selectedProject);
                if (editProjectWindow.ShowDialog() == true)
                    LoadProjects();
            });
            RemoveProjectCommand = new RelayCommand<ProjectRow>(async (row) => {
                var ctx = new AppDbContext();
                var project = await ctx.Projects.FirstOrDefaultAsync(e => e.Id == row.Id);
                if (project == null) return;
                if (MessageBox.Show("Czy napewno usunąć projekt?", "", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
                {
                    project.IsDeleted = true;
                    ctx.Projects.Update(project);
                    await ctx.SaveChangesAsync();
                    LoadProjects();
                }
            });
            Load();
        }

        public void Reload() => Load();

        // ─── Stats + Users load ──────────────────────────────────────────────────

        private void Load()
        {
            if (App.CurrentUser?.OrgRole != OrgRole.HeadAdmin)
            {
                AccessDenied  = true;
                AccessMessage = "Dostęp do panelu HeadAdmin ma wyłącznie użytkownik z rolą HeadAdmin.";
                return;
            }

            using var ctx = new AppDbContext();

            AccessDenied  = false;
            AccessMessage = string.Empty;
            CurrentPanel  = AdminPanel.Stats;

            UsersCount         = ctx.Users.Count(u => !u.IsDeleted);
            ProjectsCount      = ctx.Projects.Count(p => !p.IsDeleted);
            TasksCount         = ctx.Tasks.Count(t => !t.IsDeleted);
            SprintsCount       = ctx.Sprints.Count(s => !s.IsDeleted);
            CommentsCount      = ctx.Comments.Count(c => !c.IsDeleted);
            WorklogsCount      = ctx.Worklogs.Count(w => !w.IsDeleted);
            AttachmentsCount   = ctx.Attachments.Count(a => !a.IsDeleted);
            DocumentationCount = ctx.WikiArticles.Count();

            UserList.Clear();
            foreach (var u in ctx.Users.Where(u => !u.IsDeleted).OrderBy(u => u.FullName).ToList())
                UserList.Add(new UserRoleRow(ChangeUserRole, DeleteUser)
                {
                    UserId    = u.Id,
                    FullName  = u.FullName,
                    Email     = u.Email,
                    Role      = u.OrgRole,
                    LastLogin = u.LastLogin.HasValue ? u.LastLogin.Value.ToLocalTime().ToString("dd.MM.yyyy HH:mm") : "Nigdy"
                });

            int tOpen   = ctx.Tasks.Count(t => !t.IsDeleted && t.Status == Models.Enums.TaskStatus.Open);
            int tInProg = ctx.Tasks.Count(t => !t.IsDeleted && t.Status == Models.Enums.TaskStatus.InProgress);
            int tClosed = ctx.Tasks.Count(t => !t.IsDeleted && t.Status == Models.Enums.TaskStatus.Closed);
            TaskSlices.Clear();
            TaskSlices.Add(new PieSlice { Label = "Otwarte",   Value = tOpen,   Color = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8)) });
            TaskSlices.Add(new PieSlice { Label = "W toku",    Value = tInProg, Color = new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B)) });
            TaskSlices.Add(new PieSlice { Label = "Zamknięte", Value = tClosed, Color = new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81)) });

            RefreshSprintSlices();
        }

        private void ChangeUserRole(UserRoleRow row, OrgRole newRole)
        {
            using var ctx = new AppDbContext();
            var user = ctx.Users.FirstOrDefault(u => u.Id == row.UserId);
            if (user == null || user.OrgRole == OrgRole.HeadAdmin) return;
            user.OrgRole = newRole;
            ctx.SaveChanges();
            row.Role = newRole;
        }

        private void DeleteUser(UserRoleRow row)
        {
            if (!row.CanDelete) return;
            if (MessageBox.Show($"Czy napewno usunąć konto użytkownika \"{row.FullName}\"?\nOperacja jest nieodwracalna.", "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            using var ctx = new AppDbContext();
            var user = ctx.Users.FirstOrDefault(u => u.Id == row.UserId);
            if (user == null || user.OrgRole == OrgRole.HeadAdmin) return;
            user.IsDeleted = true;
            ctx.SaveChanges();
            UserList.Remove(row);
            UsersCount--;
        }

        // ─── Lazy loaders ────────────────────────────────────────────────────────

        private void LoadProjects()
        {
            using var ctx = new AppDbContext();
            var rows = ctx.Projects
                .Where(p => !p.IsDeleted)
                .Select(p => new
                {
                    p.Name, p.Key, p.Id,
                    OwnerName    = p.Owner.FullName,
                    MembersCount = p.ProjectUsers.Count,
                    TasksCount   = p.Tasks.Count(t => !t.IsDeleted),
                    p.CreatedAt
                })
                .OrderByDescending(p => p.CreatedAt)
                .ToList();
            ProjectList.Clear();
            foreach (var r in rows)
                ProjectList.Add(new ProjectRow
                {
                    Name = r.Name, Key = r.Key, OwnerName = r.OwnerName, Id = r.Id,
                    MembersCount = r.MembersCount, TasksCount = r.TasksCount,
                    CreatedAt = r.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy")
                });
        }

        private void LoadTasks()
        {
            using var ctx = new AppDbContext();
            var rows = ctx.Tasks
                .Where(t => !t.IsDeleted)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    ProjectName  = t.Project.Name,
                    t.Status,
                    t.Priority,
                    ReporterName = t.Reporter.FullName,
                    t.CreatedAt
                })
                .OrderByDescending(t => t.CreatedAt)
                .ToList();
            TaskList.Clear();
            foreach (var r in rows)
                TaskList.Add(new TaskRow(DeleteTask)
                {
                    Id           = r.Id,
                    Title        = r.Title,
                    ProjectName  = r.ProjectName,
                    StatusLabel  = r.Status switch
                    {
                        Models.Enums.TaskStatus.InProgress => "W toku",
                        Models.Enums.TaskStatus.Closed     => "Zamknięte",
                        _                                  => "Otwarte"
                    },
                    PriorityLabel = r.Priority switch
                    {
                        TaskPriority.High => "Wysoki",
                        TaskPriority.Low  => "Niski",
                        _                 => "Średni"
                    },
                    ReporterName = r.ReporterName,
                    CreatedAt    = r.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy")
                });
        }

        private void LoadSprints()
        {
            using var ctx = new AppDbContext();
            var rows = ctx.Sprints
                .Where(s => !s.IsDeleted)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    ProjectName = s.Project.Name,
                    s.Status,
                    s.StartDate,
                    s.EndDate
                })
                .OrderByDescending(s => s.StartDate)
                .ToList();
            SprintList.Clear();
            foreach (var r in rows)
                SprintList.Add(new SprintRow(ChangeSprintStatus, DeleteSprint)
                {
                    Id          = r.Id,
                    Name        = r.Name,
                    ProjectName = r.ProjectName,
                    Status      = r.Status,
                    StartDate   = r.StartDate.ToLocalTime().ToString("dd.MM.yyyy"),
                    EndDate     = r.EndDate.ToLocalTime().ToString("dd.MM.yyyy")
                });
        }

        private void LoadComments()
        {
            using var ctx = new AppDbContext();
            var rows = ctx.Comments
                .Where(c => !c.IsDeleted)
                .Select(c => new
                {
                    c.Id,
                    AuthorName = c.Commenter.FullName,
                    TaskTitle  = c.Task.Title,
                    c.Text,
                    c.CreatedAt
                })
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
            CommentList.Clear();
            foreach (var r in rows)
                CommentList.Add(new CommentRow(DeleteComment)
                {
                    Id          = r.Id,
                    AuthorName  = r.AuthorName,
                    TaskTitle   = r.TaskTitle,
                    TextPreview = r.Text.Length > 70 ? r.Text[..70] + "…" : r.Text,
                    CreatedAt   = r.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy")
                });
        }

        private void LoadWorklogs()
        {
            using var ctx = new AppDbContext();
            var rows = ctx.Worklogs
                .Where(w => !w.IsDeleted)
                .Select(w => new
                {
                    w.Id,
                    UserName  = w.User.FullName,
                    TaskTitle = w.Task.Title,
                    w.TimeSpent,
                    w.LoggedAt
                })
                .OrderByDescending(w => w.LoggedAt)
                .ToList();
            WorklogList.Clear();
            foreach (var r in rows)
            {
                var ts = r.TimeSpent;
                WorklogList.Add(new WorklogRow(DeleteWorklog)
                {
                    Id        = r.Id,
                    UserName  = r.UserName,
                    TaskTitle = r.TaskTitle,
                    TimeSpent = ts.TotalHours >= 1 ? $"{(int)ts.TotalHours}g {ts.Minutes}m" : $"{ts.Minutes}m",
                    LoggedAt  = r.LoggedAt.ToLocalTime().ToString("dd.MM.yyyy")
                });
            }
        }

        private void LoadAttachments()
        {
            using var ctx = new AppDbContext();
            var rows = ctx.Attachments
                .Where(a => !a.IsDeleted)
                .Select(a => new
                {
                    a.FileName,
                    TaskTitle    = a.Task != null ? a.Task.Title : "—",
                    UploaderName = a.Uploader.FullName,
                    a.UploadedAt
                })
                .OrderByDescending(a => a.UploadedAt)
                .ToList();
            AttachmentList.Clear();
            foreach (var r in rows)
                AttachmentList.Add(new AttachmentRow
                {
                    FileName     = r.FileName,
                    TaskTitle    = r.TaskTitle,
                    UploaderName = r.UploaderName,
                    UploadedAt   = r.UploadedAt.ToLocalTime().ToString("dd.MM.yyyy")
                });
        }

        private void LoadDocs()
        {
            using var ctx = new AppDbContext();
            var rows = ctx.WikiArticles
                .Select(a => new
                {
                    a.Title,
                    ProjectName = a.Project.Name,
                    a.UpdatedAt
                })
                .OrderByDescending(a => a.UpdatedAt)
                .ToList();
            DocList.Clear();
            foreach (var r in rows)
                DocList.Add(new DocRow
                {
                    Title       = r.Title,
                    ProjectName = r.ProjectName,
                    UpdatedAt   = r.UpdatedAt.ToLocalTime().ToString("dd.MM.yyyy")
                });
        }

        // ─── Action handlers ─────────────────────────────────────────────────────

        private void ChangeSprintStatus(SprintRow row, SprintStatus newStatus)
        {
            using var ctx = new AppDbContext();
            var sprint = ctx.Sprints.FirstOrDefault(s => s.Id == row.Id);
            if (sprint == null) return;
            sprint.Status = newStatus;
            ctx.SaveChanges();
            row.Status = newStatus;
            RefreshSprintSlices();
        }

        private void DeleteSprint(SprintRow row)
        {
            if (MessageBox.Show($"Czy napewno usunąć sprint \"{row.Name}\"?", "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            using var ctx = new AppDbContext();
            var sprint = ctx.Sprints.FirstOrDefault(s => s.Id == row.Id);
            if (sprint == null) return;
            sprint.IsDeleted = true;
            ctx.SaveChanges();
            SprintList.Remove(row);
            SprintsCount--;
            RefreshSprintSlices();
        }

        private void DeleteTask(TaskRow row)
        {
            if (MessageBox.Show($"Czy napewno usunąć task \"{row.Title}\"?", "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            using var ctx = new AppDbContext();
            var task = ctx.Tasks.FirstOrDefault(t => t.Id == row.Id);
            if (task == null) return;
            task.IsDeleted = true;
            ctx.SaveChanges();
            TaskList.Remove(row);
            TasksCount--;
        }

        private void DeleteComment(CommentRow row)
        {
            if (MessageBox.Show("Czy napewno usunąć ten komentarz?", "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            using var ctx = new AppDbContext();
            var comment = ctx.Comments.FirstOrDefault(c => c.Id == row.Id);
            if (comment == null) return;
            comment.IsDeleted = true;
            ctx.SaveChanges();
            CommentList.Remove(row);
            CommentsCount--;
        }

        private void DeleteWorklog(WorklogRow row)
        {
            if (MessageBox.Show("Czy napewno usunąć ten wpis czasu pracy?", "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            using var ctx = new AppDbContext();
            var worklog = ctx.Worklogs.FirstOrDefault(w => w.Id == row.Id);
            if (worklog == null) return;
            worklog.IsDeleted = true;
            ctx.SaveChanges();
            WorklogList.Remove(row);
            WorklogsCount--;
        }

        private void RefreshSprintSlices()
        {
            using var ctx = new AppDbContext();
            int sPlanned   = ctx.Sprints.Count(s => !s.IsDeleted && s.Status == SprintStatus.Planned);
            int sActive    = ctx.Sprints.Count(s => !s.IsDeleted && s.Status == SprintStatus.Active);
            int sCompleted = ctx.Sprints.Count(s => !s.IsDeleted && s.Status == SprintStatus.Completed);
            SprintSlices.Clear();
            SprintSlices.Add(new PieSlice { Label = "Planowane",  Value = sPlanned,   Color = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8)) });
            SprintSlices.Add(new PieSlice { Label = "Aktywne",    Value = sActive,    Color = new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6)) });
            SprintSlices.Add(new PieSlice { Label = "Ukończone",  Value = sCompleted, Color = new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81)) });
        }
    }
}
