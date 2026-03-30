using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamTaskManager.Models
{
    class Project
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // sprinty
        public virtual ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();

        // taski
        public virtual ICollection<Task> Backlog { get; set; } = new List<Task>();

        // czlonkowie
        public virtual ICollection<User> TeamMembers { get; set; } = new List<User>();

        // wlasciciel
        public int OwnerId { get; set; }
        public virtual User Owner { get; set; }
    }
}
