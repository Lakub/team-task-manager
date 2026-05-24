using System.Windows;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Models.Enums;
using TeamTaskManager.Views;

namespace TeamTaskManager
{
    public partial class App : Application
    {
        public static User? CurrentUser { get; set; }
        public static bool IsLoggingOut { get; set; } = false;

        protected void OnStartup(object sender, StartupEventArgs e)
        {
            var login = new LoginWindow();

            if (login.ShowDialog() == true)
            {
                CurrentUser = login.LoggedInUser;

                Window next = CurrentUser?.OrgRole == OrgRole.HeadAdmin
                    ? new HeadAdminWindow()
                    : new MainWindow();

                next.Closed += (s, args) => { if (!IsLoggingOut) Shutdown(); };
                next.Show();
            }
            else
            {
                Shutdown();
            }
        }
    }
}
