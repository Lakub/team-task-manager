using System;
using System.Collections.Generic;

namespace TeamTaskManager.Models.Entities
{
    public class WikiArticle
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

       
        public bool IsDraft { get; set; } = false;
        public bool IsFavorite { get; set; } = false;


        public int? ParentArticleId { get; set; }
        public virtual WikiArticle? ParentArticle { get; set; }
        public virtual ICollection<WikiArticle> SubArticles { get; set; } = new List<WikiArticle>();

        
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }
}
