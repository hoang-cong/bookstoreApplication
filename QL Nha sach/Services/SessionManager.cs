using Microsoft.Extensions.DependencyInjection;
using QL_Nha_sach.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace QL_Nha_sach.Services
{
    public class SessionManager : INotifyPropertyChanged
    {
        public User? _currentUser;
        public User? CurrentUser
        {
            get => _currentUser;
            private set
            {
                _currentUser = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentUser));
                OnPropertyChanged(nameof(FullName));
                OnPropertyChanged(nameof(RoleName));
            }
        }

        public string FullName => CurrentUser?.FullName ?? string.Empty;
        public string RoleName => CurrentUser?.Role?.RoleName ?? string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        
        public void SetUser(User user) => CurrentUser = user;
        public void Clear() => CurrentUser = null;

        public void Logout(NavigationService? nav)
        {
            Clear();
            if (nav != null)
            {
                var loginPage = App.AppHost.Services.GetRequiredService<QL_Nha_sach.Pages.LoginPage>();
                nav.Navigate(loginPage);
                nav.RemoveBackEntry();
            }
        }

    }
}
