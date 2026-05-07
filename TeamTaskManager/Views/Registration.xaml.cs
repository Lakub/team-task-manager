using System.Windows;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Services;

namespace TeamTaskManager.Views
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var fullName = FullNameBox.Text.Trim();
            var email    = EmailBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowError("Wszystkie pola są wymagane.");
                return;
            }

            using var context = new AppDbContext();
            var authService = new AuthService(context);

            if (authService.UserExists(email))
            {
                ShowError("Użytkownik z tym emailem już istnieje.");
                return;
            }

            var salt           = authService.GenerateSalt();
            var hashedPassword = authService.HashPassword(password, salt);

            var user = new User { FullName = fullName, Email = email };
            context.Users.Add(user);
            context.SaveChanges();

            context.UserAuths.Add(new UserAuth
            {
                UserId       = user.Id,
                PasswordSalt = salt,
                Password     = hashedPassword
            });
            context.SaveChanges();

            MessageBox.Show("Konto zostało utworzone. Możesz się teraz zalogować.",
                            "Rejestracja zakończona", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e) => Close();

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
