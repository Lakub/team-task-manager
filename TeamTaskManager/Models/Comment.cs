using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamTaskManager.Models
{
    class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsPinned { get; set; } = false;

        // task
        public int TaskId { get; set; }
        public virtual Task Task { get; set; }

        // autor
        public int UserId { get; set; }
        public virtual User User { get; set; }

        // odpowiedzi
        public int? ParentCommentId { get; set; }
        public virtual Comment ParentComment { get; set; }
        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();

        // zalaczniki
        public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
