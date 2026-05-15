using Microsoft.EntityFrameworkCore;
using QL_Nha_sach.Data;
using QL_Nha_sach.Models;
using QL_Nha_sach.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls;

namespace QL_Nha_sach.ViewModels
{
    public class UserManagementViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        
        private User? _selectedUser;
        private string _searchText;

        public ObservableCollection<User> Users { get; } = new();
        public ObservableCollection<Role> Roles { get; } = new();

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilter(); // Refresh list when user types
            }
        }

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }

        public event Action<Page> NavigateRequested;

        public UserManagementViewModel(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
            LoadUsers();
            LoadRoles();

            AddUserCommand = new RelayCommand(_ => AddUser());
            EditUserCommand = new RelayCommand(u => EditUser(u as User));
            DeleteUserCommand = new RelayCommand(u => DeleteUser(u as User));
        }

        private void LoadUsers()
        {
            using var context = _factory.CreateDbContext();
            Users.Clear();
            foreach (var user in context.Users.Include(u => u.Role).ToList())
                Users.Add(user);
        }

        private void LoadRoles()
        {
            using var context = _factory.CreateDbContext();
            Roles.Clear();
            foreach (var role in context.Roles.ToList())
                Roles.Add(role);
        }

        private void ApplyFilter()
        {
            using var context = _factory.CreateDbContext();
            var query = context.Users.Include(u => u.Role).AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(u =>
                    u.Username.Contains(SearchText) ||
                    u.EmailAddress.Contains(SearchText) ||
                    u.PhoneNumber.Contains(SearchText) ||
                    u.UserId.ToString() == SearchText);
            }

            Users.Clear();
            foreach (var user in query.ToList())
                Users.Add(user);
        }

        private void AddUser()
        {
            var vm = new AddUserViewModel(_factory);
            NavigateRequested?.Invoke(new AddUserPage(vm));

            LoadUsers();
        }

        private void EditUser(object parameter)
        {
            if (SelectedUser == null)
            {
                ShowMessage("Please select a user to edit.", true);
                return;
            }
            var vm = new EditUserViewModel(_factory, SelectedUser.UserId);
            NavigateRequested?.Invoke(new EditUserPage(vm));

            LoadUsers();
        }

        private void DeleteUser(User? user)
        {
            if (user == null) return;
            using var context = _factory.CreateDbContext();
            var dbUser = context.Users.FirstOrDefault(u => u.UserId == user.UserId);
            if (dbUser != null)
            {
                context.Users.Remove(dbUser);
                context.SaveChanges();
            }
            LoadUsers();
        }
    }
}
