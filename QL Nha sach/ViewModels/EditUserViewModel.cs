using Microsoft.EntityFrameworkCore;
using QL_Nha_sach.Data;
using QL_Nha_sach.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QL_Nha_sach.ViewModels
{
    public class EditUserViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;

        public int UserId { get; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
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
            SaveCommand = new RelayCommand(_ => SaveUser());
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
                Password = user.Password;
                PhoneNumber = user.PhoneNumber;
                EmailAddress = user.EmailAddress;
                SelectedRole = user.Role;
            }
        }

        private void SaveUser()
        {
            using var context = _factory.CreateDbContext();
            var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
            if (user != null)
            {
                user.FullName = FullName;
                user.Username = Username;
                user.Password = Password; // later replace with hashing
                user.PhoneNumber = PhoneNumber;
                user.EmailAddress = EmailAddress;
                user.RoleId = SelectedRole?.RoleId ?? user.RoleId;
                context.SaveChanges();
                ShowMessage("User updated successfully!", false);
            }
        }
    }
}
