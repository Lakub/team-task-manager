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
                CreatedAt = System.DateTime.UtcNow,
                UpdatedAt = System.DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }
    }
}