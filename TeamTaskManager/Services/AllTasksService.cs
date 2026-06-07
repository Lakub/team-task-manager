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
    public interface IAllTasksService
    {
        System.Threading.Tasks.Task<List<Task>> GetAllProjectTasksAsync(int projectId);
    }

    public class AllTasksService : IAllTasksService
    {
        private readonly AppDbContext _context;

        public AllTasksService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Task>> GetAllProjectTasksAsync(int projectId)
        {
            return await _context.Tasks
                .Where(t => t.ProjectId == projectId && !t.IsDeleted)
                .OrderBy(t => t.PerProjectId)
                .ToListAsync();
        }
    }
}