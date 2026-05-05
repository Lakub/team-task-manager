using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
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
        public Collection<User> assignedUsers { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string)parameter == "border")
            {
                if (assignedUsers == null) return unselectedUserBorderBrush;
                if (assignedUsers.Any(e => e.Id == (int)value))
                    return selectedUserBorderBrush;
                return unselectedUserBorderBrush;
            }
            else if((string)parameter == "background")
            {
                if (assignedUsers == null) return null;
                if (assignedUsers.Any(e => e.Id == (int)value))
                    return selectedUserBackgroundBrush;
                return null;
            }
            else
            {
                if (assignedUsers == null) return unselectedUserNameBrush;
                if (assignedUsers.Any(e => e.Id == (int)value))
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
        readonly ObservableCollection<User> assignedUsers = new();
        readonly Collection<User> assignedProjectManagers = new();
        readonly ObservableCollection<User> allUsers;

        public CreateProjectWindow()
        {
            InitializeComponent();
            (Resources["CheckedToColorConverter"] as CheckedToColorConverter).assignedUsers = assignedUsers;
            (Resources["CheckedToColorProjectConverter"] as CheckedToColorConverter).assignedUsers = assignedProjectManagers;
            
            using var context = new AppDbContext();
            allUsers = new ObservableCollection<User>(context.Users.Where(u => !u.IsDeleted && u.Id != App.CurrentUser!.Id).ToList());
            usersList.ItemsSource = allUsers;
            projectManagerList.ItemsSource = assignedUsers;
        }

        void SelectUser(object sender, RoutedEventArgs e)
        {
            Collection<User> higherCol;
            Collection<User> lowerCol;
            User user;
            if (((ListBox)sender).Name=="usersList"){
                user = usersList.SelectedItem as User;
                    usersList.SelectedItem = null;
                higherCol = allUsers;
                lowerCol = assignedUsers;
            }
            else
            {
                user = projectManagerList.SelectedItem as User;
                projectManagerList.SelectedItem = null;
                higherCol = assignedUsers;
                lowerCol = assignedProjectManagers;
            }
            if (user == null) return;
            higherCol.Remove(user);
            if (lowerCol.Contains(user))
            {
                lowerCol.Remove(user);
                if(lowerCol!=assignedProjectManagers)
                    assignedProjectManagers.Remove(user);
                higherCol.Add(user);
            }
            else
            {
                lowerCol.Add(user);
                higherCol.Insert(0, user);
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
            foreach (var user in assignedProjectManagers)
                assignedUsers.Remove(user);

            var members = assignedUsers
                .Select(u => (u, UserRole.Developer))
                .ToList();
            members.AddRange(assignedProjectManagers.Select(u => (u, UserRole.Manager)).ToList());

            await projectService.CreateProjectAsync(name, description, App.CurrentUser!, members);
            DialogResult = true;
        }
     
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}