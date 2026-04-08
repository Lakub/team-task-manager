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

        public ICommand ShowSprintRaportCommand { get; }
        public ICommand SeedDbCommand { get; }
        public ICommand ClearDbCommand { get; }

        public MainWindowViewModel()
        {
            ShowSprintRaportCommand = new RelayCommand(() =>
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
