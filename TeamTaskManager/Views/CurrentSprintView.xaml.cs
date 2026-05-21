using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using TeamTaskManager.Helpers;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Enums;
using TeamTaskManager.ViewModels;
using TaskStatus = TeamTaskManager.Models.Enums.TaskStatus;

namespace TeamTaskManager.Views
{
    public partial class CurrentSprintView : UserControl
    {
        private Point _dragStart;
        private Point _clickOffset;

        public CurrentSprintView(int sprintId = -1)
        {
            InitializeComponent();
            DataContext = new CurrentSprintViewModel(new AppDbContext(), sprintId);
            Loaded += CurrentSprintView_Loaded;
        }

        private async void CurrentSprintView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is CurrentSprintViewModel vm)
            {
                vm.OnTaskSelected = item =>
                {
                    var taskDetailWindow = new TaskDetailsWindow(item.TaskId);
                    taskDetailWindow.ShowDialog();
                };

                await vm.InitializeAsync();
            }
        }

        private void TaskCard_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStart = e.GetPosition(null);
            if (sender is Button btn)
                _clickOffset = e.GetPosition(btn);
        }

        private void TaskCard_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            var pos = e.GetPosition(null);
            var diff = _dragStart - pos;
            if (Math.Abs(diff.X) <= SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) <= SystemParameters.MinimumVerticalDragDistance) return;

            if (sender is not Button button || button.DataContext is not KanbanTaskItem item) return;

            var root = Application.Current.MainWindow.Content as UIElement;
            var layer = AdornerLayer.GetAdornerLayer(root);
            var ghost = new DragGhost(root, button, _clickOffset);
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
        }

        private void Column_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(typeof(KanbanTaskItem))
                ? DragDropEffects.Move
                : DragDropEffects.None;
            e.Handled = true;
        }

        private async void Column_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(KanbanTaskItem)) is not KanbanTaskItem item) return;
            if (DataContext is not CurrentSprintViewModel vm) return;
            if (sender is not FrameworkElement el) return;

            var tag = el.Tag as string;
            TaskStatus? newStatus = tag switch
            {
                "Open" => TaskStatus.Open,
                "InProgress" => TaskStatus.InProgress,
                "Closed" => TaskStatus.Closed,
                _ => null
            };

            if (newStatus.HasValue)
                await vm.MoveTaskAsync(item, newStatus.Value);
        }
    }
}