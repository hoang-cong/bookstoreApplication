using Microsoft.EntityFrameworkCore;
using QL_Nha_sach.Data;
using QL_Nha_sach.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QL_Nha_sach.ViewModels
{
    public class InventoryReportViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;

        private DateTime _selectedDate = DateTime.Now;

        public int TotalBooksCount { get; private set; }
        public int LowStockCount { get; private set; }
        public decimal TotalInventoryValue { get; private set; }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set { _selectedDate = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Book> LowStockBooks { get; } = new();

        public ICommand LoadInventoryCommand { get; }

        public InventoryReportViewModel(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
            LoadInventoryCommand = new RelayCommand(_ => LoadInventory());
            LoadInventory();
        }

        private void LoadInventory()
        {
            using var context = _factory.CreateDbContext();

            var threshold = context.Regulations.Select(r => r.StockThresholdForImport).FirstOrDefault();
            if (threshold == 0) threshold = 5; // Fallback

            var allBooks = context.Books.Include(b => b.BookStatus).ToList();

            var sortedBooks = allBooks
                .OrderBy(b => b.Stock)
                .ToList();

            TotalBooksCount = allBooks.Sum(b => b.Stock);
            LowStockCount = allBooks.Count(b => b.Stock <= threshold); // Count low stock for the KPI card
            TotalInventoryValue = allBooks.Sum(b => b.Stock * b.Price);

            LowStockBooks.Clear();
            foreach (var b in sortedBooks) LowStockBooks.Add(b);

            // Notify UI of totals
            OnPropertyChanged(nameof(TotalBooksCount));
            OnPropertyChanged(nameof(LowStockCount));
            OnPropertyChanged(nameof(TotalInventoryValue));
        }
    }
}