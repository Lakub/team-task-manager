using Microsoft.EntityFrameworkCore;
using System.Windows.Input;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Models.Enums;
using Task = TeamTaskManager.Models.Entities.Task;
using TaskStatus = TeamTaskManager.Models.Enums.TaskStatus;

namespace TeamTaskManager.Services
{
    public interface IBacklogService
    {
        Task<Sprint?> GetSprintAsync(int sprintId);
        Task<Project?> GetProjectAsync(int projectId);
        Task<List<Task>> GetActiveSprintTasksAsync(int sprintId);
        Task<List<Task>> GetBacklogTasksAsync(int projectId);
        System.Threading.Tasks.Task AddTaskToSprintAsync(int sprintId, int taskId);
        System.Threading.Tasks.Task RemoveTaskFromSprintAsync(int sprintId, int taskId);
    }

    public class BacklogService : IBacklogService
    {
        private readonly AppDbContext _context;

        public BacklogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Sprint?> GetSprintAsync(int sprintId)
        {
            return await _context.Sprints.FirstOrDefaultAsync(s => s.Id == sprintId);
        }

        public async Task<Project?> GetProjectAsync(int projectId)
        {
            return await _context.Projects
                .Include(p => p.ProjectUsers)
                .FirstOrDefaultAsync(p => p.Id == projectId);
        }

        public async Task<List<Task>> GetActiveSprintTasksAsync(int sprintId)
        {
            return await _context.SprintTasks
                .Include(st => st.Task)
                .Where(st => st.SprintId == sprintId    // tylko z tego sprintu
                          && st.RemovedAt == null)      // tylko nieusuniete ze sprintu
                .Select(st => st.Task)
                .ToListAsync();
        }

        public async Task<List<Task>> GetBacklogTasksAsync(int projectId)
        {
            var sprintTaskIds = await _context.SprintTasks
                .Where(st => st.Sprint.ProjectId == projectId && st.RemovedAt == null)
                .Select(st => st.TaskId)
                .ToListAsync();

            return await _context.Tasks
                .Where(t => t.ProjectId == projectId
                         && !t.IsDeleted                    // tylko nieusuniete
                         && t.Status != TaskStatus.Closed   // tylko otwarte lub w trakcie
                         && !sprintTaskIds.Contains(t.Id))  // tylko te, ktore nie sa w zadnym sprincie
                .ToListAsync();
        }

        public async System.Threading.Tasks.Task AddTaskToSprintAsync(int sprintId, int taskId)
        {
            var sprint = await _context.Sprints.FirstOrDefaultAsync(s => s.Id == sprintId);
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
            var addedBy = App.CurrentUser;

            if (sprint == null || task == null || addedBy == null)
            {
                throw new InvalidOperationException("Sprint, Task, or AddedBy user not found.");
            }

            var sprintTask = new SprintTask
            {
                SprintId = sprint.Id,
                TaskId = task.Id,
                AddedAt = DateTime.UtcNow,
                AssigneeId = task.Assignee?.Id,
                Status = task.Status,
                AddedById = addedBy.Id
            };

            _context.SprintTasks.Add(sprintTask);
            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task RemoveTaskFromSprintAsync(int sprintId, int taskId)
        {
            var sprintTask = await _context.SprintTasks
                .FirstOrDefaultAsync(st => st.SprintId == sprintId  // z tego sprintu
                                        && st.TaskId == taskId      // konkretny task
                                        && st.RemovedAt == null);   // tylko nieusuniete ze sprintu

            if (sprintTask != null)
            {
                sprintTask.RemovedAt = DateTime.UtcNow;             // staje sie usuniete
                await _context.SaveChangesAsync();
            }
        }
    }
}