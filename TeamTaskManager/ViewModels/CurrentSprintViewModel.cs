using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Models.Enums;
using TaskStatus = TeamTaskManager.Models.Enums.TaskStatus;

namespace TeamTaskManager.ViewModels
{
    public class KanbanTaskItem
    {
        private readonly SprintTask _model;

        public KanbanTaskItem(SprintTask model) => _model = model;
        internal SprintTask Model => _model;

        public int TaskId => _model.TaskId;
        public string Key => $"{_model.Task.Project.Key}-{_model.Task.PerProjectId}";
        public string Title => _model.Task.Title;
        public string AssigneeName => _model.Assignee?.FullName ?? "Nieprzypisany";
        public bool IsAssigned => _model.AssigneeId.HasValue;
    }

    public partial class CurrentSprintViewModel : ObservableObject
    {
        private readonly AppDbContext _context;
        private readonly int _sprintId;

        [ObservableProperty] private string sprintName = string.Empty;
        [ObservableProperty] private bool hasActiveSprint;
        [ObservableProperty] private bool noActiveSprint = true;

        public ObservableCollection<KanbanTaskItem> TodoTasks { get; } = new();
        public ObservableCollection<KanbanTaskItem> InProgressTasks { get; } = new();
        public ObservableCollection<KanbanTaskItem> DoneTasks { get; } = new();

        public Action<KanbanTaskItem>? OnTaskSelected { get; set; }
        public ICommand OpenTaskCommand { get; }

        public CurrentSprintViewModel(AppDbContext context, int sprintId)
        {
            _context = context;
            _sprintId = sprintId;
            OpenTaskCommand = new RelayCommand<KanbanTaskItem?>(item =>
            {
                if (item != null) OnTaskSelected?.Invoke(item);
            });
        }

        public async System.Threading.Tasks.Task MoveTaskAsync(KanbanTaskItem item, TaskStatus newStatus)
        {
            if (item.Model.Status == newStatus) return;

            item.Model.Status = newStatus;
            item.Model.Task.Status = newStatus;
            await _context.SaveChangesAsync();

            TodoTasks.Remove(item);
            InProgressTasks.Remove(item);
            DoneTasks.Remove(item);

            switch (newStatus)
            {
                case TaskStatus.Open: TodoTasks.Add(item); break;
                case TaskStatus.InProgress: InProgressTasks.Add(item); break;
                case TaskStatus.Closed: DoneTasks.Add(item); break;
            }
        }

        public async System.Threading.Tasks.Task InitializeAsync()
        {
            if (_sprintId < 0)
            {
                HasActiveSprint = false;
                NoActiveSprint = true;
                return;
            }

            var sprint = await _context.Sprints.FirstOrDefaultAsync(s => s.Id == _sprintId);
            if (sprint == null) { HasActiveSprint = false; NoActiveSprint = true; return; }

            SprintName = sprint.Name;
            HasActiveSprint = true;
            NoActiveSprint = false;

            var sprintTasks = await _context.SprintTasks
                .Include(st => st.Task).ThenInclude(t => t.Project)
                .Include(st => st.Assignee)
                .Where(st => st.SprintId == _sprintId && !st.IsDeleted && st.RemovedAt == null)
                .ToListAsync();

            TodoTasks.Clear();
            InProgressTasks.Clear();
            DoneTasks.Clear();

            foreach (var st in sprintTasks)
            {
                var item = new KanbanTaskItem(st);
                switch (st.Status)
                {
                    case TaskStatus.Open: TodoTasks.Add(item); break;
                    case TaskStatus.InProgress: InProgressTasks.Add(item); break;
                    case TaskStatus.Closed: DoneTasks.Add(item); break;
                }
            }
        }
    }
}