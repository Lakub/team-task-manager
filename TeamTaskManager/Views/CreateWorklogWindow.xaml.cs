
using System.Windows;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Services;
using TeamTaskManager.ViewModels;

namespace TeamTaskManager.Views
{
    public partial class CreateWorklogWindow : Window
    {
        public CreateWorklogWindow(int taskId)
        {
            InitializeComponent();

            var dbContext = new AppDbContext();
            var worklogService = new WorklogService(dbContext);
            var taskService = new TaskService(dbContext);
            var vm = new CreateWorklogViewModel(worklogService, taskService, taskId);

            vm.OnSuccess = () =>
            {
                DialogResult = true;
                Close();
            };

            vm.OnCancel = () =>
            {
                DialogResult = false;
                Close();
            };

            DataContext = vm;

            Loaded += async (_, _) => await vm.InitializeAsync();
        }
    }
}
