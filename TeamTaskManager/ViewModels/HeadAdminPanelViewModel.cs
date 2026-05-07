using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Enums;

namespace TeamTaskManager.ViewModels
{
    public class UserRoleRow : INotifyPropertyChanged
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        private OrgRole _role;
        public OrgRole Role
        {
            get => _role;
            set
            {
                _role = value;
                Notify(nameof(Role));
                Notify(nameof(RoleLabel));
                Notify(nameof(CanPromote));
                Notify(nameof(CanDemote));
            }
        }

        public string RoleLabel => Role switch
        {
            OrgRole.HeadAdmin => "HeadAdmin",
            OrgRole.Admin     => "Admin",
            _                 => "User"
        };

        public bool CanPromote => Role == OrgRole.User;
        public bool CanDemote  => Role == OrgRole.Admin;

        public ICommand PromoteCommand { get; }
        public ICommand DemoteCommand  { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void Notify(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public UserRoleRow(Action<UserRoleRow, OrgRole> onChange)
        {
            PromoteCommand = new RelayCommand(() => onChange(this, OrgRole.Admin));
            DemoteCommand  = new RelayCommand(() => onChange(this, OrgRole.User));
        }
    }

    public partial class HeadAdminPanelViewModel : ObservableObject
    {
        [ObservableProperty] private string accessMessage    = string.Empty;
        [ObservableProperty] private int    usersCount;
        [ObservableProperty] private int    projectsCount;
        [ObservableProperty] private int    tasksCount;
        [ObservableProperty] private int    sprintsCount;
        [ObservableProperty] private int    commentsCount;
        [ObservableProperty] private int    worklogsCount;
        [ObservableProperty] private int    attachmentsCount;
        [ObservableProperty] private int    documentationCount;

        private bool _accessDenied;
        public bool AccessDenied
        {
            get => _accessDenied;
            set
            {
                if (SetProperty(ref _accessDenied, value))
                {
                    OnPropertyChanged(nameof(HasAccess));
                    OnPropertyChanged(nameof(ShowStats));
                    OnPropertyChanged(nameof(ShowUserListPanel));
                }
            }
        }

        private bool _showingUserList;
        public bool ShowingUserList
        {
            get => _showingUserList;
            set
            {
                if (SetProperty(ref _showingUserList, value))
                {
                    OnPropertyChanged(nameof(ShowStats));
                    OnPropertyChanged(nameof(ShowUserListPanel));
                }
            }
        }

        public bool HasAccess       => !AccessDenied;
        public bool ShowStats       => HasAccess && !ShowingUserList;
        public bool ShowUserListPanel => HasAccess && ShowingUserList;

        public ObservableCollection<UserRoleRow> UserList { get; } = new();

        public ICommand ShowUsersCommand   { get; }
        public ICommand BackToStatsCommand { get; }

        public HeadAdminPanelViewModel()
        {
            ShowUsersCommand   = new RelayCommand(() => ShowingUserList = true);
            BackToStatsCommand = new RelayCommand(() => ShowingUserList = false);
            Load();
        }

        private void Load()
        {
            if (App.CurrentUser?.OrgRole != OrgRole.HeadAdmin)
            {
                AccessDenied = true;
                AccessMessage = "Dostęp do panelu HeadAdmin ma wyłącznie użytkownik z rolą HeadAdmin.";
                return;
            }

            using var context = new AppDbContext();

            AccessDenied    = false;
            AccessMessage   = string.Empty;
            ShowingUserList = false;

            UsersCount         = context.Users.Count(u => !u.IsDeleted);
            ProjectsCount      = context.Projects.Count(p => !p.IsDeleted);
            TasksCount         = context.Tasks.Count(t => !t.IsDeleted);
            SprintsCount       = context.Sprints.Count(s => !s.IsDeleted);
            CommentsCount      = context.Comments.Count(c => !c.IsDeleted);
            WorklogsCount      = context.Worklogs.Count(w => !w.IsDeleted);
            AttachmentsCount   = context.Attachments.Count(a => !a.IsDeleted);
            DocumentationCount = 1;

            UserList.Clear();
            foreach (var u in context.Users.Where(u => !u.IsDeleted).OrderBy(u => u.FullName).ToList())
            {
                UserList.Add(new UserRoleRow(ChangeUserRole)
                {
                    UserId   = u.Id,
                    FullName = u.FullName,
                    Email    = u.Email,
                    Role     = u.OrgRole
                });
            }
        }

        private void ChangeUserRole(UserRoleRow row, OrgRole newRole)
        {
            using var context = new AppDbContext();
            var user = context.Users.FirstOrDefault(u => u.Id == row.UserId);
            if (user == null || user.OrgRole == OrgRole.HeadAdmin) return;
            user.OrgRole = newRole;
            context.SaveChanges();
            row.Role = newRole;
        }
    }
}
