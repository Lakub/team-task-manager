using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamTaskManager.Models
{
    class Attachment
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.Now;
        public bool IsPinned { get; set; } = false;

        // komentarz
        public int? CommentId { get; set; }
        public virtual Comment Comment { get; set; }

        // task
        public int? TaskId { get; set; }
        public virtual Task Task { get; set; }
    }
}
