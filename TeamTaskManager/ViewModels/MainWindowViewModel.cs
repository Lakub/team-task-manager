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
                    WikiMainView => SelectedProject != null ? new WikiMainView(SelectedProject.Id) : CurrentView,
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
        public ICommand RandomSeedDbCommand { get; }
        public ICommand ClearDbCommand { get; }
        public ICommand ShowWikiCommand { get; }
        public ICommand EditProjectCommand { get; }
        public ICommand LogoutCommand { get; }
        public bool IsHeadAdmin => App.CurrentUser?.OrgRole == OrgRole.HeadAdmin;

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
            {
                if (SelectedProject == null)
                {
                    System.Windows.MessageBox.Show("Najpierw wybierz projekt z listy po lewej stronie.", "Brak projektu", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }
                CurrentView = new WikiMainView(SelectedProject.Id);
            });

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
            LogoutCommand = new RelayCommand(() =>
            {
                App.IsLoggingOut = true;
                App.CurrentUser = null;

                System.Windows.Application.Current.MainWindow.Close();

                var login = new LoginWindow();
                if (login.ShowDialog() == true)
                {
                    App.CurrentUser = login.LoggedInUser;
                    App.IsLoggingOut = false;
                    var main = new MainWindow();
                    main.Closed += (s, args) => { if (!App.IsLoggingOut) System.Windows.Application.Current.Shutdown(); };
                    main.Show();
                }
                else
                {
                    System.Windows.Application.Current.Shutdown();
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
