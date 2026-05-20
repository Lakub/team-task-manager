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
            // jesli wiecej niz 4 linijki po 22 lineheight
            DescriptionText.Measure(new Size(DescriptionText.ActualWidth, double.PositiveInfinity));
            vm.TextOverflows = DescriptionText.DesiredSize.Height > 22 * 4;
        }

        private void ItemText_Loaded(object sender, RoutedEventArgs e)
        {
            CheckItemTextOverflow(sender as TextBlock);
        }

        private void ItemText_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CheckItemTextOverflow(sender as TextBlock);
        }

        private void CheckItemTextOverflow(TextBlock? textBlock)
        {
            if (textBlock == null) return;

            // jesli wiecej niz 3 linijki po 20 lineheight
            textBlock.Measure(new Size(textBlock.ActualWidth, double.PositiveInfinity));
            bool overflows = textBlock.DesiredSize.Height > 20 * 3;

            if (textBlock.DataContext is CommentItem comment)
                comment.TextOverflows = overflows;
            else if (textBlock.DataContext is WorklogItem worklog)
                worklog.TextOverflows = overflows;
        }
    }
}
