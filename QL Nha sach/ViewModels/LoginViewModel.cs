using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QL_Nha_sach.Data;
using QL_Nha_sach.Models;
using QL_Nha_sach.Pages;
using QL_Nha_sach.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace QL_Nha_sach.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly SessionManager _session;

        public string Username { get; set; }

        public ICommand LoginCommand { get; }
        public ICommand SignUpCommand { get; }

        public event Action<User> LoginSucceeded;
        public event Action<Page> NavigateRequested;

        public LoginViewModel(IDbContextFactory<AppDbContext> factory, SessionManager session)
        {
            _factory = factory;
            _session = session;
            using var context = _factory.CreateDbContext();

            if (!context.Users.Any())
            {
                string defaultHash = BCrypt.Net.BCrypt.EnhancedHashPassword("123", 12);

                context.Users.AddRange(
                    new User { Username = "admin", Password = defaultHash, FullName = "Admin User", PhoneNumber = "0912995866", EmailAddress = "admin@gmail.com", RoleId = 1 }
                );
                context.SaveChanges();
            }

            LoginCommand = new RelayCommand(ExecuteLoginCommand);
            SignUpCommand = new RelayCommand(_ => OpenSignUp());
        }

        private void ExecuteLoginCommand(object parameter)
        {
            _session.Clear(); // wipe old session

            var passwordBox = parameter as PasswordBox;
            string enteredPassword = passwordBox?.Password ?? string.Empty;

            using var context = _factory.CreateDbContext();
            
            var user = context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Username == Username);

            if (user != null && BCrypt.Net.BCrypt.EnhancedVerify(enteredPassword, user.Password))
            {
                // Raise event so the View (LoginPage.xaml.cs) can handle navigation
                LoginSucceeded?.Invoke(user);
            }
            else
            {
                MessageBox.Show("Invalid username or password");
            }
        }

        private void OpenSignUp()
        {
            var vm = new SignUpViewModel(_factory);
            NavigateRequested?.Invoke(new SignUpPage(vm));
        }
    }
}