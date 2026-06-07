using System.Windows;
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
            var vm = new TaskDetailsViewModel(new TaskService(dbContext), taskId);
            vm.IsPoppedOut = true;

            DataContext = vm;
            DetailControl.DataContext = vm;

            Loaded += async (_, _) => await vm.InitializeAsync();
        }
    }
}
