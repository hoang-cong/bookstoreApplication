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
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace QL_Nha_sach.ViewModels
{
    public class AddBookViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly SessionManager _session;
        private Book _newBook = new() { Stock = 0, Price = 0 };
        private readonly IConfiguration _config;

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
        public ICommand AutofillBookCommand { get; set; }

        public AddBookViewModel(SessionManager session, IDbContextFactory<AppDbContext> factory)
        {
            _session = session;
            _factory = factory;
            LoadData();

            // This builds the bridge between the file and your code
            _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            ManageAuthorCommand = new RelayCommand(_ => ExecuteManageLookup<Author>("Author"));
            ManageGenreCommand = new RelayCommand(_ => ExecuteManageLookup<Genre>("Genre"));
            ManagePublisherCommand = new RelayCommand(_ => ExecuteManageLookup<Publisher>("Publisher"));
            SaveBookCommand = new RelayCommand(ExecuteSaveBook);

            AutofillBookCommand = new RelayCommand(async _ => await AutofillBookFromIsbnAsync());
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

        // testing ISBN: 9780140328721 (Matilda by Roald Dahl)
        // testing ISBN: 9781476746586 (All the Light We Cannot See by Anthony Doerr)
        // testing ISBN: 9780486264721 (the call of the wild by Jack London)

        public async Task AutofillBookFromIsbnAsync()
        {
            if (string.IsNullOrWhiteSpace(NewBook.ISBN))
                return;

            try
            {
                string apiKey = _config["ApiKeys:GoogleBooks"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    ShowMessage("API Key missing! Check appsettings.json", true);
                    return;
                }

                using var client = new HttpClient();
                // var apiKey = "AIzaSyD56izeqicKnWtgmWVDu2i_NNQTvIguyaY";
                var url = $"https://www.googleapis.com/books/v1/volumes?q=isbn:{NewBook.ISBN}&key={apiKey}";

                var response = await client.GetStringAsync(url);

                using var doc = JsonDocument.Parse(response);
                using var context = _factory.CreateDbContext();

                if (!doc.RootElement.TryGetProperty("items", out var items) || items.GetArrayLength() == 0)
                {
                    ShowMessage("No book found.", true);
                    return;
                }

                var volumeInfo = items[0].GetProperty("volumeInfo");

                if (volumeInfo.TryGetProperty("title", out var titleProp))
                    NewBook.Title = titleProp.GetString();

                if (volumeInfo.TryGetProperty("imageLinks", out var imageLinks))
                {
                    if (imageLinks.TryGetProperty("thumbnail", out var thumbnail))
                    {
                        NewBook.CoverImageUrl = thumbnail.GetString();
                    }
                }

                // Publisher
                if (volumeInfo.TryGetProperty("publisher", out var publisherProp))
                {
                    var publisherName = publisherProp.GetString()?.Trim();

                    if (!string.IsNullOrWhiteSpace(publisherName))
                    {
                        var publisher = Publishers.FirstOrDefault(p => p.PublisherName.ToLower() == publisherName.ToLower());

                        if (publisher == null)
                        {
                            publisher = new Publisher { PublisherName = publisherName };

                            context.Publishers.Add(publisher);
                            context.SaveChanges();

                            Publishers.Add(publisher);
                        }
                        NewBook.PublisherId = publisher.PublisherId;
                    }
                }

                // Authors
                if (volumeInfo.TryGetProperty("authors", out var authorsProp))
                {
                    foreach (var rawName in authorsProp.EnumerateArray())
                    {
                        var authorName = rawName.GetString()?.Trim();

                        if (string.IsNullOrWhiteSpace(authorName))
                            continue;

                        var author = Authors.FirstOrDefault(a => a.AuthorName.ToLower() == authorName.ToLower());

                        if (author == null)
                        {
                            author = new Author
                            {
                                AuthorName = authorName
                            };

                            context.Authors.Add(author);
                            context.SaveChanges();

                            Authors.Add(author);
                        }
                        author.IsSelected = true;
                    }
                }

                // Categories
                if (volumeInfo.TryGetProperty("categories", out var categoriesProp))
                {
                    foreach (var rawCategory in categoriesProp.EnumerateArray())
                    {
                        var categoryName = rawCategory.GetString()?.Trim();

                        if (string.IsNullOrWhiteSpace(categoryName))
                            continue;

                        var genre = Genres.FirstOrDefault(g => g.GenreName.ToLower() == categoryName.ToLower());

                        if (genre == null)
                        {
                            genre = new Genre
                            {
                                GenreName = categoryName
                            };

                            context.Genres.Add(genre);
                            context.SaveChanges();

                            Genres.Add(genre);
                        }
                        genre.IsSelected = true;
                    }
                }

                OnPropertyChanged(nameof(NewBook));
                context.SaveChanges();

                ShowMessage("Book info autofilled from ISBN!", false);
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to fetch data, please try again", true);
            }
        }

        private void ExecuteSaveBook(object parameter)
        {
            if (string.IsNullOrEmpty(NewBook.Title))
            {
                ShowMessage("Please enter a title!", true);
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

                ShowMessage("Book Added Successfully!", false);

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