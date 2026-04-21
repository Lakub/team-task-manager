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
    public class SprintItem : INotifyPropertyChanged
    {
        public int Id { get; set;  }
        public string SprintName { get; set; }
        public string CreatorName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public Visibility DaysRemainingVisibility => IsActive ? Visibility.Visible : Visibility.Collapsed;
        public bool IsPlanned { get; set; }
        public string StatusText => IsActive ? "W toku" : IsPlanned ? "Planowany" : "Zakończony";
        public Brush SprintCardColor => IsActive ? new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)) : new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD));
        public string StartDateStr => StartDate.ToString("dd.MM.yyyy");
        public string EndDateStr => EndDate.ToString("dd.MM.yyyy");
        public int DaysRemaining => Math.Max(0, (EndDate - DateTime.Today).Days);
        public int TotalTasks { get; set; }
        public int DoneTasks { get; set; }
        public int RemainingTasks => Math.Max(0, TotalTasks - DoneTasks);
        public double ProgressPercent => TotalTasks == 0 ? 0 : (double)DoneTasks / TotalTasks * 100;
        public string ProgressText => $"{DoneTasks} / {TotalTasks} zadań ukończonych ({ProgressPercent:0}%)";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    public partial class SprintsOverviewViewModel : INotifyPropertyChanged
    {
        private readonly IProjectService _projectService;
        private readonly ISprintService _sprintService;
        public ICommand OpenSprintRaportCommand { get; }

        private int _projectId;  // TYMCZASOWO USUNIETO READONLY

        public string ProjectName { get; set; }

        private ObservableCollection<SprintItem> _sprints = new();
        public ObservableCollection<SprintItem> Sprints {
            get => _sprints;
            set { _sprints = value; OnPropertyChanged(); }
        }

        public SprintsOverviewViewModel(IProjectService projectService, ISprintService sprintService, int projectId)
        {
            OpenSprintRaportCommand = new RelayCommand<SprintItem>(OpenSprintRaport);
            _projectService = projectService;
            _sprintService = sprintService;
            _projectId = projectId;
        }

        public async System.Threading.Tasks.Task InitializeAsync()
        {
            var projects = await _projectService.GetAllProjectsWithProjectUsersAsync();

            var currentUser = App.CurrentUser;
            var userProjects = projects.Where(p => p.ProjectUsers.Any(pu => pu.UserId == currentUser?.Id)).ToList();

            // TYMCZASOWO BIERZE PIERWSZY LEPSZY PROJEKT UZYTKOWNIKA
            var projectId = userProjects.FirstOrDefault();

            if (projectId == null)
            {
                MessageBox.Show("TEMP ten uzytkownik nie jest czlonkiem zadnego projektu.", "Brak projektu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _projectId = projectId.Id;

            var (project, sprints) = await _projectService.GetSprintsByProjectIdAsync(_projectId);

            if (project == null) return;

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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        private void OpenSprintRaport(SprintItem sprintItem)
        {
            if (sprintItem == null) return;

            var reportView = new SprintReportView(sprintItem.Id);

            WeakReferenceMessenger.Default.Send(new NavigationMessage(reportView));
        }
    }
}