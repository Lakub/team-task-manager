using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using TeamTaskManager.Helpers;
using TeamTaskManager.Views;
using TeamTaskManager.Models;

namespace TeamTaskManager.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        public object currentView;

        public ICommand ShowCurrentSprintCommand { get; }
        public ICommand ShowAllSprintsCommand { get; }
        public ICommand ShowTeamMembersCommand { get; }
        public ICommand CreateNewSprintCommand { get; }
        public ICommand ShowSprintReportCommand { get; }
        public ICommand SeedDbCommand { get; }
        public ICommand ClearDbCommand { get; }

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

            ClearDbCommand = new RelayCommand(() =>
            {
                using var context = new AppDbContext();
                SeedData.Clear(context);
                System.Windows.MessageBox.Show("Sukces", "Sukces", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            });
        }
    }
}
