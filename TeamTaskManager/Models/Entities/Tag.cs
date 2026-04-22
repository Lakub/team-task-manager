using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TeamTaskManager.Models.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

       
        public virtual ICollection<WikiArticle> Articles { get; set; } = new List<WikiArticle>();
    }
}
