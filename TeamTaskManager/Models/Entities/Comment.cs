using TeamTaskManager.Models.Enums;

namespace TeamTaskManager.Models.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsPinned { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

        // task
        public int TaskId { get; set; }
        public virtual required Task Task { get; set; }

        // autor
        public int CommenterId { get; set; }
        public virtual required User Commenter { get; set; }

        // odpowiedzi
        public int? ParentCommentId { get; set; }
        public virtual Comment? ParentComment { get; set; }
        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();

        // zalaczniki
        public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
