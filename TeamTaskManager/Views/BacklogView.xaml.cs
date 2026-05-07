using System.Windows;
using System.Windows.Controls;
using TeamTaskManager.Models;
using TeamTaskManager.Services;
using TeamTaskManager.ViewModels;

namespace TeamTaskManager.Views
{
    public partial class BacklogView : UserControl
    {
        public BacklogView(int sprintId, int projectId)
        {
            InitializeComponent();

            var dbContext = new AppDbContext();
            DataContext = new BacklogViewModel(
                new BacklogService(dbContext),
                sprintId,
                projectId);

            Loaded += BacklogView_Loaded;
        }

        private async void BacklogView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is BacklogViewModel vm)
            {
                vm.OnTaskSelected = item =>
                {
                    if (item != null)
                    {
                        var taskDetailWindow = new TaskDetailsWindow(item.TaskId);
                        taskDetailWindow.ShowDialog();
                    }
                };

                await vm.InitializeAsync();
            }
        }
    }
}