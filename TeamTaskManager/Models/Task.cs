using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeamTaskManager.Models
{
    class Task
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskType Type { get; set; } = TaskType.Feature;
        public TaskStatus Status { get; set; } = TaskStatus.Open;
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // zgloszone przez
        public int ReporterId { get; set; }
        public User Reporter { get; set; }

        // zlecony do
        public int? AssigneeId { get; set; }
        public User Assignee { get; set; }

        // hierarchia taskow
        public Task? ParentTask { get; set; }
        public virtual ICollection<Task> SubTasks { get; set; } = new List<Task>();

        // sprint
        public int? SprintId { get; set; }
        public virtual Sprint Sprint { get; set; }

        // projekt
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }

        // komentarze
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        // zalaczniki
        public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

        public override string ToString()
        {
            return $"{Title}, {Description}, {Reporter.FullName}, {Type}, {Status}, {Priority}";
        }
    }
}

enum TaskType
{
    Bug,
    Feature
}

enum TaskStatus
{
    Open,
    InProgress,
    Closed
}

enum TaskPriority
{
    Low,
    Medium,
    High
}
