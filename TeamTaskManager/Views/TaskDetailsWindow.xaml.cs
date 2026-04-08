using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;
using TaskEntity = TeamTaskManager.Models.Entities.Task;
using TaskStatusEnum = TeamTaskManager.Models.Enums.TaskStatus;

namespace TeamTaskManager.Views
{
    public partial class TaskDetailsWindow : UserControl, INotifyPropertyChanged
    {
        private readonly AppDbContext _context = new AppDbContext();
        private readonly int _currentUserId = 1;

        private TaskEntity? _selectedTask;
        private string _newCommentText = string.Empty;
        private string _newWorklogDescription = string.Empty;
        private string _newWorklogMinutes = string.Empty;
        private TaskStatusEnum _selectedStatus;

        public ObservableCollection<Comment> Comments { get; set; } = new();
        public ObservableCollection<Worklog> Worklogs { get; set; } = new();
        public ObservableCollection<TaskStatusEnum> AvailableStatuses { get; set; } = new();

        public TaskEntity? SelectedTask
        {
            get => _selectedTask;
            set { _selectedTask = value; OnPropertyChanged(); }
        }

        public string NewCommentText
        {
            get => _newCommentText;
            set { _newCommentText = value; OnPropertyChanged(); }
        }

        public string NewWorklogDescription
        {
            get => _newWorklogDescription;
            set { _newWorklogDescription = value; OnPropertyChanged(); }
        }

        public string NewWorklogMinutes
        {
            get => _newWorklogMinutes;
            set { _newWorklogMinutes = value; OnPropertyChanged(); }
        }

        public TaskStatusEnum SelectedStatus
        {
            get => _selectedStatus;
            set { _selectedStatus = value; OnPropertyChanged(); }
        }

        public TaskDetailsWindow()
        {
            InitializeComponent();
            DataContext = this;

            AvailableStatuses = new ObservableCollection<TaskStatusEnum>(
                Enum.GetValues(typeof(TaskStatusEnum)).Cast<TaskStatusEnum>());
        }

        public void LoadTask(int taskId)
        {
            var task = _context.Tasks
                .Include(t => t.Reporter)
                .Include(t => t.Assignee)
                .FirstOrDefault(t => t.Id == taskId);

            if (task == null)
            {
                MessageBox.Show("Task not found.");
                return;
            }

            SelectedTask = task;
            SelectedStatus = task.Status;

            LoadComments();
            LoadWorklogs();
        }

        private void LoadComments()
        {
            Comments.Clear();
            if (SelectedTask == null) return;

            var comments = _context.Comments
                .Include(c => c.Commenter)
                .Where(c => c.TaskId == SelectedTask.Id)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            foreach (var comment in comments)
                Comments.Add(comment);
        }

        private void LoadWorklogs()
        {
            Worklogs.Clear();
            if (SelectedTask == null) return;

            var worklogs = _context.Worklogs
                .Include(w => w.User)
                .Where(w => w.TaskId == SelectedTask.Id)
                .OrderByDescending(w => w.LoggedAt)
                .ToList();

            foreach (var worklog in worklogs)
                Worklogs.Add(worklog);
        }

        private void AddComment_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTask == null) { MessageBox.Show("Select a task first."); return; }
            if (string.IsNullOrWhiteSpace(NewCommentText)) { MessageBox.Show("Comment cannot be empty."); return; }

            var user = _context.Users.First(u => u.Id == _currentUserId);

            var comment = new Comment
            {
                TaskId = SelectedTask.Id,
                Task = SelectedTask,
                CommenterId = user.Id,
                Commenter = user,
                Text = NewCommentText.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            _context.SaveChanges();

            var saved = _context.Comments.Include(c => c.Commenter).First(c => c.Id == comment.Id);
            Comments.Insert(0, saved);
            NewCommentText = string.Empty;
        }

        private void AddWorklog_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTask == null) { MessageBox.Show("Select a task first."); return; }
            if (!int.TryParse(NewWorklogMinutes, out int minutes) || minutes <= 0) { MessageBox.Show("Enter a valid number of minutes."); return; }

            var user = _context.Users.First(u => u.Id == _currentUserId);

            var worklog = new Worklog
            {
                TaskId = SelectedTask.Id,
                Task = SelectedTask,
                UserId = user.Id,
                User = user,
                Description = NewWorklogDescription?.Trim() ?? string.Empty,
                StartTime = DateTime.UtcNow,
                TimeSpent = TimeSpan.FromMinutes(minutes),
                LoggedAt = DateTime.UtcNow
            };

            _context.Worklogs.Add(worklog);
            _context.SaveChanges();

            var saved = _context.Worklogs.Include(w => w.User).First(w => w.Id == worklog.Id);
            Worklogs.Insert(0, saved);
            NewWorklogDescription = string.Empty;
            NewWorklogMinutes = string.Empty;
        }

        private void SaveStatus_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTask == null) { MessageBox.Show("Select a task first."); return; }

            var task = _context.Tasks.FirstOrDefault(t => t.Id == SelectedTask.Id);
            if (task == null) { MessageBox.Show("Task not found."); return; }

            task.Status = SelectedStatus;
            task.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();

            SelectedTask.Status = SelectedStatus;
            OnPropertyChanged(nameof(SelectedTask));
            MessageBox.Show("Status saved.");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}