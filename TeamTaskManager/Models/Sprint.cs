using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeamTaskManager.Models
{
    class Sprint
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(14);
        public SprintStatus Status { get; set; } = SprintStatus.Planned;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // projekt
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }

        // taski
        public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
    }
}

enum SprintStatus
{
    Planned,
    Active,
    Completed
}
