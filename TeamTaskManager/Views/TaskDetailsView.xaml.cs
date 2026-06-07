using System.Windows;
using System.Windows.Controls;
using TeamTaskManager.Models;
using TeamTaskManager.Services;
using TeamTaskManager.ViewModels;

namespace TeamTaskManager.Views
{
    public partial class TaskDetailsView : UserControl
    {
        public TaskDetailsView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is TaskDetailsViewModel vm)
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, () =>
                {
                    if (DescriptionText != null)
                    {
                        DescriptionText.SizeChanged -= (_, _) => CheckDescriptionOverflow(vm);
                        DescriptionText.SizeChanged += (_, _) => CheckDescriptionOverflow(vm);
                        DescriptionText.UpdateLayout();
                        CheckDescriptionOverflow(vm);
                    }
                });
            }
        }

        private void CheckDescriptionOverflow(TaskDetailsViewModel vm)
        {
            if (DescriptionText == null) return;
            DescriptionText.Measure(new Size(DescriptionText.ActualWidth, double.PositiveInfinity));
            vm.TextOverflows = DescriptionText.DesiredSize.Height > 22 * 4;
        }

        private void ItemText_Loaded(object sender, RoutedEventArgs e)
            => CheckItemTextOverflow(sender as TextBlock);

        private void ItemText_SizeChanged(object sender, SizeChangedEventArgs e)
            => CheckItemTextOverflow(sender as TextBlock);

        private void CheckItemTextOverflow(TextBlock? textBlock)
        {
            if (textBlock == null) return;
            textBlock.Measure(new Size(textBlock.ActualWidth, double.PositiveInfinity));
            bool overflows = textBlock.DesiredSize.Height > 20 * 3;

            if (textBlock.DataContext is CommentItem comment)
                comment.TextOverflows = overflows;
            else if (textBlock.DataContext is WorklogItem worklog)
                worklog.TextOverflows = overflows;
        }
    }
}
