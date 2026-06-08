
using System.Windows;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Services;
using TeamTaskManager.ViewModels;

namespace TeamTaskManager.Views
{
    public partial class EditWorklogWindow : Window
    {
        public EditWorklogWindow(int worklogId)
        {
            InitializeComponent();

            var dbContext = new AppDbContext();
            var worklogService = new WorklogService(dbContext);
            var vm = new EditWorklogViewModel(worklogService, worklogId);

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
