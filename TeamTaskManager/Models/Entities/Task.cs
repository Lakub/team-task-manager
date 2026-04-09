using TeamTaskManager.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

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
        public bool IsDeleted { get; set; } = false;

        [NotMapped]
        public string LastCommentText { get; set; } = string.Empty;

        [NotMapped]
        public string TotalTimeText { get; set; } = "0m";

        // zgloszone przez
        public int ReporterId { get; set; }
        public virtual required User Reporter { get; set; }

        // MOZE DEPRECATED BO CHYBA SPRINTTASK WYSTARCZA
        // zlecony do
        public int? AssigneeId { get; set; }
        public virtual User? Assignee { get; set; }

        // hierarchia taskow
        public int? ParentTaskId { get; set; }
        public virtual Task? ParentTask { get; set; }
        public virtual ICollection<Task> SubTasks { get; set; } = new List<Task>();

        // DEPRECATED BO JEST SPRINTTASK
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

        // sprint taski
        public virtual ICollection<SprintTask> SprintTasks { get; set; } = new List<SprintTask>();

        public override string ToString()
        {
            return $"{Title}, {Description}, {Reporter?.FullName}, {Type}, {Status}, {Priority}";
        }
    }
}