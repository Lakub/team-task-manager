using TeamTaskManager.Models.Enums;

namespace TeamTaskManager.Models.Entities
{
    public class Attachment
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public bool IsPinned { get; set; } = false;

        // komentarz
        public int? CommentId { get; set; }
        public virtual Comment? Comment { get; set; }

        // task
        public int? TaskId { get; set; }
        public virtual Task? Task { get; set; }

        // autor
        public int UploaderId { get; set; }
        public virtual required User Uploader { get; set; }
    }
}
