using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using TeamTaskManager.Helpers;
using TeamTaskManager.Views;
using TeamTaskManager.Models;
using TeamTaskManager.Services;
using CommunityToolkit.Mvvm.Messaging;

namespace TeamTaskManager.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        public object currentView;

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

        public bool IsHeadAdmin => string.Equals(App.CurrentUser?.Email, "j.kowalski@email.com", System.StringComparison.OrdinalIgnoreCase);

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

            WeakReferenceMessenger.Default.Register<NavigationMessage>(this, (recipient, message) =>
            {
                CurrentView = message.TargetView;
            });

            OnPropertyChanged(nameof(IsHeadAdmin));
        }
    }
}
