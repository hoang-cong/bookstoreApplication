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

        private List<Book> _allBooks = new(); // Master copy
        private ObservableCollection<Genre> _genres = new();
        private ObservableCollection<Author> _authors = new();
        private Genre? _selectedGenre;
        private Author? _selectedAuthor;

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
        public string ExternalSearchText { get; set; } = string.Empty;
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
        public ObservableCollection<Genre> Genres { get => _genres; set { _genres = value; OnPropertyChanged(); } }
        public ObservableCollection<Author> Authors { get => _authors; set { _authors = value; OnPropertyChanged(); } }

        public Genre? SelectedGenre
        {
            get => _selectedGenre;
            set { _selectedGenre = value; OnPropertyChanged(); ApplyFilters(); }
        }

        public Author? SelectedAuthor
        {
            get => _selectedAuthor;
            set { _selectedAuthor = value; OnPropertyChanged(); ApplyFilters(); }
        }
        public bool IsManager => _session.CurrentUser?.Role?.RoleName == "Manager";

        public event Action<Page> NavigateRequested;

        public ICommand ClearFiltersCommand { get; }
        public ICommand AddBookCommand { get; set; }
        public ICommand EditBookCommand { get; set; }
        public ICommand DeleteBookCommand { get; }

        // 3. Simple constructor
        public BookViewModel(SessionManager session, IDbContextFactory<AppDbContext> factory)
        {
            _session = session;
            _factory = factory;
            LoadData();

            ClearFiltersCommand = new RelayCommand(_ => ExecuteClearFilters());
            AddBookCommand = new RelayCommand(_ => ExecuteAddBook(), _ => IsManager);
            EditBookCommand = new RelayCommand(ExecuteEditBook, _ => IsManager);
            DeleteBookCommand = new RelayCommand(ExecuteDeleteBook, _ => IsManager && SelectedBook != null);
        }

        public void LoadData()
        {
            using var context = _factory.CreateDbContext();

            // Load Genres and Authors for the filter dropdowns
            Genres = new ObservableCollection<Genre>(context.Genres.OrderBy(g => g.GenreName).ToList());
            Authors = new ObservableCollection<Author>(context.Authors.OrderBy(a => a.AuthorName).ToList());

            // Load the Master List of books
            _allBooks = context.Books
                .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
                .Include(b => b.BookGenres).ThenInclude(bg => bg.Genre)
                .Include(b => b.Publisher)
                .Include(b => b.BookStatus)
                .OrderByDescending(b => b.BookId)
                .ToList();

            if (!string.IsNullOrWhiteSpace(ExternalSearchText))
            {
                SearchText = ExternalSearchText;
            }
            ApplyFilters(); // Initial display
        }

        private void ApplyFilters()
        {
            var filtered = _allBooks.AsEnumerable();

            // 1. Filter by Search Text (Title or ISBN)
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(b =>
                    b.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    b.ISBN.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // 2. Filter by Selected Genre
            if (SelectedGenre != null)
            {
                filtered = filtered.Where(b => b.BookGenres.Any(bg => bg.GenreId == SelectedGenre.GenreId));
            }

            // 3. Filter by Selected Author
            if (SelectedAuthor != null)
            {
                filtered = filtered.Where(b => b.BookAuthors.Any(ba => ba.AuthorId == SelectedAuthor.AuthorId));
            }

            // Update the UI collection
            Books = new ObservableCollection<Book>(filtered.ToList());
        }

        private void ExecuteClearFilters()
        {
            _searchText = string.Empty;
            _selectedGenre = null;
            _selectedAuthor = null;

            // Notify the UI that these properties changed
            OnPropertyChanged(nameof(SearchText));
            OnPropertyChanged(nameof(SelectedGenre));
            OnPropertyChanged(nameof(SelectedAuthor));

            // Refresh the list
            ApplyFilters();
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

        private void ExecuteAddBook()
        {
            var vm = new AddBookViewModel(_session, _factory);
            NavigateRequested?.Invoke(new AddBookPage(vm));

            LoadData();
        }

        private void ExecuteEditBook(object parameter)
        {
            if (SelectedBook == null)
            {
                ShowMessage("Please select a book to edit.", true);
                return;
            }

            var window = new EditBookWindow(SelectedBook);
            if(Application.Current.MainWindow != null)
            {
                window.Owner = Application.Current.MainWindow;
            }
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();

            LoadData();
        }
        private void ExecuteDeleteBook(object parameter)
        {
            if (SelectedBook == null) return;
            
            var result = MessageBox.Show($"Delete {SelectedBook.Title}?", "Confirm", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                using var context = _factory.CreateDbContext();

                context.Books.Remove(SelectedBook);
                context.SaveChanges();

                LoadData();
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show($"Cannot delete '{SelectedBook.Title}' because it is linked to existing invoice, import records or active promotions.\n\nConsider marking it as inactive instead.", "Deletion Blocked",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred while deleting the book: {ex.Message}", "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }
    }
}