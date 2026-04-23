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

namespace TeamTaskManager.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        public object currentView;

        public ObservableCollection<Project> Projects { get; } = new();
        [ObservableProperty]
        private Project? selectedProject;
        public string CurrentUserName => App.CurrentUser?.FullName ?? "Uzytkownik";

        public ICommand ShowCurrentSprintCommand { get; }
        public ICommand ShowAllSprintsCommand { get; }
        public ICommand ShowTeamMembersCommand { get; }
        public ICommand CreateNewSprintCommand { get; }
        public ICommand ShowSprintReportCommand { get; }
        public ICommand SeedDbCommand { get; }
        public ICommand RandomSeedDbCommand { get; }
        public ICommand ClearDbCommand { get; }

        public ICommand CreateNewProjectCommand { get;  }
        public bool IsAdmin => App.CurrentUser?.OrgRole == Models.Enums.OrgRole.Admin;

        private void LoadProjects()
        {
            using var context = new AppDbContext();
            var projects = context.Projects
                .Where(p => !p.IsDeleted &&
                            p.ProjectUsers.Any(pu => pu.UserId == App.CurrentUser!.Id))
                .ToList();
            Projects.Clear();
            foreach (var p in projects)
                Projects.Add(p);
            SelectedProject = Projects.FirstOrDefault();
        }
        public MainWindowViewModel()
        {
            CurrentView = new CurrentSprintView();

            ShowCurrentSprintCommand = new RelayCommand(() =>
                CurrentView = new CurrentSprintView());

            ShowAllSprintsCommand = new RelayCommand(() =>
                CurrentView = new SprintsOverviewView());

            ShowTeamMembersCommand = new RelayCommand(() =>
                System.Windows.MessageBox.Show("not implemented", "not implemented", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning));

            CreateNewSprintCommand = new RelayCommand(() =>
            {
                var createSprintWindow = new CreateSprintView();
                createSprintWindow.ShowDialog();
            });

            ShowSprintReportCommand = new RelayCommand(() =>
                CurrentView = new SprintReportView());

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

            WeakReferenceMessenger.Default.Register<NavigationMessage>(this, (recipient, message) =>
            {
                CurrentView = message.TargetView;
            });
            CreateNewProjectCommand = new RelayCommand(() =>
            {
                var createProjectWindow = new CreateProjectWindow();
                if (createProjectWindow.ShowDialog() == true)
                    LoadProjects();
            });
            LoadProjects();
        }
    }

}
