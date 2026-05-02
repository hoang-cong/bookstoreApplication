using Microsoft.EntityFrameworkCore;
using QL_Nha_sach.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace QL_Nha_sach.Data
{
    public class AppDbContext : DbContext
    {
        // The Factory needs this constructor to "inject" the SQLite connection string
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Define tables here
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookAuthor> BookAuthors { get; set; }
        public DbSet<BookGenre> BookGenres { get; set; }
        public DbSet<BookStatus> BookStatuses { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Import> Imports { get; set; }
        public DbSet<ImportDetail> ImportDetails { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceDetail> InvoiceDetails { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<PromotionTarget> PromotionTargets { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Regulation> Regulations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Creates a SQLite database file named bookstore.db
            optionsBuilder.UseSqlite("Data Source=bookstore.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Composite keys for junction tables
            modelBuilder.Entity<BookAuthor>()
                .HasKey(ba => new { ba.BookId, ba.AuthorId });

            modelBuilder.Entity<BookGenre>()
                .HasKey(bg => new { bg.BookId, bg.GenreId });

            // BookAuthor relationships
            modelBuilder.Entity<BookAuthor>()
                .HasOne(ba => ba.Book)
                .WithMany(b => b.BookAuthors)
                .HasForeignKey(ba => ba.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookAuthor>()
                .HasOne(ba => ba.Author)
                .WithMany(a => a.BookAuthors)
                .HasForeignKey(ba => ba.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            // BookGenre relationships
            modelBuilder.Entity<BookGenre>()
                .HasOne(bg => bg.Book)
                .WithMany(b => b.BookGenres)
                .HasForeignKey(bg => bg.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookGenre>()
                .HasOne(bg => bg.Genre)
                .WithMany(g => g.BookGenres)
                .HasForeignKey(bg => bg.GenreId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ImportDetail>()
                .HasKey(d => new { d.ImportId, d.BookId });

            modelBuilder.Entity<InvoiceDetail>()
                .HasKey(d => new { d.InvoiceId, d.BookId });

            modelBuilder.Entity<PromotionTarget>()
                .HasKey(pt => new { pt.PromotionId, pt.BookId });

            // one to many

            modelBuilder.Entity<Book>()
                .HasOne(b => b.BookStatus)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.BookStatusId);

            modelBuilder.Entity<Book>()
                .HasOne(b => b.Publisher)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.PublisherId);

            modelBuilder.Entity<Import>()
                .HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId);

            modelBuilder.Entity<ImportDetail>()
                .HasOne(d => d.Book)
                .WithMany()
                .HasForeignKey(d => d.BookId);

            modelBuilder.Entity<ImportDetail>()
                .HasOne(d => d.Import)
                .WithMany(i => i.ImportDetails)
                .HasForeignKey(d => d.ImportId)
                .OnDelete(DeleteBehavior.Cascade);

            // Example: Connecting InvoiceDetails to Invoices

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId);

            modelBuilder.Entity<InvoiceDetail>()
                .HasOne(d => d.Book)
                .WithMany() // Or .WithMany(b => b.InvoiceDetails) if added that collection to Book.cs
                .HasForeignKey(d => d.BookId);

            modelBuilder.Entity<InvoiceDetail>()
                .HasOne(d => d.Invoice)
                .WithMany(i => i.InvoiceDetails)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasOne(d => d.Role)
                .WithMany(i => i.Users)
                .HasForeignKey(d => d.RoleId);

            // Add seed data here
            modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, RoleName = "Manager" },
            new Role {RoleId = 2, RoleName = "Staff" },
            new Role {RoleId = 3, RoleName = "Stocker" }
            );

            modelBuilder.Entity<BookStatus>().HasData(
                new BookStatus { BookStatusId = 1, StatusName = "Available"},
                new BookStatus { BookStatusId = 2, StatusName = "Out of stock"},
                new BookStatus { BookStatusId = 3, StatusName = "Not for sale"}
            );
        }
    }
}
//Go to Tools -> NuGet Package Manager -> Package Manager Console.

//PowerShell
//# This creates a "blueprint" of your database based on your classes
//Add-Migration InitialCreate

//# This actually builds the database file (bookstore.db)
//Update-Database

//What to look for
//Once you run Update-Database, look at your Solution Explorer:

//A new folder named Migrations will appear. This contains the C# "translation" of your tables into SQL.

//A file named bookstore.db (or whatever you named it in OnConfiguring) will appear in your project folder.

//One last "Peer" Check on your Models
//Before you run that command, double-check that your InvoiceDetail and StockImportDetail don't have that duplicate [Key] we talked about. If they do, the migration will fail.

//Wait! One tiny detail: Since you are in a WPF app, remember that the database file is created in the Output folder (where the .exe lives) when you run the app. If you don't see the .db file in your main folder, check bin/Debug/net8.0-windows/.
