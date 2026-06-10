
using System.Windows;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Services;
using TeamTaskManager.ViewModels;

namespace TeamTaskManager.Views
{
    public partial class EditTaskWindow : Window
    {
        public EditTaskWindow(int taskId)
        {
            InitializeComponent();

            var dbContext = new AppDbContext();
            var taskService = new TaskService(dbContext);
            var projectService = new ProjectService(dbContext);
            var vm = new EditTaskViewModel(taskService, projectService, taskId);

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
