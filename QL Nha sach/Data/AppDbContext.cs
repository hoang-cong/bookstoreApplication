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
            optionsBuilder.UseSqlite("Data Source=bookstore.db;Foreign Keys=True;");
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
                .OnDelete(DeleteBehavior.Restrict);

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
                .OnDelete(DeleteBehavior.Restrict);

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
                .HasForeignKey(b => b.PublisherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Import>()
                .HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ImportDetail>()
                .HasOne(d => d.Book)
                .WithMany()
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ImportDetail>()
                .HasOne(d => d.Import)
                .WithMany(i => i.ImportDetails)
                .HasForeignKey(d => d.ImportId)
                .OnDelete(DeleteBehavior.Cascade);

            // Example: Connecting InvoiceDetails to Invoices

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InvoiceDetail>()
                .HasOne(d => d.Book)
                .WithMany() // Or .WithMany(b => b.InvoiceDetails) if added that collection to Book.cs
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InvoiceDetail>()
                .HasOne(d => d.Invoice)
                .WithMany(i => i.InvoiceDetails)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PromotionTarget>()
                .HasOne(pt => pt.Book)
                .WithMany()
                .HasForeignKey(pt => pt.BookId)
                .OnDelete(DeleteBehavior.Restrict);

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