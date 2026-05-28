using Microsoft.EntityFrameworkCore;
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

        public UserManagementViewModel(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
            LoadUsers();
            LoadRoles();

            AddUserCommand = new RelayCommand(_ => AddUser());
            EditUserCommand = new RelayCommand(u => EditUser(u as User));
            DeleteUserCommand = new RelayCommand(DeleteUser, _ => SelectedUser != null);
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
            AddUserWindow window = new AddUserWindow(vm);
            if (Application.Current.MainWindow != null)
            {
                window.Owner = Application.Current.MainWindow;
            }
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();

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
            EditUserWindow window = new EditUserWindow(vm);
            if (Application.Current.MainWindow != null)
            {
                window.Owner = Application.Current.MainWindow;
            }
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();

            LoadUsers();
        }

        private void DeleteUser(object parameter)
        {
            if (SelectedUser == null) return;
            if (SelectedUser.UserId == 1)
            {
                ShowMessage("Cannot delete admin user", true);
                return;
            }

            try
            {
                using var context = _factory.CreateDbContext();
                context.Users.Remove(SelectedUser);
                context.SaveChanges();
                LoadUsers();
            }
            catch (DbUpdateException)
            {
                MessageBox.Show("Cannot delete this account because it is currently linked to invoice / import records in the database.",
                                "Deletion blocked", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
    }
}
