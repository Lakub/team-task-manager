using System.Text;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Models;
using System.Security.Cryptography;

namespace TeamTaskManager.Services
{
    public interface IAuthService
    {
        User? Validate(string email, string password);
        bool UserExists(string email);
        string GenerateSalt();
        string HashPassword(string password, string salt);
        bool ChangePassword(string email, string currentPassword, string newPassword);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        internal User? GetUser(string email)
        {
            return _context.Users.AsEnumerable().FirstOrDefault(u => u.Email == email);
        }

        public User? Validate(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            var user = GetUser(email);
            if (user == null)
            {
                return null;
            }

            var userAuth = _context.UserAuths.FirstOrDefault(ua => ua.UserId == user.Id);
            if (userAuth == null)
            {
                return null;
            }

            if (VerifyPassword(password, userAuth.PasswordSalt, userAuth.Password))
            {
                return user;
            }
            return null;
        }

        public bool UserExists(string email)
        {
            return _context.Users.Any(u => u.Email == email);
        }

        public string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        public string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + salt));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
        private bool VerifyPassword(string inputPassword, string salt, string storedHash)
        {
            var inputHash = HashPassword(inputPassword, salt);
            return inputHash == storedHash;
        }

        public bool ChangePassword(string email, string currentPassword, string newPassword)
        {
            var user = Validate(email, currentPassword);
            if (user == null)
                return false;

            var userAuth = _context.UserAuths.FirstOrDefault(x => x.UserId == user.Id);
            if (userAuth == null)
                return false;

            var newSalt = GenerateSalt();
            userAuth.PasswordSalt = newSalt;
            userAuth.Password = HashPassword(newPassword, newSalt);

            _context.SaveChanges();
            return true;
        }
    }
}
