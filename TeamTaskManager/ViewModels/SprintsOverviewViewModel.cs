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
using System.Windows.Navigation;
using TeamTaskManager.Helpers;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Models.Enums;
using TeamTaskManager.Services;
using TeamTaskManager.Views;
using TaskStatus = TeamTaskManager.Models.Enums.TaskStatus;

namespace TeamTaskManager.ViewModels
{
    public class SprintItem : ObservableObject
    {
        public int Id { get; set;  }
        public string SprintName { get; set; } = "";
        public string CreatorName { get; set; } = "";
        public string CreatorInitials => string.Join("", CreatorName.Split(' ').Select(n => n[0])).ToUpper();
        public Brush CreatorAvatarBg { get; set; } = new SolidColorBrush(Color.FromRgb(224, 231, 255));
        public Brush CreatorAvatarFg { get; set; } = new SolidColorBrush(Color.FromRgb(67, 56, 202));
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public Visibility DaysRemainingVisibility => IsActive ? Visibility.Visible : Visibility.Collapsed;
        public bool IsPlanned { get; set; }
        public string StatusText => IsActive ? "W toku" : IsPlanned ? "Planowany" : "Zakończony";
        public string StartDateStr => StartDate.ToString("dd.MM.yyyy");
        public string EndDateStr => EndDate.ToString("dd.MM.yyyy");
        public int DaysRemaining => Math.Max(0, (EndDate - DateTime.Today).Days);
        public int TotalTasks { get; set; }
        public int DoneTasks { get; set; }
        public int RemainingTasks => Math.Max(0, TotalTasks - DoneTasks);
        public double ProgressPercent => TotalTasks == 0 ? 0 : (double)DoneTasks / TotalTasks * 100;
        public string ProgressText => $"{DoneTasks} / {TotalTasks} zadań ukończonych ({ProgressPercent:0}%)";
    }

    public partial class SprintsOverviewViewModel : ObservableObject
    {
        private readonly IProjectService _projectService;
        public ICommand OpenSprintReportCommand { get; }
        public ICommand CreateSprintCommand { get; }

        private int _projectId;

        public bool CanManageProject { get; set; }

        public string ProjectName { get; set; } = "";

        private ObservableCollection<SprintItem> _sprints = new();
        public ObservableCollection<SprintItem> Sprints {
            get => _sprints;
            set { _sprints = value; OnPropertyChanged(); }
        }

        public SprintsOverviewViewModel(IProjectService projectService, int projectId)
        {
            _projectService = projectService;
            _projectId = projectId;

            OpenSprintReportCommand = new RelayCommand<SprintItem>(OpenSprintReport);
            CreateSprintCommand = new RelayCommand(CreateSprint);

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

        public async System.Threading.Tasks.Task LoadSprintsAsync()
        {
            var (project, sprints) = await _projectService.GetSprintsByProjectIdAsync(_projectId);

            if (project == null) return;

            CanManageProject = UserHelper.HasAdminPowers() ||
                               project.ProjectUsers.Any(pu => pu.UserId == App.CurrentUser?.Id
                                                           && (pu.Role == UserRole.Manager || pu.Role == UserRole.Owner));

            ProjectName = project.Name;

            _sprints.Clear();
            foreach (var s in sprints)
            {
                _sprints.Add(new SprintItem
                {
                    Id = s.Id,
                    SprintName = s.Name,
                    CreatorName = s.Creator.FullName,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    IsActive = s.Status == SprintStatus.Active,
                    IsPlanned = s.Status == SprintStatus.Planned,
                    DoneTasks = s.SprintTasks.Count(st => st.Task.Status == TaskStatus.Closed && !st.RemovedAt.HasValue),
                    TotalTasks = s.SprintTasks.Count(st => !st.RemovedAt.HasValue)
                });
            }

            OnPropertyChanged(string.Empty);
        }

        private void OpenSprintReport(SprintItem? sprintItem)
        {
            if (sprintItem == null) return;

            var reportView = new SprintReportView(sprintItem.Id, _projectId);
            WeakReferenceMessenger.Default.Send(new NavigationMessage(reportView));
        }

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
    }
}