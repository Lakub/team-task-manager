using System.Windows;
using System.Windows.Controls;
using TeamTaskManager.Models;
using TeamTaskManager.Services;
using TeamTaskManager.ViewModels;

namespace TeamTaskManager.Views
{
    public partial class AllTasksView : UserControl
    {
        private AllTasksViewModel? _vm;

        public AllTasksView(int projectId)
        {
            InitializeComponent();

            var dbContext = new AppDbContext();
            _vm = new AllTasksViewModel(
                new AllTasksService(dbContext),
                new BacklogService(dbContext),
                projectId);

            DataContext = _vm;

            _vm.SelectedTaskChanged += OnSelectedTaskChanged;

            Loaded += async (_, _) => await _vm.InitializeAsync();
        }

        private void OnSelectedTaskChanged(BacklogTaskItem? item)
        {
            if (item == null)
            {
                TaskDetailPanel.DataContext = null;
                return;
            }

            if (TaskDetailPanel.DataContext is TaskDetailsViewModel currentVm && currentVm.TaskId == item.TaskId)
            {
                return;
            }

            var dbContext = new AppDbContext();
            var detailVm = new TaskDetailsViewModel(new TaskService(dbContext), item.TaskId);

            TaskDetailPanel.DataContext = detailVm;

            _ = detailVm.InitializeAsync();
        }
    }
}
