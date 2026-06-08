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

            var worklog = new Worklog
            {
                Description = description,
                StartTime = startTime,
                TimeSpentText = timeSpentText,
                TimeSpent = timeSpent,
                Task = task,
                User = user,
                LoggedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Worklogs.Add(worklog);
            await _context.SaveChangesAsync();
            return worklog;
        }
    }
}