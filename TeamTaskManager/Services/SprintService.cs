using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;

namespace TeamTaskManager.Services
{
    public interface ISprintService
    {
        Task<(Sprint Sprint, List<SprintTask> Tasks)> GetSprintReportDataAsync(int sprintId);
        Task<List<Sprint>> GetAllSprintsAsync();
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
                .Include(st => st.Task)
                    .ThenInclude(t => t.Assignee)
                .Where(st => st.SprintId == sprintId)
                .ToListAsync();

            return (sprint!, sprintTasks);
        }

        public async Task<List<Sprint>> GetAllSprintsAsync()
        {
            return await _context.Sprints.ToListAsync();
        }
    }
}
