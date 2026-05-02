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
using System.Windows.Input;

namespace QL_Nha_sach.ViewModels
{
    public class AddBookViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly SessionManager _session;
        private Book _newBook = new() { Stock = 0, Price = 0 };

        public IDbContextFactory<AppDbContext> Factory => _factory;

        public Book NewBook
        {
            get => _newBook;
            set { _newBook = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Author> Authors { get; set; }
        public ObservableCollection<Genre> Genres { get; set; }
        public ObservableCollection<Publisher> Publishers { get; set; }
        public List<BookStatus> BookStatuses { get; set; }

        public ICommand ManageAuthorCommand { get; set; }
        public ICommand ManageGenreCommand { get; set; }
        public ICommand ManagePublisherCommand { get; set; }
        public ICommand SaveBookCommand { get; set; }

        public AddBookViewModel(SessionManager session, IDbContextFactory<AppDbContext> factory)
        {
            _session = session;
            _factory = factory;
            LoadData();
            ManageAuthorCommand = new RelayCommand(_ => ExecuteManageLookup<Author>("Author"));
            ManageGenreCommand = new RelayCommand(_ => ExecuteManageLookup<Genre>("Genre"));
            ManagePublisherCommand = new RelayCommand(_ => ExecuteManageLookup<Publisher>("Publisher"));
            SaveBookCommand = new RelayCommand(ExecuteSaveBook);
        }

        public void LoadData()
        {
            using var context = _factory.CreateDbContext();

            Authors = new ObservableCollection<Author>(context.Authors.ToList());
            Genres = new ObservableCollection<Genre>(context.Genres.ToList());
            Publishers = new ObservableCollection<Publisher>(context.Publishers.ToList());
            BookStatuses = new List<BookStatus>([.. context.BookStatuses]);
        }

        private void ExecuteManageLookup<T>(string title) where T : class, ILookup, new()
        {
            // Get the window from DI
            var window = App.AppHost.Services.GetRequiredService<LookupWindow>();

            var vm = new LookupViewModel<T>(_factory, title);

            window.DataContext = vm;
            window.ShowDialog();

            LoadData();
        }
        private void ExecuteSaveBook(object parameter)
        {
            if (string.IsNullOrEmpty(NewBook.Title))
            {
                MessageBox.Show("Please enter a title!");
                return;
            }

            try
            {
                using var context = _factory.CreateDbContext();

                // Add the book first
                context.Books.Add(NewBook);
                context.SaveChanges();

                // Add authors (junction table)
                foreach (var author in Authors.Where(a => a.IsSelected))
                {
                    context.Add(new BookAuthor
                    {
                        BookId = NewBook.BookId,
                        AuthorId = author.AuthorId
                    });
                }

                // Add genres (junction table)
                foreach (var genre in Genres.Where(g => g.IsSelected))
                {
                    context.Add(new BookGenre
                    {
                        BookId = NewBook.BookId,
                        GenreId = genre.GenreId
                    });
                }

                context.SaveChanges();

                MessageBox.Show("Book Added Successfully!");

                // Reset for next entry
                NewBook = new() { Stock = 0, Price = 0 };
                foreach (var author in Authors) author.IsSelected = false;
                foreach (var genre in Genres) genre.IsSelected = false;

                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
    }
}
/*Your code is much closer, and you've fixed the main issue by initializing the `BookStatusId`. However, there is one **"sneaky" bug** waiting for you in your reset logic that will cause the second book you try to add to fail.

Here is the breakdown of what is good and what needs one final polish:

### 1. The Reset Logic Bug
In your `ExecuteSaveBook`, you reset the form with this line:
`NewBook = new() { Stock = 0, Price = 0 };`

* **The Problem:** You forgot to include `BookStatusId = 1` here!
* **The Result:** The first book will save perfectly. But after that, `NewBook` becomes a fresh object where `BookStatusId` is back to **0**. When you try to save the second book, you'll get that **SQLite Error 19** again.

### 2. ObservableCollection Properties
In `LoadData`, you are doing: 
`Authors = new ObservableCollection<Author>(...)`
* **The Problem:** Since `Authors` is a property, the UI needs to know when the *entire collection* is replaced. 
* **The Fix:** Make sure your collections call `OnPropertyChanged()` or use the backing fields, otherwise, when you return from the "Manage" window, the ComboBox might look empty even though you reloaded the data.

---

### The "Final Polish" Version
Here is the corrected `ExecuteSaveBook` and a more robust way to handle the collections:

```csharp
// Use backing fields to ensure the UI updates when you reload data
private ObservableCollection<Author> _authors;
public ObservableCollection<Author> Authors 
{ 
    get => _authors; 
    set { _authors = value; OnPropertyChanged(); } 
}

// ... do the same for Genres and Publishers ...

private void ExecuteSaveBook(object parameter)
{
    if (string.IsNullOrWhiteSpace(NewBook.Title))
    {
        MessageBox.Show("Please enter a title!");
        return;
    }

    // Explicitly check for 0s before saving to avoid crashes
    if (NewBook.AuthorId == 0 || NewBook.GenreId == 0 || NewBook.PublisherId == 0)
    {
        MessageBox.Show("Please select an Author, Genre, and Publisher!");
        return;
    }

    try
    {
        using var context = _factory.CreateDbContext();

        context.Books.Add(NewBook);
        context.SaveChanges();

        MessageBox.Show("Book Added Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

        // FIX: Re-include the StatusId here so the NEXT book doesn't crash!
        NewBook = new() { Stock = 0, Price = 0, BookStatusId = 1 };
    }
    catch (Exception ex)
    {
        // Don't comment out the try-catch! 
        // It's your best friend for finding errors without the app crashing.
        MessageBox.Show($"Error: {ex.InnerException?.Message ?? ex.Message}");
    }
}
```

### Why did you comment out the `try-catch`?
I noticed you commented it out in your snippet. Definitely keep it! In database programming, things can go wrong that aren't your fault (like the database file being locked or a unique constraint on an ISBN being triggered). You want the app to tell the user what happened rather than just disappearing.

### One last tip for the road:
Make sure you actually have a row in your `BookStatuses` table with an `Id` of `1`. If that table is empty, even setting it to `1` in code won't save you from Error 19!

**Everything else looks solid—the generic lookup logic and the UI binding are very well implemented.** Are you planning to add an "Image" or "Cover" property for the books next?
**/