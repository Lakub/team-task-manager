using System;
using System.Windows;
using TeamTaskManager.Models;
using TeamTaskManager.Services;
using TeamTaskManager.ViewModels;

namespace TeamTaskManager.Views
{
    public partial class StartSprintWindow : Window
    {
        public DateTime SelectedEndDate { get; private set; }

        private readonly StartSprintViewModel _vm;

        public StartSprintWindow(int projectId, DateTime? defaultEndDate = null)
        {
            InitializeComponent();

            var dbContext = new AppDbContext();
            var projectService = new ProjectService(dbContext);

            _vm = new StartSprintViewModel(projectService, projectId, defaultEndDate);

            _vm.OnConfirmed = () =>
            {
                SelectedEndDate = _vm.EndDate;
                DialogResult = true;
            };

            _vm.OnCancelled = () => DialogResult = false;

            DataContext = _vm;

            Loaded += async (_, _) => await _vm.InitializeAsync();
        }
    }
}