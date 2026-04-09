using TeamTaskManager.Models.Enums;
using TaskStatus = TeamTaskManager.Models.Enums.TaskStatus;

namespace TeamTaskManager.Models.Entities
{
    public class SprintTask
    {
        // sprint
        public int SprintId { get; set; }
        public virtual required Sprint Sprint { get; set; }

        // task
        public int TaskId { get; set; }
        public virtual required Task Task { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RemovedAt { get; set; }

        // jesli task byl descopowany lub nieskonczony w sprincie 2,
        // a potem dodany do sprintu 5 i tam skonczony,
        // to w raporcie sprintu 2 bedzie, ze byl nieskonczony,
        // pomimo ze sam task jest juz skonczony
        public TaskStatus Status { get; set; }
        public bool IsDeleted { get; set; } = false;

        // ostatni assignee, adekwatnie do Status
        public int? AssigneeId { get; set; }
        public virtual User? Assignee { get; set; }

        // kto dodal
        public int AddedById { get; set; }
        public required virtual User AddedBy { get; set; }

        // kto usunal
        public int? RemovedById { get; set; }
        public virtual User? RemovedBy { get; set; }
    }
}

