using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using TeamTaskManager.Helpers;
using TeamTaskManager.Models;
using TeamTaskManager.Services;
using TeamTaskManager.ViewModels;

namespace TeamTaskManager.Views
{
    public partial class BacklogView : UserControl
    {
        private Point _dragStart;
        private Point _clickOffset;

        private bool _isDragging = false;

        public BacklogView(int projectId, int? sprintId = null)
        {
            InitializeComponent();

            var dbContext = new AppDbContext();
            DataContext = new BacklogViewModel(
                new BacklogService(dbContext),
                projectId,
                sprintId);

            Loaded += BacklogView_Loaded;
        }

        private async void BacklogView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is BacklogViewModel vm)
            {
                vm.OnTaskSelected = item =>
                {
                    if (item != null)
                    {
                        var taskDetailWindow = new TaskDetailsWindow(item.TaskId);
                        taskDetailWindow.ShowDialog();
                    }
                };

                await vm.InitializeAsync();
            }
        }

        private void TaskCard_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStart = e.GetPosition(null);
            if (sender is FrameworkElement btn)
                _clickOffset = e.GetPosition(btn);
        }

        private void TaskCard_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _isDragging) return;

            var vm = DataContext as BacklogViewModel;
            if (!vm.CanEditSprint) return;

            var pos = e.GetPosition(null);
            var diff = _dragStart - pos;
            if (Math.Abs(diff.X) <= SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) <= SystemParameters.MinimumVerticalDragDistance) return;

            if (sender is not FrameworkElement button || button.DataContext is not BacklogTaskItem item) return;

            _isDragging = true;

            var ghostTarget = button.FindName("GhostTarget") as UIElement ?? button;

            var root = Application.Current.MainWindow.Content as UIElement;
            var layer = AdornerLayer.GetAdornerLayer(root);

            var ghost = new DragGhost(root, ghostTarget, _clickOffset);
            layer.Add(ghost);
            ghost.Follow();

            button.Opacity = 0.3;

            GiveFeedbackEventHandler onFeedback = (_, args) =>
            {
                ghost.Follow();
                args.UseDefaultCursors = false;
                args.Handled = true;
            };

            button.GiveFeedback += onFeedback;
            DragDrop.DoDragDrop(button, item, DragDropEffects.Move);
            button.GiveFeedback -= onFeedback;

            button.Opacity = 1;
            layer.Remove(ghost);

            _isDragging = false;
        }

        private void Row_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(typeof(BacklogTaskItem))
                ? DragDropEffects.Move
                : DragDropEffects.None;
            e.Handled = true;
        }

        private async void Row_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(BacklogTaskItem)) is not BacklogTaskItem item) return;
            if (DataContext is not BacklogViewModel vm) return;
            if (sender is not FrameworkElement el) return;

            var tag = el.Tag as string;
            if (tag == "Backlog")
            {
                await vm.RemoveFromSprint(item);
            }
            else if (tag == "Sprint")
            {
                await vm.MoveToSprint(item);
            }
        }
    }
}