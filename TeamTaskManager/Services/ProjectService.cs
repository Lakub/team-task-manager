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
    public interface IProjectService
    {
        Task<(Project Project, List<Sprint> Sprints)> GetSprintsByProjectIdAsync(int projectId);
        System.Threading.Tasks.Task<List<User>> GetMembersByProjectIdAsync(int projectId);
        Task<List<Project>> GetAllProjectsAsync();
        Task<List<Project>> GetNonDeletedProjectsWithSprintsByUserIdAsync(int userId);
        System.Threading.Tasks.Task<Project> CreateProjectAsync(string name, string description, User owner, List<(User User, UserRole Role)> members);
        Task<List<Project>> GetAllProjectsWithProjectUsersAsync();
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
                        .ThenInclude(t => t.Worklogs)
                .Include(s => s.Creator)
                .Where(st => st.ProjectId == projectId)
                .ToListAsync();

            return (project!, sprints);
        }

        public async Task<List<User>> GetMembersByProjectIdAsync(int projectId)
        {
            return await _context.ProjectUsers
                .Include(pu => pu.User)
                .Where(pu => pu.ProjectId == projectId && !pu.IsDeleted)
                .Select(pu => pu.User)
                .ToListAsync();
        }

        public async Task<List<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects.ToListAsync();
        }

        public async Task<List<Project>> GetNonDeletedProjectsWithSprintsByUserIdAsync(int userId)
        {
            var projects = await _context.Projects
                .Include(p => p.Sprints)
                .Include(p => p.ProjectUsers)
                .Where(p => !p.IsDeleted && p.ProjectUsers.Any(pu => pu.UserId == userId))
                .ToListAsync();

            return projects;
        }

        public async System.Threading.Tasks.Task<Project> CreateProjectAsync(string name, string description, User owner, List<(User User, UserRole Role)> members)
        {
            var trackedOwner = await _context.Users.FindAsync(owner.Id);

            var project = new Project
            {
                Name = name,
                Description = description,
                Owner = trackedOwner!,
                CreatedAt = DateTime.UtcNow
            };

            project.ProjectUsers.Add(new ProjectUser
            {
                User = trackedOwner!,
                Project = project,
                Role = UserRole.Owner
            });

            foreach (var (user, role) in members)
            {
                if (user.Id == owner.Id) continue;
                var trackedUser = await _context.Users.FindAsync(user.Id);
                project.ProjectUsers.Add(new ProjectUser
                {
                    User = trackedUser!,
                    Project = project,
                    Role = role
                });
            }

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<List<Project>> GetAllProjectsWithProjectUsersAsync()
        {
            return await _context.Projects.Include(p => p.ProjectUsers).ToListAsync();
        }
    }
}
