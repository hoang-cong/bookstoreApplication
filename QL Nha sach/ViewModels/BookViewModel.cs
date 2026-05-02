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
using System.Windows.Navigation;

namespace QL_Nha_sach.ViewModels
{
    public class BookViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly SessionManager _session;

        // 1. Private backing fields
        private Book _newBook = new();
        private Book? _selectedBook;
        private ObservableCollection<Book> _books = new();
        private string _searchText = string.Empty;

        // 2. Public properties
        public IDbContextFactory<AppDbContext> Factory => _factory;
        public Book NewBook
        {
            get => _newBook;
            set { _newBook = value; OnPropertyChanged(); }
        }
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

        public event Action<Page> NavigateRequested;

        public ICommand AddBookCommand { get; set; }
        public ICommand EditBookCommand { get; set; }
        public ICommand DeleteBookCommand { get; }

        // 3. Simple constructor
        public BookViewModel(SessionManager session, IDbContextFactory<AppDbContext> factory)
        {
            _session = session;
            _factory = factory;
            LoadData();
            AddBookCommand = new RelayCommand(_ => ExecuteAddBook());
            EditBookCommand = new RelayCommand(ExecuteEditBook);
            DeleteBookCommand = new RelayCommand(ExecuteDeleteBook, (obj) => SelectedBook != null);
        }

        public void LoadData()
        {
            using var context = _factory.CreateDbContext();
            
            // Fetch everything from the DB
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
            using var context = _factory.CreateDbContext();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadData();
                return;
            }

            var filtered = context.Books
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Include(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
                .Include(b => b.Publisher)
                .Where(b => b.Title.Contains(SearchText) || b.ISBN.Contains(SearchText))
                .ToList();

            Books = new ObservableCollection<Book>(filtered);
        }

        private void ExecuteAddBook()
        {
            var vm = new AddBookViewModel(_session, _factory);
            NavigateRequested?.Invoke(new AddBookPage(vm));
        }

        private void ExecuteEditBook(object parameter)
        {
            if (SelectedBook == null)
            {
                MessageBox.Show("Please select a book to edit.");
                return;
            }

            var window = new EditBookWindow(SelectedBook);
            window.ShowDialog();

            LoadData();
        }
        private void ExecuteDeleteBook(object parameter)
        {
            using var context = _factory.CreateDbContext();
            
            var result = MessageBox.Show($"Delete {SelectedBook.Title}?", "Confirm", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                context.Books.Remove(SelectedBook);
                context.SaveChanges();
                LoadData();
            }
        }
    }
}