using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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
        System.Threading.Tasks.Task<List<Comment>> GetCommentsByTaskIdAsync(int taskId);
        System.Threading.Tasks.Task<Task> CreateTaskAsync(
            string title, string description,
            TaskType type, TaskPriority priority,
            int projectId, int reporterId, int? assigneeId);
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
            var reporter = await _context.Users.FindAsync(reporterId);
            var project = await _context.Projects.FindAsync(projectId);
            var maxPerProjectId = await _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .MaxAsync(t => (int?)t.PerProjectId) ?? 0;
            User? assignee = assigneeId.HasValue
                ? await _context.Users.FindAsync(assigneeId.Value)
                : null;

            var task = new Task
            {
                Title = title,
                Description = description,
                Type = type,
                Priority = priority,
                Status = TaskStatus.Open,
                Reporter = reporter!,
                Assignee = assignee,
                Project = project!,
                PerProjectId = maxPerProjectId + 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async System.Threading.Tasks.Task<Task?> GetTaskByIdAsync(int taskId)
        {
            return await _context.Tasks
                .Include(t => t.Reporter)
                .Include(t => t.Assignee)
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);
        }

        public async System.Threading.Tasks.Task<List<Worklog>> GetWorklogsByTaskIdAsync(int taskId)
        {
            return await _context.Worklogs
                .Where(w => w.TaskId == taskId && !w.IsDeleted)
                .Include(w => w.User)
                .ToListAsync();
        }

        public async System.Threading.Tasks.Task<List<Comment>> GetCommentsByTaskIdAsync(int taskId)
        {
            return await _context.Comments
                .Where(c => c.TaskId == taskId && !c.IsDeleted)
                .Include(c => c.Commenter)
                .ToListAsync();
        }
    }
}