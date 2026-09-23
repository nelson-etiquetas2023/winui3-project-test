using System;
using System.Collections.Generic;
using System.Linq;

namespace WINUI_TEST.Services
{
    public sealed class AppUser
    {
        public AppUser(string name, string email, string password)
        {
            Name = name;
            Email = email;
            Password = password;
        }

        public string Name { get; }
        public string Email { get; }
        public string Password { get; }

        public string Initials
        {
            get
            {
                var parts = Name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                {
                    return "?";
                }

                char first = char.ToUpperInvariant(parts[0][0]);
                char last = parts.Length > 1 ? char.ToUpperInvariant(parts[^1][0]) : first;
                return new string(new[] { first, last });
            }
        }
    }

    /// <summary>
    /// Sesión simulada en memoria. Conectar a una API real cuando exista.
    /// </summary>
    public static class UserSession
    {
        private static readonly List<AppUser> Users = new()
        {
            new AppUser("admin", "admin@demo.com", "admin")
        };

        public static AppUser? Current { get; private set; }

        public static event EventHandler? SessionChanged;

        public static bool Login(string emailOrUser, string password)
        {
            var user = Users.FirstOrDefault(u =>
                string.Equals(u.Email, emailOrUser, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(u.Name, emailOrUser, StringComparison.OrdinalIgnoreCase));

            if (user == null || user.Password != password)
            {
                return false;
            }

            Current = user;
            SessionChanged?.Invoke(null, EventArgs.Empty);
            return true;
        }

        public static bool Register(string name, string email, string password)
        {
            name = (name ?? "").Trim();
            email = (email ?? "").Trim();

            if (name.Length == 0 || email.Length == 0 || (password ?? "").Length == 0)
            {
                return false;
            }

            if (Users.Any(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            Users.Add(new AppUser(name, email, password));
            Current = Users[^1];
            SessionChanged?.Invoke(null, EventArgs.Empty);
            return true;
        }

        public static void Logout()
        {
            Current = null;
            SessionChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}