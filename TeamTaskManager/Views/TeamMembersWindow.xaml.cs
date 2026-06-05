using FlappyBirdClone;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;

namespace TeamTaskManager.Views
{
    /// <summary>
    /// Interaction logic for TeamMembersWindow.xaml
    /// </summary>
    public partial class TeamMembersWindow : Window
    {
        ObservableCollection<User> users;
        string easterEgg;
        public TeamMembersWindow(Project project)
        {
            InitializeComponent();
            users = new();
            easterEgg="";
            using (var context = new AppDbContext())
            {
                foreach (var pUser in project.ProjectUsers)
                {
                    var user = context.Users.FirstOrDefault(e => !e.IsDeleted && pUser.UserId == e.Id);
                    if (user != null)
                        users.Add(user);
                }
            }
            UsersTable.ItemsSource = users;
            KeyDown += GetKeyInput;
        }
        void GetKeyInput(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F: easterEgg = easterEgg + "f" ; break;
                case Key.L: easterEgg = easterEgg + "l"; break;
                case Key.A: easterEgg = easterEgg + "a"; break;
                case Key.P: easterEgg = easterEgg + "p"; break;
                case Key.Y: easterEgg = easterEgg + "y"; break;
                default:easterEgg = "";break;
            }
            if(easterEgg!="" && "flappy".Contains(easterEgg))
            {
                if ("flappy".Length == easterEgg.Length) {
                    new FlappyBirdWindow().Show();
                    easterEgg = "";
                }
                if(easterEgg.Length > "flappy".Length)
                    easterEgg = "";
            }
            else if (!"flappy".Contains(easterEgg))
            {
                easterEgg = "";
            }
            if (easterEgg == "" && e.Key == Key.F)
                easterEgg = "f";
        }
        private ListCollectionView View
        {
            get
            { return (ListCollectionView)CollectionViewSource.GetDefaultView(users); }
        }
        private void Filter(object sender, RoutedEventArgs e)
        {
            View.Filter = delegate (object item)
            {
                User user = item as User;
                if (user != null)
                {
                    return user.FullName.ToLower().Contains(UserNameFilterBox.Text.ToLower());
                }
                return false;
            };
        }
        void CopyEmail(object sender,  RoutedEventArgs e)
        {
            User user = (sender as Button).Tag as User;
            if (user != null)
                Clipboard.SetText(user.Email);
        }

    }
}
