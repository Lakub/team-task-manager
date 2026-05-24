using System.Windows;
using TeamTaskManager.Models.Enums;

namespace TeamTaskManager.Views
{
    public partial class HeadAdminWindow : Window
    {
        public HeadAdminWindow()
        {
            InitializeComponent();
            UserNameText.Text = App.CurrentUser?.FullName ?? "Użytkownik";
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            App.IsLoggingOut = true;
            App.CurrentUser = null;
            Close();

            var login = new LoginWindow();
            if (login.ShowDialog() == true)
            {
                App.CurrentUser = login.LoggedInUser;
                App.IsLoggingOut = false;

                Window next = App.CurrentUser?.OrgRole == OrgRole.HeadAdmin
                    ? new HeadAdminWindow()
                    : new MainWindow();

                next.Closed += (s, args) => { if (!App.IsLoggingOut) Application.Current.Shutdown(); };
                next.Show();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }
    }
}
