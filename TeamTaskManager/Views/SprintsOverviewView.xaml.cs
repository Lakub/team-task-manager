using System.Windows;
using System.Windows.Controls;
using TeamTaskManager.Models;
using TeamTaskManager.Services;
using TeamTaskManager.ViewModels;

namespace TeamTaskManager.Views
{
    public partial class SprintsOverviewView : UserControl
    {
        public SprintsOverviewView(int projectId = -1)
        {
            InitializeComponent();

            var dbContext = new AppDbContext();
            DataContext = new SprintsOverviewViewModel(
                new ProjectService(dbContext),
                new SprintService(dbContext),
                projectId);

            Loaded += SprintsOverviewView_Loaded;
        }

        private async void SprintsOverviewView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is SprintsOverviewViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }
    }
}
