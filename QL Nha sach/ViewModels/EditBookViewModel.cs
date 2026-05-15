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
    public class EditBookViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        public List<Publisher> Publishers { get; set; }
        public List<BookStatus> BookStatuses { get; set; }
        public Book? EditableBook { get; set; }

        public ObservableCollection<Author> Authors { get; set; }
        public ObservableCollection<Genre> Genres { get; set; }

        public ObservableCollection<Author> SelectedAuthors { get; set; }
        public ObservableCollection<Genre> SelectedGenres { get; set; }

        public ICommand SaveCommand { get; }

        public EditBookViewModel(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
            SaveCommand = new RelayCommand(ExecuteSave);
        }

        public void LoadData()
        {
            using var context = _factory.CreateDbContext();

            Authors = new ObservableCollection<Author>(context.Authors.ToList());
            Genres = new ObservableCollection<Genre>(context.Genres.ToList());
            Publishers = new List<Publisher>([.. context.Publishers]);
            BookStatuses = new List<BookStatus>([.. context.BookStatuses]);

            EditableBook = context.Books
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookGenres).ThenInclude(bg => bg.Genre)
            .FirstOrDefault(b => b.BookId == EditableBook.BookId);

            SelectedAuthors = new ObservableCollection<Author>(EditableBook.BookAuthors.Select(ba => ba.Author));
            SelectedGenres = new ObservableCollection<Genre>(EditableBook.BookGenres.Select(bg => bg.Genre));
        }

        private void ExecuteSave(object parameter)
        {
            using var context = _factory.CreateDbContext();
            var dbBook = context.Books
                .Include(b => b.BookAuthors)
                .Include(b => b.BookGenres)
                .FirstOrDefault(b => b.BookId == EditableBook.BookId);

            if (dbBook != null)
            {
                // Update scalar fields
                dbBook.Title = EditableBook.Title;
                dbBook.ISBN = EditableBook.ISBN;
                dbBook.Price = EditableBook.Price;
                dbBook.Stock = EditableBook.Stock;
                dbBook.PublisherId = EditableBook.PublisherId;
                dbBook.BookStatusId = EditableBook.BookStatusId;

                // Update Authors (junction table)
                dbBook.BookAuthors.Clear();
                foreach (var author in SelectedAuthors)
                {
                    dbBook.BookAuthors.Add(new BookAuthor
                    {
                        BookId = dbBook.BookId,
                        AuthorId = author.AuthorId
                    });
                }

                // Update Genres (junction table)
                dbBook.BookGenres.Clear();
                foreach (var genre in SelectedGenres)
                {
                    dbBook.BookGenres.Add(new BookGenre
                    {
                        BookId = dbBook.BookId,
                        GenreId = genre.GenreId
                    });
                }

                context.SaveChanges();
                ShowMessage("Book updated successfully.", false);
            }
        }

    }
}
