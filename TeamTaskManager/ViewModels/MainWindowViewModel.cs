using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using TeamTaskManager.Helpers;
using TeamTaskManager.Views;
using TeamTaskManager.Models;
using TeamTaskManager.Services;
using CommunityToolkit.Mvvm.Messaging;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Models.Enums;

namespace TeamTaskManager.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly IProjectService _projectService;

        [ObservableProperty]
        public object currentView;

        public ObservableCollection<Project> Projects { get; } = new();
        
        private Project? _selectedProject;
        public Project? SelectedProject
        {
            get => _selectedProject;
            set
            {
                SetProperty(ref _selectedProject, value);
                ActiveSprint = value?.Sprints.FirstOrDefault(s => s.Status == SprintStatus.Active);;
                CurrentView = CurrentView switch
                {
                    CurrentSprintView => new CurrentSprintView(),
                    SprintsOverviewView => new SprintsOverviewView(SelectedProject?.Id ?? -1),
                    SprintReportView => ActiveSprint != null ? new SprintReportView(ActiveSprint.Id) : CurrentView,
                    _ => CurrentView
                };
            }
        }

        [ObservableProperty]
        private Sprint? activeSprint;

        public string CurrentUserName => App.CurrentUser?.FullName ?? "Uzytkownik";

        public ICommand ShowCurrentSprintCommand { get; }
        public ICommand ShowAllSprintsCommand { get; }
        public ICommand ShowTeamMembersCommand { get; }
        public ICommand CreateNewSprintCommand { get; }
        public ICommand ShowSprintReportCommand { get; }
        public ICommand ShowHeadAdminPanelCommand { get; }
        public ICommand SeedDbCommand { get; }
        public ICommand RandomSeedDbCommand { get; }
        public ICommand ClearDbCommand { get; }
        public ICommand ShowWikiCommand { get; }

        public bool IsHeadAdmin => string.Equals(App.CurrentUser?.Email, "j.kowalski@email.com", System.StringComparison.OrdinalIgnoreCase);

        public ICommand CreateNewProjectCommand { get;  }
        public bool IsAdmin => App.CurrentUser?.OrgRole == Models.Enums.OrgRole.Admin;

        private async void LoadProjects()
        {
            var projects = await _projectService.GetNonDeletedProjectsWithSprintsByUserIdAsync(App.CurrentUser!.Id);
            Projects.Clear();
            foreach (var p in projects)
                Projects.Add(p);
            SelectedProject = Projects.FirstOrDefault();
        }

        public MainWindowViewModel(IProjectService projectService)
        {
            _projectService = projectService;

            CurrentView = new CurrentSprintView();

            ShowCurrentSprintCommand = new RelayCommand(() =>
                CurrentView = new CurrentSprintView());

            ShowAllSprintsCommand = new RelayCommand(() =>
            {
                if (SelectedProject == null)
                {
                    System.Windows.MessageBox.Show("Nie wybrano projektu.", "Błąd", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }
                CurrentView = new SprintsOverviewView(SelectedProject?.Id ?? -1);
            });

            ShowTeamMembersCommand = new RelayCommand(() =>
                System.Windows.MessageBox.Show("not implemented", "not implemented", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning));

            ShowWikiCommand = new RelayCommand(() =>
                CurrentView = new WikiMainView());

            CreateNewSprintCommand = new RelayCommand(() =>
            {
                var createSprintWindow = new CreateSprintView();
                createSprintWindow.ShowDialog();
            });

            ShowSprintReportCommand = new RelayCommand(() =>
            {
                if (ActiveSprint == null)
                {
                    System.Windows.MessageBox.Show("Brak aktywnego sprintu w projekcie.", "Błąd", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }
                CurrentView = new SprintReportView(ActiveSprint?.Id ?? -1);
            });

            ShowHeadAdminPanelCommand = new RelayCommand(() =>
            {
                if (!IsHeadAdmin)
                {
                    System.Windows.MessageBox.Show("Brak uprawnień do panelu HeadAdmin.", "Autoryzacja", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                CurrentView = new HeadAdminPanelView();
            });

            SeedDbCommand = new RelayCommand(() =>
            {
                using var context = new AppDbContext();
                SeedData.Seed(context);
                System.Windows.MessageBox.Show("Sukces", "Sukces", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            });

            RandomSeedDbCommand = new RelayCommand(() =>
            {
                using var context = new AppDbContext();
                var authService = new AuthService(context);
                SeedData.RandomSeed(context, authService);
                System.Windows.MessageBox.Show("Sukces", "Sukces", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            });

            ClearDbCommand = new RelayCommand(() =>
            {
                using var context = new AppDbContext();
                SeedData.Clear(context);
                System.Windows.MessageBox.Show("Sukces", "Sukces", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            });

            CreateNewProjectCommand = new RelayCommand(() =>
            {
                var createProjectWindow = new CreateProjectWindow();
                if (createProjectWindow.ShowDialog() == true)
                    LoadProjects();
            });

            WeakReferenceMessenger.Default.Register<NavigationMessage>(this, (recipient, message) =>
            {
                CurrentView = message.TargetView;
            });

            LoadProjects();

            OnPropertyChanged(nameof(IsHeadAdmin));
        }
    }
}
