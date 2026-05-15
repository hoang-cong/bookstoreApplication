using Microsoft.EntityFrameworkCore;
using QL_Nha_sach.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QL_Nha_sach.ViewModels
{
    public class SalesReportViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;

        private DateTime _selectedDate = DateTime.Now;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set { _selectedDate = value; OnPropertyChanged(); }
        }
        public class GenreSale
        {
            public string Genre { get; set; }
            public int Quantity { get; set; }
            public decimal Revenue { get; set; }
        }
        public decimal TotalRevenue { get; private set; }
        public int TotalQuantity { get; private set; }

        public ObservableCollection<GenreSale> GenreSales { get; } = new();

        public ICommand LoadReportCommand { get; }

        public SalesReportViewModel(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
            LoadReportCommand = new RelayCommand(_ => LoadMonthlySales());
        }

        private void LoadMonthlySales()
        {
            using var context = _factory.CreateDbContext();
            var start = new DateTime(SelectedDate.Year, SelectedDate.Month, 1);
            var end = start.AddMonths(1);

            var details = context.InvoiceDetails
                .Include(d => d.Book).ThenInclude(b => b.BookGenres).ThenInclude(bg => bg.Genre)
                .Include(d => d.Invoice)
                .Where(d => d.Invoice.InvoiceDate >= start && d.Invoice.InvoiceDate < end)
                .ToList();

            TotalQuantity = details.Sum(d => d.Quantity);
            TotalRevenue = details.Sum(d => d.Quantity * d.UnitPrice * (decimal)(1 - (d.Discount / 100)));

            var sales = details
                .SelectMany(d => d.Book.BookGenres.Select(bg => new
                {
                    Genre = bg.Genre.GenreName,
                    Quantity = d.Quantity,
                    Revenue = d.Quantity * d.UnitPrice * (decimal)(1 - (d.Discount / 100))
                }))
                .GroupBy(x => x.Genre)
                .Select(g => new { Genre = g.Key, Quantity = g.Sum(x => x.Quantity), Revenue = g.Sum(x => x.Revenue) })
                .ToList();

            GenreSales.Clear();
            foreach (var s in sales)
                GenreSales.Add(new GenreSale { Genre = s.Genre, Quantity = s.Quantity, Revenue = s.Revenue });

            OnPropertyChanged(nameof(TotalQuantity));
            OnPropertyChanged(nameof(TotalRevenue));
        }
    }
}
