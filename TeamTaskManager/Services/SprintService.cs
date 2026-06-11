using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Models.Enums;

namespace TeamTaskManager.Services
{
    public interface ISprintService
    {
        Task<(Sprint Sprint, List<SprintTask> Tasks)> GetSprintReportDataAsync(int sprintId);
        Task<List<Sprint>> GetAllSprintsAsync();
        Task<Sprint?> GetSprintByIdAsync(int sprintId);
        Task<List<SprintTask>> GetActiveSprintTasksAsync(int sprintId);
        Task<List<Sprint>> GetAllSprintsByProjectIdAsync(int projectId);
        Task<Sprint> CreateSprintAsync(string name, DateTime? startDate, DateTime? endDate, int projectId, int creatorId);
    }

    public class SprintService : ISprintService
    {
        private readonly AppDbContext _context;

        public SprintService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(Sprint Sprint, List<SprintTask> Tasks)> GetSprintReportDataAsync(int sprintId)
        {
            var sprint = await _context.Sprints
                .Include(s => s.Project)
                .Include(s => s.Creator)
                .FirstOrDefaultAsync(s => s.Id == sprintId);

            var sprintTasks = await _context.SprintTasks
                .Include(st => st.Task)
                    .ThenInclude(t => t.Worklogs)
                        .ThenInclude(w => w.User)
                .Include(st => st.Task)
                .Include(st => st.Assignee)
                .Where(st => st.SprintId == sprintId)
                .ToListAsync();

            var deduplicated = sprintTasks
                .GroupBy(st => st.TaskId)
                .Select(g => g.OrderByDescending(st => st.AddedAt).First())
                .ToList();

            return (sprint!, deduplicated);
        }
        public async Task<Sprint?> GetSprintByIdAsync(int sprintId)
        {
            return await _context.Sprints
                .FirstOrDefaultAsync(s => s.Id == sprintId);
        }

        public async Task<List<SprintTask>> GetActiveSprintTasksAsync(int sprintId)
        {
            return await _context.SprintTasks
                .AsNoTracking()
                .Include(st => st.Task)
                    .ThenInclude(t => t.Project)
                .Include(st => st.Assignee)
                .Where(st => st.SprintId == sprintId
                          && !st.RemovedAt.HasValue)
                .ToListAsync();
        }

        public async Task<List<Sprint>> GetAllSprintsAsync()
        {
            return await _context.Sprints.ToListAsync();
        }

        public async Task<List<Sprint>> GetAllSprintsByProjectIdAsync(int projectId)
        {
            return await _context.Sprints
                .Where(s => s.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<Sprint> CreateSprintAsync(
            string name, DateTime? startDate, DateTime? endDate,
            int projectId, int creatorId)
        {
            var creator = await _context.Users.FindAsync(creatorId);
            var project = await _context.Projects.FindAsync(projectId);

            var sprint = new Sprint
            {
                Name = name,
                StartDate = startDate,
                EndDate = endDate,
                Status = SprintStatus.Planned,
                Creator = creator!,
                Project = project!,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Sprints.Add(sprint);
            await _context.SaveChangesAsync();
            return sprint;
        }
    }
}
