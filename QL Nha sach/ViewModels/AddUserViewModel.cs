using Microsoft.EntityFrameworkCore;
using QL_Nha_sach.Data;
using QL_Nha_sach.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace QL_Nha_sach.ViewModels
{
    public class AddUserViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;

        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public Role? SelectedRole { get; set; }

        public ObservableCollection<Role> Roles { get; } = new();

        public ICommand SaveCommand { get; }

        public AddUserViewModel(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
            LoadRoles();
            SaveCommand = new RelayCommand(SaveUser);
        }

        private void LoadRoles()
        {
            using var context = _factory.CreateDbContext();
            Roles.Clear();
            foreach (var role in context.Roles.ToList())
                Roles.Add(role);
        }

        private void SaveUser(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            string rawPassword = passwordBox?.Password ?? string.Empty;

            if (SelectedRole == null || string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Username))
            {
                ShowMessage("Please enter all information", true);
                return;
            }
            if (rawPassword.Length < 6)
            {
                ShowMessage("Password must have at least 6 characters", true);
                return;
            }

            using var context = _factory.CreateDbContext();
            
            bool exists = context.Users.Any(u => u.Username == Username);
            if (exists)
            {
                ShowMessage("User with this Username already exists!", true);
                return;
            }

            string passwordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(rawPassword, 12);
            var newUser = new User
            {
                FullName = FullName,
                Username = Username,
                Password = passwordHash, // later replace with hashing
                PhoneNumber = PhoneNumber,
                EmailAddress = EmailAddress,
                RoleId = SelectedRole.RoleId
            };

            context.Users.Add(newUser);
            context.SaveChanges();
            ShowMessage("User added successfully!", false);
        }
    }
}
