using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
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
    public class SprintTaskItem : INotifyPropertyChanged
    {
        private readonly SprintTask _model;
        private readonly DateTime _sprintStart;
        private readonly DateTime _sprintEnd;

        public SprintTaskItem(SprintTask model, DateTime sprintStart, DateTime sprintEnd)
        {
            _model = model;
            _sprintStart = sprintStart;
            _sprintEnd = sprintEnd;
        }

        public int TaskId => _model.TaskId;
        public string Title => _model.Task.Title;
        public int PerProjectId => _model.Task.PerProjectId;
        public string Key => $"{_model.Task.Project.Key}-{_model.Task.PerProjectId}";
        public string KeyAndTitle => $"{Key} - {Title}";
        public TaskType Type => _model.Task.Type;
        public TaskPriority Priority => _model.Task.Priority;
        public bool IsAssigned => _model.AssigneeId.HasValue;
        public double? HoursSpent
        {
            get
            {
                return _model.Task.Worklogs?
                    .Where(w =>
                        w.StartTime >= _sprintStart // praca zaczeta po starcie sprintu
                        && w.LoggedAt <= _sprintEnd // praca zlogowana przed koncem sprintu
                    )
                    .Sum(w => w.TimeSpent.TotalHours);
            }
        }

        public TaskStatus Status => _model.Status;

        public ScopeChange Scope
        {
            get
            {
                if (_model.RemovedAt.HasValue) return ScopeChange.Descoped;     // jest data usuniecia => zdescopowany
                if (_model.AddedAt > _sprintStart) return ScopeChange.Added;    // data dodania jest po starcie sprintu => dodany w trakcie
                return ScopeChange.None;                                        // _ => bez zmian
            }
        }

        // pomocnicze
        public string TypeDisplay => Type.ToString();
        public string PriorityDisplay => Priority.ToString();
        public string TimeSpent => HoursSpent > 0 ? $"{HoursSpent:0.#}h" : "-";

        public Brush StatusColor => Status switch
        {
            TaskStatus.Closed => new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81)),      // zielony
            TaskStatus.InProgress => new SolidColorBrush(Color.FromRgb(0x60, 0xA5, 0xFA)),  // niebieski
            TaskStatus.Open => new SolidColorBrush(Color.FromRgb(0xD1, 0xD5, 0xDB)),        // szary
            _ => new SolidColorBrush(Color.FromRgb(0xD1, 0xD5, 0xDB))                       // szary
        };

        public Brush PriorityColor => Priority switch
        {
            TaskPriority.High => new SolidColorBrush(Color.FromRgb(0xB9, 0x1C, 0x1C)),      // czerwony
            TaskPriority.Medium => new SolidColorBrush(Color.FromRgb(0xB4, 0x53, 0x09)),    // pomarańczowy
            TaskPriority.Low => new SolidColorBrush(Color.FromRgb(0x16, 0xA3, 0x9C)),       // morski
            _ => new SolidColorBrush(Color.FromRgb(0x2B, 0x2B, 0x2B))                       // szary
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

        public Visibility ScopeBadgeVisibility => Scope == ScopeChange.None ? Visibility.Collapsed : Visibility.Visible;    // nie pokazujemy badge jesli nie bylo zmiany
        public string ScopeBadgeText => Scope switch
        {
            ScopeChange.Added => "+dodane",
            ScopeChange.Descoped => "descoped",
            _ => ""
        };

        public Brush ScopeBadgeBg => Scope switch
        {
            ScopeChange.Added => new SolidColorBrush(Color.FromRgb(0xFE, 0xF3, 0xC7)),      // pomaranczowy
            ScopeChange.Descoped => new SolidColorBrush(Color.FromRgb(0xFE, 0xE2, 0xE2)),   // czerwony
            _ => new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0))                       // szary
        };
        public Brush ScopeBadgeFg => Scope switch
        {
            ScopeChange.Added => new SolidColorBrush(Color.FromRgb(0x92, 0x40, 0x0E)),      // pomaranczowy
            ScopeChange.Descoped => new SolidColorBrush(Color.FromRgb(0x99, 0x1B, 0x1B)),   // czerwony
            _ => new SolidColorBrush(Color.FromRgb(0x2B, 0x2B, 0x2B))                       // szary
        };

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    public class TeamMemberItem : ObservableObject
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public double HoursLogged { get; set; }
        public int DoneCount { get; set; }
        public int InProgressCount { get; set; }
        public bool IsCurrentUser { get; set; }
        public Visibility YouBadgeVisibility => IsCurrentUser ? Visibility.Visible : Visibility.Collapsed;

        // TYMCZASOWY AVATRA
        public string Initials => string.Concat(FullName.Split(' ').Select(n => n[0])).ToUpper();
        public Brush AvatarBg { get; set; } = new SolidColorBrush(Color.FromRgb(224, 231, 255));
        public Brush AvatarFg { get; set; } = new SolidColorBrush(Color.FromRgb(67, 56, 202));
    }

    public partial class SprintReportViewModel : ObservableObject
    {
        private readonly ISprintService _sprintService;

        private int _sprintId;
        private int _projectId;
        private string ProjectKey { get; set; } = "";

        private readonly List<SprintTaskItem> _allTaskItems = new();

        public Action<SprintTaskItem>? OnTaskSelected { get; set; }
        public ICommand OpenTaskCommand { get; }

        public Action<TeamMemberItem>? OnTeamMemberSelected { get; set; }
        public ICommand OpenUserProfileCommand { get; }
        public ICommand OpenSprintBacklogCommand { get; }

        public bool CanManageProject { get; set; }
        public bool CanEditSprint => (IsPlanned || IsActive) && CanManageProject;

        public string SprintName { get; set; } = "";
        public string ProjectName { get; set; } = "";
        public string CreatorName { get; set; } = "";
        public string CreatorInitials => string.IsNullOrWhiteSpace(CreatorName) ? "?" : string.Concat(CreatorName.Split(' ').Select(n => n[0])).ToUpper();
        public Brush CreatorAvatarBg { get; set; } = new SolidColorBrush(Color.FromRgb(224, 231, 255));
        public Brush CreatorAvatarFg { get; set; } = new SolidColorBrush(Color.FromRgb(67, 56, 202));
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsPlanned { get; set; }
        public Visibility DaysRemainingVisibility => IsActive ? Visibility.Visible : Visibility.Collapsed;
        public string LongestTaskTime { get; set; } = "-";
        public string LongestTaskKey { get; set; } = "";
        public string AvgTaskTime { get; set; } = "-";

        public string StatusText => IsActive ? "W toku" : IsPlanned ? "Planowany" : "Zakończony";
        public string StartDateStr => StartDate.ToString("dd.MM.yyyy");
        public string EndDateStr => EndDate.ToString("dd.MM.yyyy");
        public int DaysRemaining => Math.Max(0, (EndDate - DateTime.Today).Days);

        public int TotalTasks => _allTaskItems.Count(t => t.Scope != ScopeChange.Descoped);
        public int DoneTasks => _allTaskItems.Count(t => t.Status == TaskStatus.Closed && t.Scope != ScopeChange.Descoped);
        public int RemainingTasks => Math.Max(0, TotalTasks - DoneTasks);
        public int InProgressTasks => _allTaskItems.Count(t => t.Status == TaskStatus.InProgress && t.Scope != ScopeChange.Descoped);
        public int TodoTasks => _allTaskItems.Count(t => t.Status == TaskStatus.Open && t.Scope != ScopeChange.Descoped);
        public int DescoppedTasks => _allTaskItems.Count(t => t.Scope == ScopeChange.Descoped);
        public int ScopeCreepCount => _allTaskItems.Count(t => t.Scope == ScopeChange.Added);
        public int UnassignedTasks => _allTaskItems.Count(t => !t.IsAssigned && t.Scope != ScopeChange.Descoped && t.Status != TaskStatus.Closed);

        public double ProgressPercent => TotalTasks == 0 ? 0 : (double)DoneTasks / TotalTasks * 100;
        public string ProgressText => $"{DoneTasks} / {TotalTasks} zadań ukończonych ({ProgressPercent:0}%)";

        public int FeatureCount => _allTaskItems.Count(i => i.Type == TaskType.Feature && i.Scope != ScopeChange.Descoped);
        public int BugCount => _allTaskItems.Count(i => i.Type == TaskType.Bug && i.Scope != ScopeChange.Descoped);
        public int TaskCount => _allTaskItems.Count(i => i.Type == TaskType.Task && i.Scope != ScopeChange.Descoped);

        public int NonFeatureCount => Math.Max(0, TotalTasks - FeatureCount);
        public int NonBugCount => Math.Max(0, TotalTasks - BugCount);
        public int NonTaskCount => Math.Max(0, TotalTasks - TaskCount);

        public int HighPrioCount => _allTaskItems.Count(i => i.Priority == TaskPriority.High && i.Scope != ScopeChange.Descoped);
        public int LowPrioCount => _allTaskItems.Count(i => i.Priority == TaskPriority.Low && i.Scope != ScopeChange.Descoped);
        public int MediumPrioCount => _allTaskItems.Count(i => i.Priority == TaskPriority.Medium && i.Scope != ScopeChange.Descoped);

        public int NonHighPrioCount => Math.Max(0, TotalTasks - HighPrioCount);
        public int NonMediumPrioCount => Math.Max(0, TotalTasks - MediumPrioCount);
        public int NonLowPrioCount => Math.Max(0, TotalTasks - LowPrioCount);

        private ObservableCollection<SprintTaskItem> _tasks = new();
        public ObservableCollection<SprintTaskItem> FilteredTasks { get; } = new();

        private ObservableCollection<TeamMemberItem> _teamMembers = new();
        public ObservableCollection<TeamMemberItem> TeamMembers
        {
            get => _teamMembers;
            set { _teamMembers = value; OnPropertyChanged(); }
        }

        public SprintReportViewModel(ISprintService sprintService, int sprintId, int projectId)
        {
            _sprintService = sprintService;
            _sprintId = sprintId;
            _projectId = projectId;

            OpenTaskCommand = new RelayCommand<SprintTaskItem?>(OpenTask);
            OpenUserProfileCommand = new RelayCommand<TeamMemberItem?>(OpenUserProfile);
            OpenSprintBacklogCommand = new RelayCommand(OpenSprintBacklog);
            ToggleSortCommand = new RelayCommand(() => SortAscending = !SortAscending);
        }

        public async System.Threading.Tasks.Task InitializeAsync()
        {
            if (_sprintId < 0)
            {
                var sprints = await _sprintService.GetAllSprintsByProjectIdAsync(_projectId);
                
                // aktywny sprint, jesli nie ma to ostatni zaczety, jesli nie ma to info o braku sprinta
                var targetSprint = sprints.FirstOrDefault(s => s.Status == SprintStatus.Active)
                                ?? sprints.Where(s => s.Status != SprintStatus.Planned).OrderByDescending(s => s.StartDate).FirstOrDefault()
                                ?? sprints.FirstOrDefault();

                if (targetSprint == null)
                {
                    MessageBox.Show("Brak sprintów w projekcie.");
                    var backlogView = new BacklogView(-1, _projectId);
                    WeakReferenceMessenger.Default.Send(new NavigationMessage(backlogView));
                    return;
                }

                _sprintId = targetSprint.Id;
            }

            var (sprint, sprintTasks) = await _sprintService.GetSprintReportDataAsync(_sprintId);

            if (sprint == null) return;

            _projectId = sprint.ProjectId;

            ProjectKey = sprint.Project.Key;
            SprintName = sprint.Name;
            ProjectName = sprint.Project.Name;
            CreatorName = sprint.Creator.FullName;
            StartDate = sprint.StartDate;
            EndDate = sprint.EndDate;
            IsActive = sprint.Status == SprintStatus.Active;
            IsPlanned = sprint.Status == SprintStatus.Planned;

            CanManageProject = UserHelper.HasAdminPowers() ||
                               sprint.Project.ProjectUsers.Any(pu => pu.UserId == App.CurrentUser?.Id
                                                           && (pu.Role == UserRole.Manager || pu.Role == UserRole.Owner));

            _allTaskItems.Clear();
            foreach (var st in sprintTasks)
            {
                _allTaskItems.Add(new SprintTaskItem(st, StartDate, EndDate));
            }

            OnPropertyChanged(string.Empty);
            ApplyFilterSort();

            LoadTeamStats(sprintTasks);
        }

        private void LoadTeamStats(List<SprintTask> sprintTasks)
        {
            var worklogs = sprintTasks
                .Where(st => st.Task.Worklogs != null)
                .SelectMany(st => st.Task.Worklogs)
                .ToList();

            // oddzielnie bo tutaj bardziej chcemy czas na taski, nie przecietny czas pracy w sprincie, bo to jest nizej w team statach
            if (worklogs.Any())
            {
                var longest = worklogs.OrderByDescending(w => w.TimeSpent).First();
                LongestTaskTime = $"{longest.TimeSpent.TotalHours:0.#}h";
                LongestTaskKey = $"{ProjectKey}-{longest.Task.PerProjectId}";
                var avgTime = worklogs.Average(w => w.TimeSpent.TotalHours);
                AvgTaskTime = $"{avgTime:0.#}h";
            }

            // worklogi z czasu trwania sprintu
            var worklogsDuringSprint = worklogs.Where(w =>
                w.StartTime >= StartDate    // praca zaczeta po starcie sprintu
                && w.LoggedAt <= EndDate    // praca zlogowana przed koncem sprintu
            ).ToList();

            // uzytkownicy, ktorzy logowali prace w sprincie
            var worklogUsers = worklogsDuringSprint
                .Where(w => w.User != null)
                .Select(w => w.User);

            // uzytkownicy, ktorzy mieli przydzielone zadania
            var assigneeUsers = sprintTasks
                .Where(st => st.Task?.Assignee != null)
                .Select(st => st.Task.Assignee!);

            // unikalne z obu
            var distinctUsers = worklogUsers
                .Union(assigneeUsers)
                .GroupBy(u => u.Id)
                .Select(g => g.First())
                .ToList();

            // mapowanko userId do sumy godzin pracy w sprincie
            var worklogsByUser = worklogsDuringSprint
                .GroupBy(w => w.UserId)
                .ToDictionary(g => g.Key, g => g.Sum(w => w.TimeSpent.TotalHours));

            // mapowanko userId do listy zadan przydzielonych w sprincie
            var tasksByUser = sprintTasks
                .Where(st => st.AssigneeId.HasValue)
                .GroupBy(st => st.AssigneeId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            TeamMembers.Clear();
            foreach (var user in distinctUsers)
            {
                worklogsByUser.TryGetValue(user.Id, out var hours);
                tasksByUser.TryGetValue(user.Id, out var assignedToThisUser);
                // jak nie znalazlo to pusta
                assignedToThisUser ??= new List<SprintTask>();

                int done = assignedToThisUser.Count(t => t.Status == TaskStatus.Closed && !t.RemovedAt.HasValue);
                int inProgress = assignedToThisUser.Count(t => t.Status == TaskStatus.InProgress && !t.RemovedAt.HasValue);

                var member = new TeamMemberItem
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    HoursLogged = Math.Round(hours, 1),
                    DoneCount = done,
                    InProgressCount = inProgress,
                    IsCurrentUser = user.Id == App.CurrentUser?.Id
                };

                TeamMembers.Add(member);
            }

            OnPropertyChanged(string.Empty);
        }

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
            new() { Label = "Zmiana scope", Value = "Scope" },
            new() { Label = "Błędy", Value = "Bug" },
            new() { Label = "Funkcje", Value = "Feature" },
            new() { Label = "Zadania", Value = "Task" },
            new() { Label = "Wysoki priorytet", Value = "High" },
            new() { Label = "Średni priorytet", Value = "Medium" },
            new() { Label = "Niski priorytet", Value = "Low" },
        };

        public ICommand ToggleSortCommand { get; }

        private void ApplyFilterSort()
        {
            IEnumerable<SprintTaskItem> result = _allTaskItems;

            result = _selectedFilter switch
            {
                "Open" => result.Where(t => t.Status == TaskStatus.Open),
                "InProgress" => result.Where(t => t.Status == TaskStatus.InProgress),
                "Closed" => result.Where(t => t.Status == TaskStatus.Closed),
                "Scope" => result.Where(t => t.Scope != ScopeChange.None),
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

        public void OpenTask(SprintTaskItem? item)
        {
            if (item != null) OnTaskSelected?.Invoke(item);
        }

        private void OpenSprintBacklog()
        {
            var backlogView = new BacklogView(_sprintId, _projectId);
            WeakReferenceMessenger.Default.Send(new NavigationMessage(backlogView));
        }

        public void OpenUserProfile(TeamMemberItem? item)
        {
            if (item != null) OnTeamMemberSelected?.Invoke(item);
        }
    }
}