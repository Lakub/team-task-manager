namespace TeamTaskManager.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

        // projekty
        public virtual ICollection<ProjectUser> ProjectUsers { get; set; } = new List<ProjectUser>();

        // utworzone komentarze
        public virtual ICollection<Comment> CreatedComments { get; set; } = new List<Comment>();

        // przypisane taski
        public virtual ICollection<Task> AssignedTasks { get; set; } = new List<Task>();

        // zgloszone taski
        public virtual ICollection<Task> ReportedTasks { get; set; } = new List<Task>();

        // utworzone worklogi
        public virtual ICollection<Worklog> Worklogs { get; set; } = new List<Worklog>();

        // utworzone zalaczniki
        public virtual ICollection<Attachment> UploadedAttachments { get; set; } = new List<Attachment>();

        // utworzone sprinty
        public virtual ICollection<Sprint> CreatedSprints { get; set; } = new List<Sprint>();

        // utworzone projekty
        public virtual ICollection<Project> OwnedProjects { get; set; } = new List<Project>();

        // taski dodane do sprintow
        public virtual ICollection<SprintTask> AddedSprintTasks { get; set; } = new List<SprintTask>();

        // taski usuniete ze sprintow
        public virtual ICollection<SprintTask> RemovedSprintTasks { get; set; } = new List<SprintTask>();

        // przypisane sprinttaski
        public virtual ICollection<SprintTask> AssignedSprintTasks { get; set; } = new List<SprintTask>();

        public override string ToString()
        {
            return $"{FullName} ({Email})";
        }
    }
}
