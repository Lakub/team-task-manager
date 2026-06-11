using Microsoft.EntityFrameworkCore;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Models.Enums;
using Task = TeamTaskManager.Models.Entities.Task;
using TaskStatus = TeamTaskManager.Models.Enums.TaskStatus;

namespace TeamTaskManager.Services
{
    public interface ITaskService
    {
        System.Threading.Tasks.Task<Task?> GetTaskByIdAsync(int taskId);
        System.Threading.Tasks.Task<List<Worklog>> GetWorklogsByTaskIdAsync(int taskId);
        System.Threading.Tasks.Task<List<Comment>> GetNonReplyCommentsByTaskIdAsync(int taskId);

        System.Threading.Tasks.Task<Task> CreateTaskAsync(
            string title, string description,
            TaskType type, TaskPriority priority,
            int projectId, int reporterId, int? assigneeId);
        System.Threading.Tasks.Task<Task> EditTaskAsync(
            int taskId, string title, string description,
            TaskType type, TaskPriority priority,
            int? assigneeId);
        System.Threading.Tasks.Task DeleteTaskAsync(int taskId);

        System.Threading.Tasks.Task<Comment> CreateTaskCommentAsync(
            string text, int taskId, int commenterId, int? parentCommentId);
        System.Threading.Tasks.Task DeleteTaskCommentAsync(int commentId);
        System.Threading.Tasks.Task EditTaskCommentAsync(int commentId, string newText);

        System.Threading.Tasks.Task DeleteTaskWorklogAsync(int worklogId);

        System.Threading.Tasks.Task UpdateTaskAssigneeAsync(int taskId, int? newAssigneeId);
        System.Threading.Tasks.Task UpdateTaskStatusAsync(int taskId, TaskStatus newStatus);

    }

    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;

        public TaskService(AppDbContext context)
        {
            _context = context;
        }

        public async System.Threading.Tasks.Task<Task> CreateTaskAsync(
            string title, string description,
            TaskType type, TaskPriority priority,
            int projectId, int reporterId, int? assigneeId)
        {
            var maxPerProjectId = await _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .MaxAsync(t => (int?)t.PerProjectId) ?? 0;

            var now = DateTime.UtcNow;

            var task = new Task
            {
                Title = title,
                Description = description,
                Type = type,
                Priority = priority,
                Status = TaskStatus.Open,
                ReporterId = reporterId,
                AssigneeId = assigneeId,
                ProjectId = projectId,
                PerProjectId = maxPerProjectId + 1,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async System.Threading.Tasks.Task<Task> EditTaskAsync(
            int taskId, string title, string description,
            TaskType type, TaskPriority priority,
            int? assigneeId)
        {
            var task = await _context.Tasks
                .Include(t => t.SprintTasks)
                    .ThenInclude(st => st.Sprint)
                .FirstOrDefaultAsync(t => t.Id == taskId) ?? throw new Exception("Task not found");

            task.Title = title;
            task.Description = description;
            task.Type = type;
            task.Priority = priority;
            task.AssigneeId = assigneeId;
            task.UpdatedAt = DateTime.UtcNow;

            // aktualizujemy wszystkie nieusuniete sprinttaski z aktywnych i planowanych sprintow
            foreach (var st in task.SprintTasks.Where(st => !st.RemovedAt.HasValue && st.Sprint.Status != SprintStatus.Completed))
            {
                st.AssigneeId = assigneeId;
            }

            await _context.SaveChangesAsync();
            return task;
        }

        public async System.Threading.Tasks.Task<Task?> GetTaskByIdAsync(int taskId)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Include(t => t.Reporter)
                .Include(t => t.Assignee)
                .Include(t => t.Project)
                    .ThenInclude(p => p.ProjectUsers)
                        .ThenInclude(pu => pu.User)
                .Include(t => t.SprintTasks)
                    .ThenInclude(st => st.Sprint)
                .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);
        }

        public async System.Threading.Tasks.Task<List<Worklog>> GetWorklogsByTaskIdAsync(int taskId)
        {
            return await _context.Worklogs
                .AsNoTracking()
                .Where(w => w.TaskId == taskId && !w.IsDeleted)
                .Include(w => w.User)
                .ToListAsync();
        }

        public async System.Threading.Tasks.Task<List<Comment>> GetNonReplyCommentsByTaskIdAsync(int taskId)
        {
            return await _context.Comments
                .Where(c => c.TaskId == taskId && c.ParentCommentId == null)
                .Include(c => c.Commenter)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.Commenter)
                .ToListAsync();
        }

        public async System.Threading.Tasks.Task<Comment> CreateTaskCommentAsync(
            string text, int taskId, int commenterId, int? parentCommentId)
        {
            var now = DateTime.UtcNow;

            var comment = new Comment
            {
                Text = text,
                TaskId = taskId,
                CommenterId = commenterId,
                ParentCommentId = parentCommentId,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async System.Threading.Tasks.Task DeleteTaskCommentAsync(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null || comment.IsDeleted)
                throw new Exception("Comment not found");
            comment.IsDeleted = true;
            comment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task EditTaskCommentAsync(int commentId, string newText)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null || comment.IsDeleted)
                throw new Exception("Comment not found");
            comment.Text = newText;
            comment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task DeleteTaskAsync(int taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null || task.IsDeleted)
                throw new Exception("Task not found");
            task.IsDeleted = true;
            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task UpdateTaskAssigneeAsync(int taskId, int? newAssigneeId)
        {
            var task = await _context.Tasks
                .Include(t => t.SprintTasks)
                    .ThenInclude(st => st.Sprint)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null || task.IsDeleted)
                throw new Exception("Task not found");

            task.AssigneeId = newAssigneeId;

            // aktualizujemy wszystkie nieusuniete sprinttaski z aktywnych i planowanych sprintow
            foreach (var st in task.SprintTasks.Where(st => !st.RemovedAt.HasValue && st.Sprint.Status != SprintStatus.Completed))
            {
                st.AssigneeId = newAssigneeId;
            }

            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task UpdateTaskStatusAsync(int taskId, TaskStatus newStatus)
        {
            var task = await _context.Tasks
                .Include(t => t.SprintTasks)
                    .ThenInclude(st => st.Sprint)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null || task.IsDeleted)
                throw new Exception("Task not found");

            task.Status = newStatus;

            // aktualizujemy wszystkie nieusuniete sprinttaski z aktywnych i planowanych sprintow
            foreach (var st in task.SprintTasks.Where(st => !st.RemovedAt.HasValue && st.Sprint.Status != SprintStatus.Completed))
            {
                st.Status = newStatus;
            }

            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task DeleteTaskWorklogAsync(int worklogId)
        {
            var worklog = await _context.Worklogs.FindAsync(worklogId);
            if (worklog == null || worklog.IsDeleted)
                throw new Exception("Worklog not found");
            worklog.IsDeleted = true;
            worklog.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}