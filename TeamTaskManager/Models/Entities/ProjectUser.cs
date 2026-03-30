using TeamTaskManager.Models.Enums;

namespace TeamTaskManager.Models.Entities
{
    public class ProjectUser
    {
        // projekt
        public int ProjectId { get; set; }
        public virtual required Project Project { get; set; }

        // uzytkownik
        public int UserId { get; set; }
        public virtual required User User { get; set; }

        public UserRole Role { get; set; } = UserRole.Developer;
    }
}
