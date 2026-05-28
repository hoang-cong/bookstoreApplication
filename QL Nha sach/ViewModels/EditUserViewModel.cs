using Microsoft.EntityFrameworkCore;
using QL_Nha_sach.Data;
using QL_Nha_sach.Models;
using QL_Nha_sach.Services;
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
    public class EditUserViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;

        public int UserId { get; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public Role? SelectedRole { get; set; }

        public ObservableCollection<Role> Roles { get; } = new();

        public ICommand SaveCommand { get; }

        public EditUserViewModel(IDbContextFactory<AppDbContext> factory, int userId)
        {
            _factory = factory;
            UserId = userId;
            LoadRoles();
            LoadUser();
            SaveCommand = new RelayCommand(SaveUser);
        }

        private void LoadRoles()
        {
            using var context = _factory.CreateDbContext();
            Roles.Clear();
            foreach (var role in context.Roles.ToList())
                Roles.Add(role);
        }

        private void LoadUser()
        {
            using var context = _factory.CreateDbContext();
            var user = context.Users.Include(u => u.Role).FirstOrDefault(u => u.UserId == UserId);
            if (user != null)
            {
                FullName = user.FullName;
                Username = user.Username;
                PhoneNumber = user.PhoneNumber;
                EmailAddress = user.EmailAddress;
                SelectedRole = user.Role;
            }
        }

        private void SaveUser(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            string Password = passwordBox?.Password ?? string.Empty;

            if (string.IsNullOrWhiteSpace(FullName))
            {
                ShowMessage("Full Name cannot be empty.", true);
                return;
            }

            using var context = _factory.CreateDbContext();
            var user = context.Users.FirstOrDefault(u => u.UserId == UserId);

            if (user != null)
            {

                user.FullName = FullName;
                user.Username = Username;
                user.PhoneNumber = PhoneNumber;
                user.EmailAddress = EmailAddress;
                user.RoleId = SelectedRole?.RoleId ?? user.RoleId;

                bool exists = context.Users.Any(u => u.Username == user.Username);
                if (exists)
                {
                    ShowMessage("User with this Username already exists!", true);
                    return;
                }
                if (!string.IsNullOrWhiteSpace(Password))
                {
                    if (Password.Length < 6)
                    {
                        ShowMessage("Password must have at least 6 character", true);
                        return;
                    }
                    user.Password = Password;
                }

                context.SaveChanges();

                ShowMessage("Profile updated successfully!", false);

                if (passwordBox != null) passwordBox.Password = string.Empty;
            }
        }
    }
}
