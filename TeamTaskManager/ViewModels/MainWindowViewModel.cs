using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SQLitePCL;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TeamTaskManager.Helpers;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Models.Enums;
using TeamTaskManager.Services;
using TeamTaskManager.Views;

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
                ActiveSprint = value?.Sprints.FirstOrDefault(s => s.Status == SprintStatus.Active);
                CurrentView = CurrentView switch
                {
                    CurrentSprintView => new CurrentSprintView(),
                    SprintsOverviewView => new SprintsOverviewView(SelectedProject?.Id ?? -1),
                    SprintReportView => SelectedProject != null ? new SprintReportView(ActiveSprint?.Id ?? -1, SelectedProject.Id) : CurrentView,
                    BacklogView => SelectedProject != null ? new BacklogView(ActiveSprint?.Id ?? -1, SelectedProject.Id) : CurrentView,
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
        public ICommand ShowBacklogCommand { get; }
        public ICommand ShowSprintReportCommand { get; }
        public ICommand ShowHeadAdminPanelCommand { get; }
        public ICommand SeedDbCommand { get; }
        public ICommand RandomSeedDbCommand { get; }
        public ICommand ClearDbCommand { get; }
        public ICommand ShowWikiCommand { get; }
        public ICommand EditProjectCommand { get; }

        public bool IsHeadAdmin => string.Equals(App.CurrentUser?.Email, "j.kowalski@email.com", System.StringComparison.OrdinalIgnoreCase);

        public ICommand CreateNewProjectCommand { get;  }
        public bool IsAdmin => App.CurrentUser?.OrgRole == Models.Enums.OrgRole.Admin;

        private async void LoadProjects(int id=-1)
        {
            _projectService.SwitchContext();
            var projects = await _projectService.GetNonDeletedProjectsWithSprintsByUserIdAsync(App.CurrentUser!.Id);
            Projects.Clear();
            foreach (var p in projects)
                Projects.Add(p);
            if (id < 0)
                SelectedProject = Projects.FirstOrDefault();
            else
                SelectedProject = Projects.FirstOrDefault(e => e.Id == id);
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

            ShowBacklogCommand = new RelayCommand(() =>
            {
                if (SelectedProject == null)
                {
                    System.Windows.MessageBox.Show("Nie wybrano projektu.", "Błąd", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                CurrentView = new BacklogView(ActiveSprint?.Id ?? -1, SelectedProject?.Id ?? -1);
            });

            ShowSprintReportCommand = new RelayCommand(() =>
            {
                if (SelectedProject == null)
                {
                    System.Windows.MessageBox.Show("Nie wybrano projektu.", "Błąd", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }
                CurrentView = new SprintReportView(ActiveSprint?.Id ?? -1, SelectedProject?.Id ?? -1);
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
                    
            EditProjectCommand = new RelayCommand(() =>
            {
                var editProjectWindow = new CreateProjectWindow(true,SelectedProject);
                if (editProjectWindow.ShowDialog() == true){
                    LoadProjects(editProjectWindow.editedProject.Id);
                }
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
