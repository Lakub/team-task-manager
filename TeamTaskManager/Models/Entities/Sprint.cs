using TeamTaskManager.Models.Enums;

namespace TeamTaskManager.Models.Entities
{
    public class Sprint
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddDays(14);
        public SprintStatus Status { get; set; } = SprintStatus.Planned;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

        // utworzony przez
        public int CreatorId { get; set; }
        public virtual required User Creator { get; set; }

        // projekt
        public int ProjectId { get; set; }
        public virtual required Project Project { get; set; }

        // taski
        public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

        // sprint taski
        public virtual ICollection<SprintTask> SprintTasks { get; set; } = new List<SprintTask>();
        public override string ToString()
        {
            return $"Sprint od: {StartDate.ToString("dd.MM")} do: {EndDate.ToString("dd.MM")}";
        }
    }
}
