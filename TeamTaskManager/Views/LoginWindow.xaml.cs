using System.Windows;
using System.Windows.Input;
using TeamTaskManager.Helpers;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Services;
using TeamTaskManager.ViewModels;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;

namespace TeamTaskManager.Views
{
    public partial class LoginWindow : Window
    {
        public User? LoggedInUser { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var email = EmailBox.Text.Trim();
            var password = PasswordBox.Password;

            using var context = new AppDbContext();
            var authService = new AuthService(context);
            var user = authService.Validate(email, password);

            if (user != null)
            {
                LoggedInUser = user;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Nieprawidłowy email lub hasło.", "Błąd logowania", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new RegisterWindow();
            window.Owner = this;
            window.ShowDialog();
        }

        // TEMP
        private void TempRandomSeedDb_Click(object sender, RoutedEventArgs e)
        {
            using var context = new AppDbContext();
            var authService = new AuthService(context);
            SeedData.RandomSeed(context, authService);
            System.Windows.MessageBox.Show("Sukces", "Sukces", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private void TempClearDb_Click(object sender, RoutedEventArgs e)
        {
            using var context = new AppDbContext();
            SeedData.Clear(context);
            System.Windows.MessageBox.Show("Sukces", "Sukces", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        // TEMP
        private void TempSeeUsers_Click(object sender, RoutedEventArgs e)
        {
            using var context = new AppDbContext();
            var userData = context.UserAuths
                .Select(ua => new
                {
                    ua.UserId,
                    ua.User.Email,
                    ua.User.OrgRole,
                    WorklogCount = ua.User.Worklogs.Count()
                })
                .ToList();

            var message = "Hasło: password\n" + string.Join("\n", userData.Select(d => $"{d.UserId}: {d.Email} [{d.OrgRole}], Worklogów: {d.WorklogCount}"));

            MessageBox.Show(message, "Users", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // TEMP
        private void TempAddUser_Click(object sender, RoutedEventArgs e)
        {
            var email = EmailBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Email i hasło nie mogą być puste.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var context = new AppDbContext();
            var authService = new AuthService(context);

            if (authService.UserExists(email))
            {
                MessageBox.Show("Użytkownik z tym emailem już istnieje.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                var salt = authService.GenerateSalt();
                var hashedPassword = authService.HashPassword(password, salt);

                var user = new User
                {
                    Email = email,
                    FullName = email.Split('@')[0]
                };
                context.Users.Add(user);
                context.SaveChanges();

                var userAuth = new UserAuth
                {
                    UserId = user.Id,
                    PasswordSalt = salt,
                    Password = hashedPassword
                };
                context.UserAuths.Add(userAuth);
                context.SaveChanges();

                MessageBox.Show("Użytkownik został dodany.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}