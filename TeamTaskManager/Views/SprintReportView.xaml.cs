using System.Windows;
using System.Windows.Controls;
using TeamTaskManager.Models;
using TeamTaskManager.Services;
using TeamTaskManager.ViewModels;

namespace TeamTaskManager.Views
{
    public partial class SprintReportView : UserControl
    {
        public SprintReportView(int sprintId, int projectId)
        {
            InitializeComponent();

            var dbContext = new AppDbContext();
            DataContext = new SprintReportViewModel(
                new SprintService(dbContext),
                sprintId,
                projectId);

            Loaded += SprintReportView_Loaded;
        }

        private async void SprintReportView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is SprintReportViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }
    }
}
