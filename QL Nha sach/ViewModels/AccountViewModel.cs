using Microsoft.EntityFrameworkCore;
using QL_Nha_sach.Data;
using QL_Nha_sach.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace QL_Nha_sach.ViewModels
{
    public class AccountViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly SessionManager _session;

        // Properties bound to the View fields
        public string Username { get; set; } // Read-only in the UI
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string RoleName { get; set; } // Read-only in the UI

        public ICommand SaveChangesCommand { get; }

        public AccountViewModel(SessionManager session, IDbContextFactory<AppDbContext> factory)
        {
            _session = session;
            _factory = factory;

            SaveChangesCommand = new RelayCommand(ExecuteSaveChangesCommand);

            LoadCurrentUserData();
        }

        private void LoadCurrentUserData()
        {
            var currentUser = _session.CurrentUser;

            if (currentUser != null)
            {
                Username = currentUser.Username;
                FullName = currentUser.FullName;
                PhoneNumber = currentUser.PhoneNumber;
                EmailAddress = currentUser.EmailAddress;
                RoleName = currentUser.Role?.RoleName ?? "Standard User";
            }
        }

        private void ExecuteSaveChangesCommand(object parameter)
        {
            // 1. Check if they typed a new password
            var passwordBox = parameter as PasswordBox;
            string newPassword = passwordBox?.Password ?? string.Empty;

            if (string.IsNullOrWhiteSpace(FullName))
            {
                ShowMessage("Full Name cannot be empty.", true);
                return;
            }

            using var context = _factory.CreateDbContext();
            var dbUser = context.Users.FirstOrDefault(u => u.Username == Username);

            if (dbUser != null)
            {
                try
                {
                    dbUser.FullName = FullName;
                    dbUser.PhoneNumber = PhoneNumber;
                    dbUser.EmailAddress = EmailAddress;

                    if (!string.IsNullOrWhiteSpace(newPassword))
                    {
                        if (newPassword.Length < 6)
                        {
                            ShowMessage("Password must have at least 6 character", true);
                            return;
                        }
                        dbUser.Password = newPassword; // Ideally hashed!
                    }

                    context.SaveChanges();

                    _session.CurrentUser.FullName = FullName;
                    _session.CurrentUser.PhoneNumber = PhoneNumber;
                    _session.CurrentUser.EmailAddress = EmailAddress;

                    ShowMessage("Profile updated successfully!", false);

                    if (passwordBox != null) passwordBox.Password = string.Empty;
                }
                catch (Exception ex)
                {
                    ShowMessage($"Failed to save profile: {ex.Message}", true);
                }
            }
        }
    }
}
