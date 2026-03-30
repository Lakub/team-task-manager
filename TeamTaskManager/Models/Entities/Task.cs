using TeamTaskManager.Models.Enums;

namespace TeamTaskManager.Models.Entities
{
    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskType Type { get; set; } = TaskType.Feature;
        public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Open;
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // zgloszone przez
        public int ReporterId { get; set; }
        public virtual required User Reporter { get; set; }

        // zlecony do
        public int? AssigneeId { get; set; }
        public virtual User? Assignee { get; set; }

        // hierarchia taskow
        public int? ParentTaskId { get; set; }
        public virtual Task? ParentTask { get; set; }
        public virtual ICollection<Task> SubTasks { get; set; } = new List<Task>();

        // sprint
        public int? SprintId { get; set; }
        public virtual Sprint? Sprint { get; set; }

        // projekt
        public int ProjectId { get; set; }
        public virtual required Project Project { get; set; }

        // worklogi
        public virtual ICollection<Worklog> Worklogs { get; set; } = new List<Worklog>();

        // komentarze
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        // zalaczniki
        public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

        public override string ToString()
        {
            return $"{Title}, {Description}, {Reporter?.FullName}, {Type}, {Status}, {Priority}";
        }
    }
}

