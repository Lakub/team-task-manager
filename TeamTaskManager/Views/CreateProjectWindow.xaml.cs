using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Models.Enums;
using TeamTaskManager.Services;

namespace TeamTaskManager.Views
{
    public partial class CreateProjectWindow : Window
    {
        private readonly ObservableCollection<User> _assignedUsers = new();

        public CreateProjectWindow()
        {
            InitializeComponent();
            LoadUsers();
            AssignedList.ItemsSource = _assignedUsers;
        }

        private void LoadUsers()
        {
            using var context = new AppDbContext();
            var users = context.Users
                .Where(u => !u.IsDeleted && u.Id != App.CurrentUser!.Id)
                .ToList();
            UserComboBox.ItemsSource = users;
            UserComboBox.DisplayMemberPath = "FullName";
        }

        private void UserComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UserComboBox.SelectedItem is not User user) return;
            if (_assignedUsers.Any(u => u.Id == user.Id)) return;
            _assignedUsers.Add(user);
            UserComboBox.SelectedItem = null;
        }

        private void RemoveUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is User user)
                _assignedUsers.Remove(user);
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

            var members = _assignedUsers
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