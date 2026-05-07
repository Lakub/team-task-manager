using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using TeamTaskManager.Helpers;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;

namespace TeamTaskManager.Views
{
    public partial class MainWindowOld : Window
    {
        private ObservableCollection<TeamTaskManager.Models.Entities.Task> tasks { get; } = new();
        private User joe;
        private Project proj1;

        public MainWindowOld()
        {
            InitializeComponent();

            using var context = new AppDbContext();

            var dbTasks = context.Tasks
                .Include(t => t.Reporter)
                .Include(t => t.Assignee)
                .Include(t => t.Project)
                .Include(t => t.Sprint)
                .Include(t => t.ParentTask)
                .Include(t => t.Comments)
                .ThenInclude(c => c.Commenter)
                .ToList();

            foreach (var t in dbTasks)
            {
                t.LastCommentText = t.Comments
                    .OrderByDescending(c => c.CreatedAt)
                    .FirstOrDefault()?.Text ?? string.Empty;
                tasks.Add(t);
            }

            TaskList.ItemsSource = tasks;

            joe = context.Users.FirstOrDefault(u => u.FullName == "Joe Mama")!;
            proj1 = context.Projects.FirstOrDefault(p => p.Name == "Projekt 1")!;
        }

        private TeamTaskManager.Models.Entities.Task? SelectedTask => TaskList.SelectedItem as TeamTaskManager.Models.Entities.Task;

        private void Login_Click(object sender, RoutedEventArgs e) { }

        private void CreateTask_Click(object sender, RoutedEventArgs e)
        {
            using var context = new AppDbContext();
            context.Users.Attach(joe);
            context.Projects.Attach(proj1);

            var task = new TeamTaskManager.Models.Entities.Task
            {
                Title = TaskTitleBox.Text,
                Description = TaskDescBox.Text,
                Reporter = joe,
                Project = proj1
            };

            context.Tasks.Add(task);
            context.SaveChanges();

            context.Entry(task).Reference(t => t.Reporter).Load();
            context.Entry(task).Reference(t => t.Assignee).Load();
            context.Entry(task).Reference(t => t.Project).Load();
            context.Entry(task).Reference(t => t.Sprint).Load();
            context.Entry(task).Reference(t => t.ParentTask).Load();

            task.LastCommentText = string.Empty;
            tasks.Add(task);
            TaskList.SelectedItem = task;
        }

        private void AddComment_Click(object sender, RoutedEventArgs e)
        {
            var task = SelectedTask;
            if (task == null)
            {
                MessageBox.Show("Select a task first.");
                return;
            }

            if (string.IsNullOrWhiteSpace(CommentBox.Text))
            {
                MessageBox.Show("Comment cannot be empty.");
                return;
            }

            using var context = new AppDbContext();
            var dbTask = context.Tasks.First(t => t.Id == task.Id);
            var user = context.Users.First(u => u.Id == joe.Id);

            var comment = new Comment
            {
                TaskId = dbTask.Id,
                Task = dbTask,
                CommenterId = user.Id,
                Commenter = user,
                Text = CommentBox.Text.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            context.Comments.Add(comment);
            context.SaveChanges();

            task.LastCommentText = comment.Text;
            TaskList.Items.Refresh();
            CommentBox.Clear();
            MessageBox.Show("Comment saved.");
        }

        private void AddTime_Click(object sender, RoutedEventArgs e)
        {
            var task = SelectedTask;
            if (task == null)
            {
                MessageBox.Show("Select a task first.");
                return;
            }

            if (!int.TryParse(MinutesBox.Text, out int minutes) || minutes <= 0)
            {
                MessageBox.Show("Enter a valid number of minutes.");
                return;
            }

            using var context = new AppDbContext();
            var dbTask = context.Tasks.First(t => t.Id == task.Id);
            var user = context.Users.First(u => u.Id == joe.Id);

            var worklog = new Worklog
            {
                TaskId = dbTask.Id,
                Task = dbTask,
                UserId = user.Id,
                User = user,
                Description = WorkDescriptionBox.Text?.Trim() ?? string.Empty,
                StartTime = DateTime.UtcNow,
                TimeSpent = TimeSpan.FromMinutes(minutes),
                LoggedAt = DateTime.UtcNow
            };

            context.Worklogs.Add(worklog);
            context.SaveChanges();

            WorkDescriptionBox.Clear();
            MinutesBox.Clear();
            MessageBox.Show("Time saved.");
        }
    }
}
