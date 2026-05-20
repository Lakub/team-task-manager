using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Models.Enums;
using TeamTaskManager.Services;
using static System.Reflection.Metadata.BlobBuilder;

namespace TeamTaskManager.Views
{
    public class CheckedToColorConverter : IValueConverter
    {
        public Brush unselectedUserBorderBrush { get; set; }
        public Brush unselectedUserNameBrush { get; set; }
        public Brush selectedMemberBackgroundBrush { get; set; }
        public Brush selectedMemberBorderBrush { get; set; }
        public Brush selectedMemberNameBrush { get; set; }
        public Brush selectedManagerBackgroundBrush { get; set; }
        public Brush selectedManagerBorderBrush { get; set; }
        public Brush selectedManagerNameBrush { get; set; }

        public Collection<User> assignedUsers { get; set; }
        public Collection<User> assignedPMs { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush pmB = null;
            Brush memberB = null;
            Brush userB = null;
            if ((string)parameter == "border")
            {
                pmB = selectedManagerBorderBrush;
                memberB = selectedMemberBorderBrush;
                userB = unselectedUserBorderBrush;
            }
            else if((string)parameter == "background")
            {
                pmB = selectedManagerBackgroundBrush;
                memberB = selectedMemberBackgroundBrush;
                userB = Brushes.Transparent;
            }
            else
            {
                pmB = selectedManagerNameBrush;
                memberB = selectedMemberNameBrush;
                userB = unselectedUserNameBrush;
            }
            if (assignedUsers == null || assignedPMs == null) return userB;
            if (assignedPMs.Any(e => e.Id == (int)value))
                    return pmB;
            if (assignedUsers.Any(e => e.Id == (int)value))
                return memberB;
            return userB;
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
        readonly ObservableCollection<User> comboboxUsers;
        public Project editedProject;
        User? editedProjectsOwner;
        bool editing;
        private ListCollectionView View
        {
            get
            { return (ListCollectionView)CollectionViewSource.GetDefaultView(allUsers); }
        }
        public CreateProjectWindow(bool editing=false, Project? project=null)
        {
            InitializeComponent();
            var context = new AppDbContext();
            allUsers = new ObservableCollection<User>(context.Users.Where(u => !u.IsDeleted && u.OrgRole!=OrgRole.HeadAdmin && u.Id != App.CurrentUser!.Id));
            this.editing=editing;
            if (editing)
            {
                comboboxUsers = new ObservableCollection<User>(allUsers);
                editedProject = project;
                ProjectOwnerAssignRow.DataContext = editedProject;
                var projectOwners = editedProject.ProjectUsers.Where(e => e.Role == UserRole.Owner).Select(e=>e.UserId);
                editedProjectsOwner = allUsers.FirstOrDefault(e => projectOwners.Contains(e.Id));
                if(editedProjectsOwner != null)
                    allUsers.Remove(editedProjectsOwner);
                AllUsersComboBox.ItemsSource = comboboxUsers;
                AllUsersComboBox.SelectedItem= editedProjectsOwner;
                Title = "Edytowanie projektu";
                FormTitle.Text = "Edytuj projekt";
                CreateButton.Content = "Zapisz";
                TitleTextBox.Text = project.Name;
                var assignedUsersIds = project.ProjectUsers.Where(e => e.Role == UserRole.Developer).Select(e => e.UserId);
                foreach (var userId in assignedUsersIds)
                {
                    var user = allUsers.FirstOrDefault(e => e.Id == userId);
                    if (user == null) continue;
                    allUsers.Remove(user);
                    assignedUsers.Add(user);
                    allUsers.Insert(0, user);
                }
                assignedUsersIds = project.ProjectUsers.Where(e => e.Role == UserRole.Manager).Select(e => e.UserId);
                foreach (var userId in assignedUsersIds)
                {
                    var user = allUsers.FirstOrDefault(e => e.Id == userId);
                    if (user == null) continue;
                    allUsers.Remove(user);
                    allUsers.Insert(0, user);
                    assignedUsers.Remove(user);
                    assignedProjectManagers.Add(user);
                    assignedUsers.Insert(0, user);
                }
            }
            (Resources["CheckedToColorConverter"] as CheckedToColorConverter).assignedUsers = assignedUsers;
            (Resources["CheckedToColorConverter"] as CheckedToColorConverter).assignedPMs = assignedProjectManagers;

            usersList.ItemsSource = allUsers;
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

        void SelectUser(object sender, RoutedEventArgs e)
        {
            User user;
            user = usersList.SelectedItem as User;
                usersList.SelectedItem = null;
            
            if (user == null) return;
            allUsers.Remove(user);
            if (assignedProjectManagers.Contains(user))
            {
                assignedProjectManagers.Remove(user);
                assignedUsers.Remove(user);
                allUsers.Insert(assignedUsers.Count, user);
            }
            else if (assignedUsers.Contains(user))
            {
                assignedProjectManagers.Add(user);
                allUsers.Insert(0, user);
            }
            else
            {
                assignedUsers.Add(user);
                allUsers.Insert(assignedProjectManagers.Count, user);
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

            if (!editing)
                await projectService.CreateProjectAsync(name, description, App.CurrentUser!, members);
            else
            {
                if (editedProjectsOwner == null)
                {
                    MessageBox.Show("Projekt musi mieć właściciela.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                editedProject.Owner = editedProjectsOwner;
                editedProject.OwnerId = editedProjectsOwner.Id;
                await projectService.EditProjectAsync(name, description, editedProject, members);
            }
            DialogResult = true;
        }
     
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void AllUsersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            allUsers.Insert(assignedUsers.Count, editedProjectsOwner);
            editedProjectsOwner = AllUsersComboBox.SelectedItem as User;
            assignedUsers.Remove(editedProjectsOwner);
            assignedProjectManagers.Remove(editedProjectsOwner);
            allUsers.Remove(editedProjectsOwner);
        }
    }
}