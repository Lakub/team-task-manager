using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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
    public partial class SprintsOverviewViewModel : ObservableObject
    {
        private readonly IProjectService _projectService;
        private int _projectId;

        public SprintsOverviewViewModel(IProjectService projectService, int projectId)
        {
            _projectService = projectService;
            _projectId = projectId;

            OpenSprintReportCommand = new RelayCommand<SprintItem>(OpenSprintReport);
            CreateSprintCommand = new RelayCommand(CreateSprint);
            StartSprintCommand = new AsyncRelayCommand<SprintItem>(StartSprint);
            EndSprintCommand = new AsyncRelayCommand<SprintItem>(EndSprint);
        }

        public async System.Threading.Tasks.Task InitializeAsync()
        {
            if (_projectId < 0)
            {
                var projects = await _projectService.GetAllProjectsWithProjectUsersAsync();
                var currentUser = App.CurrentUser;

                var userProjects = projects.Where(p => p.ProjectUsers.Any(pu => pu.UserId == currentUser?.Id)).ToList();
                _projectId = userProjects.FirstOrDefault()?.Id ?? -1;

                if (_projectId < 0)
                {
                    MessageBox.Show("TEMP nie ma przypisanych projektów.", "Brak projektu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            await LoadSprintsAsync();
        }


        // uprawnienia
        public bool CanManageProject { get; set; }


        // dane projektu
        public string ProjectName { get; set; } = "";


        // sprinty
        public ObservableCollection<SprintItem> Sprints { get; } = new();
        public bool HasSprints => Sprints.Any();

        public async System.Threading.Tasks.Task LoadSprintsAsync()
        {
            var (project, sprints) = await _projectService.GetSprintsByProjectIdAsync(_projectId);

            if (project == null) return;

            CanManageProject = UserHelper.HasAdminPowers() ||
                               project.ProjectUsers.Any(pu => pu.UserId == App.CurrentUser?.Id
                                                           && (pu.Role == UserRole.Manager || pu.Role == UserRole.Owner));

            ProjectName = project.Name;

            Sprints.Clear();
            foreach (var s in sprints)
            {
                Sprints.Add(new SprintItem
                {
                    Id = s.Id,
                    SprintName = s.Name,
                    CreatorName = s.Creator.FullName,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    Status = s.Status,
                    DoneTasks = s.SprintTasks.Count(st => st.Status == TaskStatus.Closed && !st.RemovedAt.HasValue),
                    TotalTasks = s.SprintTasks.Count(st => !st.RemovedAt.HasValue)
                });
            }

            OnPropertyChanged(string.Empty);
        }


        // otwieranie raportu
        public ICommand OpenSprintReportCommand { get; }
        private void OpenSprintReport(SprintItem? sprintItem)
        {
            if (sprintItem == null) return;

            var reportView = new SprintReportView(sprintItem.Id, _projectId);
            WeakReferenceMessenger.Default.Send(new NavigationMessage(reportView));
        }


        // tworzenie sprintu
        public ICommand CreateSprintCommand { get; }
        private void CreateSprint()
        {
            if (!CanManageProject)
            {
                MessageBox.Show("Nie masz uprawnień do tej akcji.", "Brak uprawnień", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var createTaskWindow = new CreateSprintWindow(_projectId);
            if (createTaskWindow.ShowDialog() == true)
            {
                _ = LoadSprintsAsync();
            }
        }


        // rozpoczynanie sprintu
        public ICommand StartSprintCommand { get; }
        private async Task StartSprint(SprintItem? sprintItem)
        {
            if (sprintItem == null) return;

            if (!CanManageProject)
            {
                MessageBox.Show("Nie masz uprawnień do tej akcji.", "Brak uprawnień", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // czy istnieje już aktywny sprint
            if (Sprints.Any(s => s.IsActive))
            {
                MessageBox.Show("Zakończ aktywny sprint przed rozpoczęciem nowego.", "Aktywny sprint", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new StartSprintWindow(_projectId, sprintItem.EndDate);
            if (dialog.ShowDialog() != true) return;

            if (dialog.SelectedEndDate <= DateTime.Today)
            {
                MessageBox.Show("Data zakończenia musi być późniejsza niż dzisiaj.", "Nieprawidłowa data", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await _projectService.StartSprintAsync(sprintItem.Id, dialog.SelectedEndDate);
            await LoadSprintsAsync();
        }


        // kończenie sprintu
        public ICommand EndSprintCommand { get; }
        private async Task EndSprint(SprintItem? sprintItem)
        {
            if (sprintItem == null) return;

            if (!CanManageProject)
            {
                MessageBox.Show("Nie masz uprawnień do tej akcji.", "Brak uprawnień", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Czy na pewno chcesz zakończyć sprint \"{sprintItem.SprintName}\"?", "Zakończ sprint",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            await _projectService.EndSprintAsync(sprintItem.Id);
            await LoadSprintsAsync();
        }
    }

    public class SprintItem : ObservableObject
    {
        // sprint
        public int Id { get; set; }
        public string SprintName { get; set; } = "";

        // tworca
        public string CreatorName { get; set; } = "";
        public string CreatorInitials => string.Join("", CreatorName.Split(' ').Select(n => n[0])).ToUpper();
        public Brush CreatorAvatarBg { get; set; } = new SolidColorBrush(Color.FromRgb(224, 231, 255));
        public Brush CreatorAvatarFg { get; set; } = new SolidColorBrush(Color.FromRgb(67, 56, 202));

        // daty
        public DateTime? StartDate { get; set; }
        public string StartDateStr => StartDate?.ToString("dd.MM.yyyy") ?? "-";
        public DateTime? EndDate { get; set; }
        public string EndDateStr => EndDate?.ToString("dd.MM.yyyy") ?? "-";
        public int DaysRemaining => EndDate.HasValue ? Math.Max(0, ((DateTime)EndDate - DateTime.Today).Days) : 0;

        // status
        public SprintStatus Status { get; set; }
        public string StatusText => StyleHelper.GetSprintStatusDisplay(Status);
        public Brush StatusBg => StyleHelper.GetSprintStatusStyle(Status).Bg;
        public Brush StatusFg => StyleHelper.GetSprintStatusStyle(Status).Fg;
        public bool IsActive => Status == SprintStatus.Active;
        public bool IsPlanned => Status == SprintStatus.Planned;
        public bool IsCompleted => Status == SprintStatus.Completed;

        // progres
        public int TotalTasks { get; set; }
        public int DoneTasks { get; set; }
        public int RemainingTasks => Math.Max(0, TotalTasks - DoneTasks);
        public double ProgressPercent => TotalTasks == 0 ? 0 : (double)DoneTasks / TotalTasks * 100;
        public string ProgressText => $"{DoneTasks} / {TotalTasks} zadań ukończonych ({ProgressPercent:0}%)";
    }

}