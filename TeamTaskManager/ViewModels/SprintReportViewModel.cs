using CommunityToolkit.Mvvm.Input;
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
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Models.Enums;
using TeamTaskManager.Services;
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

        public string Title => _model.Task.Title;
        public TaskType Type => _model.Task.Type;
        public TaskPriority Priority => _model.Task.Priority;
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
            TaskStatus.Closed => new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81)),
            TaskStatus.InProgress => new SolidColorBrush(Color.FromRgb(0x60, 0xA5, 0xFA)),
            _ => new SolidColorBrush(Color.FromRgb(0xD1, 0xD5, 0xDB))
        };

        public Brush PriorityColor => Priority switch
        {
            TaskPriority.High => new SolidColorBrush(Color.FromRgb(0xB9, 0x1C, 0x1C)),
            TaskPriority.Medium => new SolidColorBrush(Color.FromRgb(0xB4, 0x53, 0x09)),
            _ => new SolidColorBrush(Color.FromRgb(0x06, 0x5F, 0x46))
        };

        public Brush TypeBadgeBg => Type == TaskType.Bug ? new SolidColorBrush(Color.FromRgb(0xFE, 0xF2, 0xF2)) : new SolidColorBrush(Color.FromRgb(0xEF, 0xF6, 0xFF));         // bug => czerwony, feature => niebieski
        public Brush TypeBadgeFg => Type == TaskType.Bug ? new SolidColorBrush(Color.FromRgb(0xB9, 0x1C, 0x1C)) : new SolidColorBrush(Color.FromRgb(0x1D, 0x4E, 0xD8));

        public Visibility ScopeBadgeVisibility => Scope == ScopeChange.None ? Visibility.Collapsed : Visibility.Visible;                                                        // nie pokazujemy badge jesli nie bylo zmiany
        public string ScopeBadgeText => Scope == ScopeChange.Added ? "+dodane" : "descoped";
        public Brush ScopeBadgeBg => Scope == ScopeChange.Added ? new SolidColorBrush(Color.FromRgb(0xFE, 0xF3, 0xC7)) : new SolidColorBrush(Color.FromRgb(0xFE, 0xE2, 0xE2));  // dodany => pomaranczowy, descoped => czerwony
        public Brush ScopeBadgeFg => Scope == ScopeChange.Added ? new SolidColorBrush(Color.FromRgb(0x92, 0x40, 0x0E)) : new SolidColorBrush(Color.FromRgb(0x99, 0x1B, 0x1B));

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    public class TeamMemberItem : INotifyPropertyChanged
    {
        public string FullName { get; set; } = string.Empty;
        public double HoursLogged { get; set; }
        public int DoneCount { get; set; }
        public int InProgressCount { get; set; }

        // TYMCZASOWY AVATRA
        public string Initials => string.Concat(FullName.Split(' ').Select(n => n[0])).ToUpper();
        public Brush AvatarBg { get; set; } = new SolidColorBrush(Color.FromRgb(224, 231, 255));
        public Brush AvatarFg { get; set; } = new SolidColorBrush(Color.FromRgb(67, 56, 202));

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public partial class SprintReportViewModel : INotifyPropertyChanged
    {
        private readonly ISprintService _sprintService;
        private readonly IUserService _userService;
        public ICommand FilterCommand { get; }

        private int _sprintId;  // TYMCZASOWO USUNIETO READONLY

        private readonly List<SprintTaskItem> _allTaskItems = new();

        public string SprintName { get; set; }
        public string ProjectName { get; set; }
        public string CreatorName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsPlanned { get; set; }

        private string _avgTaskTime = "-";
        public string AvgTaskTime
        {
            get => _avgTaskTime;
            set
            {
                if (_avgTaskTime != value)
                {
                    _avgTaskTime = value;
                    OnPropertyChanged(nameof(AvgTaskTime));
                }
            }
        }

        private string _longestTaskTime = "-";
        public string LongestTaskTime
        {
            get => _longestTaskTime;
            set
            {
                if (_longestTaskTime != value)
                {
                    _longestTaskTime = value;
                    OnPropertyChanged(nameof(LongestTaskTime));
                }
            }
        }

        private string _longestTaskName = "-";
        public string LongestTaskName
        {
            get => _longestTaskName;
            set
            {
                if (_longestTaskName != value)
                {
                    _longestTaskName = value;
                    OnPropertyChanged(nameof(LongestTaskName));
                }
            }
        }

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

        public double ProgressPercent => TotalTasks == 0 ? 0 : (double)DoneTasks / TotalTasks * 100;
        public string ProgressText => $"{DoneTasks} / {TotalTasks} zadań ukończonych ({ProgressPercent:0}%)";

        public int FeatureCount => _allTaskItems.Count(i => i.Type == TaskType.Feature && i.Scope != ScopeChange.Descoped);
        public int BugCount => _allTaskItems.Count(i => i.Type == TaskType.Bug && i.Scope != ScopeChange.Descoped);
        public int TaskCount => _allTaskItems.Count(i => i.Type == TaskType.Task && i.Scope != ScopeChange.Descoped);

        public int NonFeatureCount => Math.Max(0, TotalTasks - FeatureCount);
        public int NonBugCount => Math.Max(0, TotalTasks - BugCount);
        public int NonTaskCount => Math.Max(0, TotalTasks - NonFeatureCount);

        public int HighPrioCount => _allTaskItems.Count(i => i.Priority == TaskPriority.High && i.Scope != ScopeChange.Descoped);
        public int LowPrioCount => _allTaskItems.Count(i => i.Priority == TaskPriority.Low && i.Scope != ScopeChange.Descoped);
        public int MediumPrioCount => _allTaskItems.Count(i => i.Priority == TaskPriority.Medium && i.Scope != ScopeChange.Descoped);

        public int NonHighPrioCount => Math.Max(0, TotalTasks - HighPrioCount);
        public int NonMediumPrioCount => Math.Max(0, TotalTasks - MediumPrioCount);
        public int NonLowPrioCount => Math.Max(0, TotalTasks - LowPrioCount);

        private ObservableCollection<SprintTaskItem> _tasks = new();
        public ObservableCollection<SprintTaskItem> Tasks {
            get => _tasks;
            set { _tasks = value; OnPropertyChanged(); }
        }

        private ObservableCollection<TeamMemberItem> _teamMembers = new();
        public ObservableCollection<TeamMemberItem> TeamMembers
        {
            get => _teamMembers;
            set { _teamMembers = value; OnPropertyChanged(); }
        }

        public SprintReportViewModel(ISprintService sprintService, IUserService userService, int sprintId)
        {
            _sprintService = sprintService;
            _userService = userService;
            _sprintId = sprintId;

            FilterCommand = new RelayCommand<string?>(ApplyFilter);
        }

        public async System.Threading.Tasks.Task InitializeAsync()
        {
            // TYMCZASOWE LADOWANIE PIERWSZEFGO LESPSZEGO
            if (_sprintId < 0)
            {
                var sprints = await _sprintService.GetAllSprintsAsync();
                _sprintId = sprints.FirstOrDefault()?.Id ?? 0;
            }

            var (sprint, sprintTasks) = await _sprintService.GetSprintReportDataAsync(_sprintId);

            if (sprint == null) return;

            SprintName = sprint.Name;
            ProjectName = sprint.Project.Name;
            CreatorName = sprint.Creator.FullName;
            StartDate = sprint.StartDate;
            EndDate = sprint.EndDate;
            IsActive = sprint.Status == SprintStatus.Active;

            _allTaskItems.Clear();
            foreach (var st in sprintTasks)
            {
                _allTaskItems.Add(new SprintTaskItem(st, StartDate, EndDate));
            }

            OnPropertyChanged(string.Empty);
            ApplyFilter(null);

            await LoadTeamStatsAsync(sprintTasks);
        }

        private async System.Threading.Tasks.Task LoadTeamStatsAsync(List<SprintTask> sprintTasks)
        {
            var worklogs = sprintTasks
                .Where(st => st.Task.Worklogs != null)
                .SelectMany(st => st.Task.Worklogs);

            // oddzielnie bo tutaj bardziej chcemy czas na taski, nie przecietny czas pracy w sprincie, bo to jest nizej w team statach
            if (worklogs.Any())
            {
                var longest = worklogs.OrderByDescending(w => w.TimeSpent).First();
                LongestTaskTime = $"{longest.TimeSpent.TotalHours:0.#}h";
                LongestTaskName = longest.Task.Title;
                var avgTime = worklogs.Average(w => w.TimeSpent.TotalHours);
                AvgTaskTime = $"{avgTime:0.#}h";
            }

            var worklogsDuringSprint = worklogs.Where(w =>
                w.StartTime >= StartDate    // praca zaczeta po starcie sprintu
                && w.LoggedAt <= EndDate    // praca zlogowana przed koncem sprintu
            );

            var worklogsUserIds = worklogsDuringSprint.Select(w => w.UserId).Distinct();

            var assigneeIds = sprintTasks
                .Where(st => st.AssigneeId.HasValue)
                .Select(st => st.AssigneeId!.Value);

            var userIds = worklogsUserIds.Union(assigneeIds).Distinct();

            TeamMembers.Clear();
            foreach (var userId in userIds)
            {
                var user = await _userService.GetByIdAsync(userId);

                var hours = worklogsDuringSprint
                    .Where(w => w.UserId == userId)
                    .Sum(w => w.TimeSpent.TotalHours);

                var assignedToThisUser = sprintTasks.Where(st => st.AssigneeId == userId).ToList();
                int done = assignedToThisUser.Count(
                    t => t.Status == TaskStatus.Closed
                    && !t.RemovedAt.HasValue
                );
                int inProgress = assignedToThisUser.Count(
                    t => t.Status == TaskStatus.InProgress
                    && !t.RemovedAt.HasValue
                );

                var member = new TeamMemberItem
                {
                    FullName = user.FullName,
                    HoursLogged = Math.Round(hours, 1),
                    DoneCount = done,
                    InProgressCount = inProgress
                };

                TeamMembers.Add(member);
            }
        }

        public void ApplyFilter(string? filter)
        {
            var filtered = filter switch
            {
                "done" => _allTaskItems.Where(t => t.Status == TaskStatus.Closed && t.Scope != ScopeChange.Descoped),
                "inprogress" => _allTaskItems.Where(t => t.Status == TaskStatus.InProgress && t.Scope != ScopeChange.Descoped),
                "scope" => _allTaskItems.Where(t => t.Scope != ScopeChange.None),
                _ => _allTaskItems
            };
            Tasks = new ObservableCollection<SprintTaskItem>(filtered);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}