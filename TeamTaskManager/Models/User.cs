using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeamTaskManager.Models
{
    class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Developer;
        public string Email { get; set; } = string.Empty;
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // projekty
        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

        // utworzone komentarze
        public virtual ICollection<Comment> CreatedComments { get; set; } = new List<Comment>();

        // przypisane taski
        public virtual ICollection<Task> AssignedTasks { get; set; } = new List<Task>();

        // zgloszone taski
        public virtual ICollection<Task> ReportedTasks { get; set; } = new List<Task>();

        // utworzone zalaczniki
        public virtual ICollection<Attachment> CreatedAttachments { get; set; } = new List<Attachment>();

        // utworzone sprinty
        public virtual ICollection<Sprint> CreatedSprints { get; set; } = new List<Sprint>();

        // utworzone projekty
        public virtual ICollection<Project> CreatedProjects { get; set; } = new List<Project>();

        public override string ToString()
        {
            return $"{FullName} ({Email})";
        }
    }
}

enum UserRole
{
    Manager,
    Developer
}
