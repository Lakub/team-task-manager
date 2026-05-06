using System.Configuration;
using System.Data;
using System.Windows;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Views;

namespace TeamTaskManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
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
                var main = new MainWindow();
                main.Closed += (s, args) => { if (!IsLoggingOut) Shutdown(); };
                main.Show();
            }
            else
            {
                Shutdown();
            }
            
        }
       
    }
        
}
