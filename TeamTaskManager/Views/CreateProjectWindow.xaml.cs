using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Models.Enums;
using TeamTaskManager.Services;

namespace TeamTaskManager.Views
{
    public class CheckedToColorConverter : IValueConverter
    {
        public Brush unselectedUserBorderBrush { get; set; }
        public Brush selectedUserBorderBrush { get; set; }
        public Brush selectedUserBackgroundBrush { get; set; }
        public Brush selectedUserNameBrush { get; set; }
        public Brush unselectedUserNameBrush { get; set; }
        public List<User> assignedUsers { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string)parameter == "border")
            {
                if (assignedUsers == null) return unselectedUserBorderBrush;
                if (assignedUsers.Any(e => e.Email == (string)value))
                    return selectedUserBorderBrush;
                return unselectedUserBorderBrush;
            }
            else if((string)parameter == "background")
            {
                if (assignedUsers == null) return null;
                if (assignedUsers.Any(e => e.Email == (string)value))
                    return selectedUserBackgroundBrush;
                return null;
            }
            else
            {
                if (assignedUsers == null) return unselectedUserNameBrush;
                if (assignedUsers.Any(e => e.Email == (string)value))
                    return selectedUserNameBrush;
                return unselectedUserNameBrush;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    public partial class CreateProjectWindow : Window
    {
        readonly List<User> assignedUsers = new();
        readonly ObservableCollection<User> allUsers;

        public CreateProjectWindow()
        {
            InitializeComponent();
            (Resources["CheckedToColorConverter"] as CheckedToColorConverter).assignedUsers = assignedUsers;
            using var context = new AppDbContext();
            allUsers = new ObservableCollection<User>(context.Users.Where(u => !u.IsDeleted && u.Id != App.CurrentUser!.Id).ToList());
            usersList.ItemsSource = allUsers;
        }

        void SelectUser(object sender, RoutedEventArgs e)
        {
            var user = usersList.SelectedItem as User;
            usersList.SelectedItem = null;
            if (user == null) return;
            allUsers.Remove(user);
            if (assignedUsers.Contains(user))
            {
                assignedUsers.Remove(user);
                allUsers.Add(user);
            }
            else
            {
                assignedUsers.Add(user);
                allUsers.Insert(0, user);
            }
        }
        private void SelectUser(object sender, MouseEventArgs e)
        {
            if (sender == null) return;
            if (!(sender is Grid)) return;
            Focus();
            var user = ((Grid)sender).Tag as User;
            allUsers.Remove(user);
            if (assignedUsers.Contains(user)){
                assignedUsers.Remove(user);
                allUsers.Add(user);
            }
            else
            {
                assignedUsers.Add(user);
                allUsers.Insert(0,user);
            }
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var name = TitleTextBox.Text.Trim();
            var description = DescriptionTextBox.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Nazwa projektu nie może być pusta.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var context = new AppDbContext();
            var projectService = new ProjectService(context);

            var members = assignedUsers
                .Select(u => (u, UserRole.Developer))
                .ToList();

            await projectService.CreateProjectAsync(name, description, App.CurrentUser!, members);
            DialogResult = true;
        }
     
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}