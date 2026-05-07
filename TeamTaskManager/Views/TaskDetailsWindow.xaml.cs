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

                DescriptionText.SizeChanged += (_, _) => CheckDescriptionOverflow(vm);

                DescriptionText.UpdateLayout();
                CheckDescriptionOverflow(vm);
            }
        }

        private void CheckDescriptionOverflow(TaskDetailsViewModel vm)
        {
            // jesli wiecej niz 4 linijki tesku to 'pokaz wiecej'
            DescriptionText.Measure(new Size(DescriptionText.ActualWidth, double.PositiveInfinity));
            vm.DescriptionOverflows = DescriptionText.DesiredSize.Height > 88;
        }
    }
}
