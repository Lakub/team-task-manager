using TeamTaskManager.Models.Enums;

namespace TeamTaskManager.Models.Entities
{
    public class Worklog
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public TimeSpan TimeSpent { get; set; } = TimeSpan.Zero;
        public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

        // task
        public int TaskId { get; set; }
        public virtual required Task Task { get; set; }

        // developer
        public int UserId { get; set; }
        public virtual required User User { get; set; }
    }
}
