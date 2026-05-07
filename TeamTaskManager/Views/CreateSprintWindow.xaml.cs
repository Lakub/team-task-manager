using System.Windows;
using System.Windows.Controls;
using TeamTaskManager.Models;
using TeamTaskManager.Services;
using TeamTaskManager.ViewModels;

namespace TeamTaskManager.Views;

public partial class CreateSprintWindow : Window
{
    public CreateSprintWindow(int projectId)
    {
        InitializeComponent();

        var dbContext = new AppDbContext();
        var sprintService = new SprintService(dbContext);
        var vm = new CreateSprintViewModel(sprintService, projectId);

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