using Microsoft.EntityFrameworkCore;
using QL_Nha_sach.Data;
using QL_Nha_sach.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace QL_Nha_sach.ViewModels
{
    public class SignUpViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;

        // Form Fields
        public string Username { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }

        public ICommand SignUpCommand { get; }

        // Event to notify the View to navigate back to Login after success
        public event Action SignUpSucceeded;

        public SignUpViewModel(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
            SignUpCommand = new RelayCommand(ExecuteSignUpCommand);
        }

        private void ExecuteSignUpCommand(object parameter)
        {
            // 1. Extract the password safely from the CommandParameter
            var passwordBox = parameter as PasswordBox;
            string rawPassword = passwordBox?.Password ?? string.Empty;

            // 2. Basic Validation
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(FullName))
            {
                ShowMessage("Please enter all information.", true);
                return;
            }
            if (rawPassword.Length < 6)
            {
                ShowMessage("Password must have at least 6 character.", true);
                return;
            }

            using var context = _factory.CreateDbContext();

            // 3. Check if username already exists
            bool userExists = context.Users.Any(u => u.Username == Username);
            if (userExists)
            {
                ShowMessage("Username is already taken. Please choose another.", true);
                return;
            }

            try
            {
                // 4. Create and save the new user
                string passwordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(rawPassword, 12);
                var newUser = new User
                {
                    Username = Username,
                    Password = passwordHash, // Note: In production, hash this password!
                    FullName = FullName,
                    PhoneNumber = PhoneNumber,
                    EmailAddress = EmailAddress,
                    RoleId = 2 // Default role (e.g., User/Staff)
                };

                context.Users.Add(newUser);
                context.SaveChanges();

                ShowMessage("Registration successful!", false);

                // 5. Notify the view to navigate back to the login page
                SignUpSucceeded?.Invoke();
            }
            catch (Exception ex)
            {
                ShowMessage($"An error occurred during registration: {ex.Message}", true);
            }
        }
    }
}
