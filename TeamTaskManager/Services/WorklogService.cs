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
    public interface IWorklogService
    {
        System.Threading.Tasks.Task<Worklog> CreateWorklogAsync(
            string description, DateTime startTime, string timeSpentText, TimeSpan timeSpent, int taskId, int userId);
        System.Threading.Tasks.Task<Worklog> EditWorklogAsync(
            int worklogId, string description, DateTime startTime, string timeSpentText, TimeSpan timeSpent);
        System.Threading.Tasks.Task<Worklog?> GetWorklogByIdAsync(int worklogId);
    }

    public class WorklogService : IWorklogService
    {
        private readonly AppDbContext _context;

        public WorklogService(AppDbContext context)
        {
            _context = context;
        }

        public async System.Threading.Tasks.Task<Worklog> CreateWorklogAsync(
            string description, DateTime startTime, string timeSpentText, TimeSpan timeSpent, int taskId, int userId)
        {
            var user = await _context.Users.FindAsync(userId) ?? throw new Exception("User not found");
            var task = await _context.Tasks.FindAsync(taskId) ?? throw new Exception("Task not found");

            var now = DateTime.UtcNow;

            var worklog = new Worklog
            {
                Description = description,
                StartTime = startTime,
                TimeSpentText = timeSpentText,
                TimeSpent = timeSpent,
                Task = task,
                User = user,
                LoggedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            };

            _context.Worklogs.Add(worklog);
            await _context.SaveChangesAsync();
            return worklog;
        }

        public async System.Threading.Tasks.Task<Worklog> EditWorklogAsync(
            int worklogId, string description, DateTime startTime, string timeSpentText, TimeSpan timeSpent)
        {
            var worklog = await _context.Worklogs.FindAsync(worklogId) ?? throw new Exception("Worklog not found");

            worklog.Description = description;
            worklog.StartTime = startTime;
            worklog.TimeSpentText = timeSpentText;
            worklog.TimeSpent = timeSpent;
            worklog.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return worklog;
        }

        public async System.Threading.Tasks.Task<Worklog?> GetWorklogByIdAsync(int worklogId)
        {
            return await _context.Worklogs
                .Include(w => w.User)
                .Include(w => w.Task)
                    .ThenInclude(t => t.Project)
                .FirstOrDefaultAsync(w => w.Id == worklogId);
        }
    }
}