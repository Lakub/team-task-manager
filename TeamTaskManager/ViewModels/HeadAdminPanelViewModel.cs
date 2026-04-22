using CommunityToolkit.Mvvm.ComponentModel;
using TeamTaskManager.Models;

namespace TeamTaskManager.ViewModels
{
    public partial class HeadAdminPanelViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool accessDenied;

        [ObservableProperty]
        private string accessMessage = string.Empty;

        [ObservableProperty]
        private int usersCount;

        [ObservableProperty]
        private int projectsCount;

        [ObservableProperty]
        private int tasksCount;

        [ObservableProperty]
        private int sprintsCount;

        [ObservableProperty]
        private int commentsCount;

        [ObservableProperty]
        private int worklogsCount;

        [ObservableProperty]
        private int attachmentsCount;

        [ObservableProperty]
        private int documentationCount;

        public bool HasAccess => !AccessDenied;

        public HeadAdminPanelViewModel()
        {
            Load();
        }

        private void Load()
        {
            if (!string.Equals(App.CurrentUser?.Email, "j.kowalski@email.com", System.StringComparison.OrdinalIgnoreCase))
            {
                AccessDenied = true;
                AccessMessage = "Dostęp do panelu HeadAdmin ma wyłącznie użytkownik z rolą HeadAdmin.";
                return;
            }

            using var context = new AppDbContext();

            AccessDenied = false;
            AccessMessage = string.Empty;

            UsersCount = context.Users.Count(u => !u.IsDeleted);
            ProjectsCount = context.Projects.Count(p => !p.IsDeleted);
            TasksCount = context.Tasks.Count(t => !t.IsDeleted);
            SprintsCount = context.Sprints.Count(s => !s.IsDeleted);
            CommentsCount = context.Comments.Count(c => !c.IsDeleted);
            WorklogsCount = context.Worklogs.Count(w => !w.IsDeleted);
            AttachmentsCount = context.Attachments.Count(a => !a.IsDeleted);
            DocumentationCount = 1;
        }
    }
}
