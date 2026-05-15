using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using QL_Nha_sach.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace QL_Nha_sach.Models
{
    public class BookStatus
    {
        [Key]
        public int BookStatusId { get; set; }
        public string StatusName { get; set; } = string.Empty; // "Available", "OutOfStock", "NotForSale"
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();
    }
    public class Book
    {
        [Key]
        public int BookId { get; set; }
        public string ISBN { get; set; } = string.Empty; // Use string for ISBN (prevents leading zero issues)
        public string Title { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; } = string.Empty;

        // Use IDs for the database relationships
        public int PublisherId { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; } 
        public int BookStatusId { get; set; }

        // This is the Navigation Property - REQUIRED for HasOne(b => b.Author)
        public virtual Publisher? Publisher { get; set; }
        public virtual BookStatus? BookStatus { get; set; }

        // Many to many
        public virtual ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
        public virtual ICollection<BookGenre> BookGenres { get; set; } = new List<BookGenre>();

        [NotMapped]
        public string AuthorNames => string.Join(", ", BookAuthors.Select(ba => ba.Author.AuthorName));

        [NotMapped]
        public string GenreNames => string.Join(", ", BookGenres.Select(bg => bg.Genre.GenreName));

    }
}