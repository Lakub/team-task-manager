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
    public interface IProjectService
    {
        Task<(Project Project, List<Sprint> Sprints)> GetSprintsByProjectIdAsync(int projectId);
        Task<List<Project>> GetAllProjectsAsync();
    }

    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _context;

        public ProjectService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<(Project Project, List<Sprint> Sprints)> GetSprintsByProjectIdAsync(int projectId)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId);

            var sprints = await _context.Sprints
                .Include(s => s.SprintTasks)
                    .ThenInclude(st => st.Task)
                .Include(s => s.Creator)
                .Where(st => st.ProjectId == projectId)
                .ToListAsync();

            return (project!, sprints);
        }

        public async Task<List<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects.ToListAsync();
        }
    }
}
