using Microsoft.EntityFrameworkCore;
using QL_Nha_sach.Data;
using QL_Nha_sach.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QL_Nha_sach.ViewModels
{
    public class LookupViewModel<T> : BaseViewModel where T : class, ILookup, new()
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        public ObservableCollection<T> Items { get; set; }

        private T? _selectedItem;
        public T? SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }

        private string _newName;
        public string NewName
        {
            get => _newName;
            set { _newName = value; OnPropertyChanged(); }
        }

        public string Title { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        public LookupViewModel(IDbContextFactory<AppDbContext> factory, string title)
        {
            _factory = factory;
            Title = title;
            LoadData();
            AddCommand = new RelayCommand(_ => ExecuteAdd());
            DeleteCommand = new RelayCommand(ExecuteDelete, _ => SelectedItem != null);
        }

        private void LoadData()
        {
            using var context = _factory.CreateDbContext();
            // Set<T> dynamically finds the right table (Authors, Genres, etc.)
            Items = new ObservableCollection<T>(context.Set<T>().ToList());
        }

        private void ExecuteAdd()
        {
            if (string.IsNullOrWhiteSpace(NewName)) return;

            using var context = _factory.CreateDbContext();
            var newItem = new T { Name = NewName };

            context.Set<T>().Add(newItem);
            context.SaveChanges();

            Items.Add(newItem);
            NewName = string.Empty;
        }

        private void ExecuteDelete(object parameter)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete '{SelectedItem.Name}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                using var context = _factory.CreateDbContext();

                context.Set<T>().Remove(SelectedItem);
                context.SaveChanges();

                Items.Remove(SelectedItem);

                MessageBox.Show("Deleted successfully!");
            }
            catch (DbUpdateException)
            {
                MessageBox.Show("Cannot delete this item because it is currently linked to books in the database. Please remove the books first.",
                                "Dependency Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
    }
}
