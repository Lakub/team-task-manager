using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Models.Enums;


namespace TeamTaskManager.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectUser> ProjectUsers { get; set; }
        public DbSet<Entities.Task> Tasks { get; set; }
        public DbSet<Sprint> Sprints { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Worklog> Worklogs { get; set; }
        public DbSet<SprintTask> SprintTasks { get; set; }
        public DbSet<UserAuth> UserAuths { get; set; }
        public DbSet<WikiArticle> WikiArticles { get; set; }
        public DbSet<Tag> Tags { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=app.db");

        public void EnsureHeadAdminSchema()
        {
            Database.EnsureCreated();

            using var connection = new SqliteConnection("Data Source=app.db");
            connection.Open();

            using var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "PRAGMA table_info(Users);";

            var hasSystemRoleColumn = false;

            using (var reader = checkCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (string.Equals(reader[1]?.ToString(), "SystemRole", StringComparison.OrdinalIgnoreCase))
                    {
                        hasSystemRoleColumn = true;
                        break;
                    }
                }
            }

            if (!hasSystemRoleColumn)
            {
                using var alterCommand = connection.CreateCommand();
                alterCommand.CommandText = "ALTER TABLE Users ADD COLUMN SystemRole INTEGER NOT NULL DEFAULT 2;";
                alterCommand.ExecuteNonQuery();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProjectUser>()
                .HasKey(pu => new { pu.ProjectId, pu.UserId });

            modelBuilder.Entity<ProjectUser>()
                .HasOne(pu => pu.Project)
                .WithMany(p => p.ProjectUsers)
                .HasForeignKey(pu => pu.ProjectId);

            modelBuilder.Entity<ProjectUser>()
                .HasOne(pu => pu.User)
                .WithMany(u => u.ProjectUsers)
                .HasForeignKey(pu => pu.UserId);

            modelBuilder.Entity<Entities.Task>()
                .HasOne(t => t.Reporter)
                .WithMany(u => u.ReportedTasks)
                .HasForeignKey(t => t.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Entities.Task>()
                .HasOne(t => t.Assignee)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssigneeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SprintTask>()
                .HasOne(st => st.Assignee)
                .WithMany(u => u.AssignedSprintTasks)
                .HasForeignKey(st => st.AssigneeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SprintTask>()
                .HasOne(st => st.AddedBy)
                .WithMany(u => u.AddedSprintTasks)
                .HasForeignKey(st => st.AddedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SprintTask>()
                .HasOne(st => st.RemovedBy)
                .WithMany(u => u.RemovedSprintTasks)
                .HasForeignKey(st => st.RemovedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Entities.Task>()
                .HasOne(t => t.ParentTask)
                .WithMany(t => t.SubTasks)
                .HasForeignKey(t => t.ParentTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Sprint>()
                .HasOne(s => s.Creator)
                .WithMany(u => u.CreatedSprints)
                .HasForeignKey(s => s.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Project>()
                .HasOne(p => p.Owner)
                .WithMany(u => u.OwnedProjects)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.Uploader)
                .WithMany(u => u.UploadedAttachments)
                .HasForeignKey(a => a.UploaderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Commenter)
                .WithMany(u => u.CreatedComments)
                .HasForeignKey(c => c.CommenterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
