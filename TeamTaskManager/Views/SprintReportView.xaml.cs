using System.Windows;
using System.Windows.Controls;
using TeamTaskManager.Models;
using TeamTaskManager.Services;
using TeamTaskManager.ViewModels;

namespace TeamTaskManager.Views
{
    public partial class SprintReportView : UserControl
    {
        public SprintReportView(int sprintId, int projectId)
        {
            InitializeComponent();

            var dbContext = new AppDbContext();
            DataContext = new SprintReportViewModel(
                new SprintService(dbContext),
                new UserService(dbContext),
                sprintId,
                projectId);

            Loaded += SprintReportView_Loaded;
        }

        private async void SprintReportView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is SprintReportViewModel vm)
            {
                vm.OnTaskSelected = item =>
                {
                    if (item != null)
                    {
                        var taskDetailWindow = new TaskDetailsWindow(item.TaskId);
                        taskDetailWindow.ShowDialog();
                    }
                };

                vm.OnTeamMemberSelected = item =>
                {
                    if (item != null)
                    {
                        MessageBox.Show($"User: {item.FullName}", "User Profile Window", MessageBoxButton.OK, MessageBoxImage.Information);
                        //var userProfileWindow = new UserProfileWindow(item.Id);
                        //userProfileWindow.ShowDialog();
                    }
                };

                await vm.InitializeAsync();
            }
        }
    }
}
