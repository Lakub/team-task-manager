
using System.Windows;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Services;
using TeamTaskManager.ViewModels;

namespace TeamTaskManager.Views
{
    public partial class CreateTaskWindow : Window
    {
        public CreateTaskWindow(int projectId = -1)
        {
            InitializeComponent();

            var dbContext = new AppDbContext();
            var taskService = new TaskService(dbContext);
            var projectService = new ProjectService(dbContext);
            var vm = new CreateTaskViewModel(taskService, projectService, projectId);

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
