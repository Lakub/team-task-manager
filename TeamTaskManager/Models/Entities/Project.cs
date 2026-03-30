using TeamTaskManager.Models.Enums;

namespace TeamTaskManager.Models.Entities
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // wlasciciel
        public int OwnerId { get; set; }
        public virtual required User Owner { get; set; }

        // sprinty
        public virtual ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();

        // taski (backlog)
        public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

        // czlonkowie teamu
        public virtual ICollection<ProjectUser> ProjectUsers { get; set; } = new List<ProjectUser>();
    }
}
