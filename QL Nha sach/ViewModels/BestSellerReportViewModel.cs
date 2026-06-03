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
    public class BestSellerReportViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;

        private DateTime _selectedDate = DateTime.Now;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set { _selectedDate = value; OnPropertyChanged(); }
        }
        public class BestSeller
        {
            public string Title { get; set; }
            public int Quantity { get; set; }
            public string? CoverImageUrl { get; set; }
            public string? ISBN { get; set; }
        }

        public ObservableCollection<BestSeller> BestSellers { get; } = new();

        public ICommand LoadBestSellerReportCommand { get; }

        public BestSellerReportViewModel(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
            LoadBestSellerReportCommand = new RelayCommand(_ => LoadBestSellers());
        }

        private void LoadBestSellers()
        {
            using var context = _factory.CreateDbContext();
            var start = new DateTime(SelectedDate.Year, SelectedDate.Month, 1);
            var end = start.AddMonths(1);

            var topBooks = context.InvoiceDetails
                .Include(d => d.Book)
                .Include(d => d.Invoice)
                .Where(d => d.Invoice != null
                    && !d.Invoice.IsVoided
                    && d.Invoice.InvoiceDate >= start
                    && d.Invoice.InvoiceDate < end)
                .GroupBy(d => d.Book)
                .Select(g => new
                {
                    BookObject = g.Key,
                    TotalSold = g.Sum(d => d.Quantity)
                })
                .OrderByDescending(g => g.TotalSold)
                .Take(10)
                .ToList();

            BestSellers.Clear();
            foreach (var item in topBooks)
            {
                if (item.BookObject == null) continue;

                BestSellers.Add(new BestSeller
                {
                    Title = item.BookObject.Title,
                    Quantity = item.TotalSold,
                    CoverImageUrl = item.BookObject.CoverImageUrl,
                    ISBN = item.BookObject.ISBN
                });
            }
        }
    }
}
