using System.Windows;
using System.Windows.Controls;
using TeamTaskManager.Models;
using TeamTaskManager.Services;
using TeamTaskManager.ViewModels;

namespace TeamTaskManager.Views
{
    public partial class TaskDetailsWindow : Window
    {
        public TaskDetailsWindow(int taskId)
        {
            InitializeComponent();

            var dbContext = new AppDbContext();
            DataContext = new TaskDetailsViewModel(new TaskService(dbContext), taskId);

            Loaded += TaskDetailsWindow_Loaded;
        }

        private async void TaskDetailsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is TaskDetailsViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }
    }
}
