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
using System.Windows;
using System.Windows.Input;

namespace QL_Nha_sach.ViewModels
{
    public class StockViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;

        private ObservableCollection<Book> _books = new();
        private string _searchText = string.Empty;

        private Book? _selectedBook;
        public ObservableCollection<Book> Books
        {
            get => _books;
            set { _books = value; OnPropertyChanged(); }
        }
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ExecuteSearch(); // Filter list as user types
            }
        }
        public Book SelectedBook
        {
            get => _selectedBook;
            set
            {
                _selectedBook = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; set; }

        public StockViewModel(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;

            LoadData();

            SaveCommand = new RelayCommand(ExecuteSave);
        }

        public void LoadData()
        {
            using var context = _factory.CreateDbContext();

            var bookList = context.Books
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Include(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
                .Include(b => b.Publisher)
                .Include(b => b.BookStatus)
                .ToList();

            Books = new ObservableCollection<Book>(bookList);
            context.ChangeTracker.Clear();
        }

        private void ExecuteSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadData();
                return;
            }

            var filtered = Books
                .Where(b => b.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                       || b.ISBN.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            Books = new ObservableCollection<Book>(filtered);
        }

        private void ExecuteSave(object parameter)
        {
            if (SelectedBook == null)
            {
                ShowMessage("Please select a book to update.", true);
                return;
            }

            using var context = _factory.CreateDbContext();
            var regulation = context.Regulations.FirstOrDefault();
            if (regulation == null)
            {
                ShowMessage("System regulations are not configured.", true);
                return;
            }
            var dbBook = context.Books.FirstOrDefault(b => b.BookId == SelectedBook.BookId);

            if (dbBook != null)
            {
                if (SelectedBook.Stock < 0)
                {
                    ShowMessage("Stock cannot be negative.", true);
                    return;
                }
                if (SelectedBook.Stock > regulation.MaxStock)
                {
                    ShowMessage($"Stock must be less than {regulation.MaxStock}.", true);
                    return;
                }
                if (SelectedBook.Stock < regulation.MinStock)
                {
                    ShowMessage($"Stock must be greater than {regulation.MinStock}.", true);
                    return;
                }

                dbBook.Stock = SelectedBook.Stock;
                context.SaveChanges();
                ShowMessage($"Stock for '{dbBook.Title}' updated to {dbBook.Stock}.", false);
            }

            LoadData();
        }
    }
}
