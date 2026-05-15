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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using static QL_Nha_sach.ViewModels.BestSellerReportViewModel;

namespace QL_Nha_sach.ViewModels
{
    public class HomeScreenViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly SessionManager _session;
        private ObservableCollection<Book> _books = new();
        private DateTime _date = DateTime.Now;

        public ObservableCollection<Promotion> Promotions { get; }
        public int TotalBooksCount => Books?.Sum(b => b.Stock) ?? 0;
        public int UniqueTitlesCount => Books?.Select(b => b.Title).Distinct().Count() ?? 0;
        public int TotalUsersCount { get; set; }
        public int LowStockCount { get; set; }
        public ObservableCollection<BestSeller> HomeBestSellers { get; } = new();

        public string FullName { get; set; }
        public string RoleName { get; set; }

        public ObservableCollection<Book> Books
        {
            get => _books;
            set { _books = value; OnPropertyChanged(); }
        }
        public ObservableCollection<Book> NewBooks { get; } = new ObservableCollection<Book>();
        public DateTime Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(); }
        }

        // Commands
        public ICommand ManageBooksCommand { get; }
        public ICommand ManagePromotionCommand { get; }
        public ICommand ViewPromotionCommand { get; }
        public ICommand CreateInvoiceCommand { get; }
        public ICommand OpenInvoiceListCommand { get; }
        public ICommand ImportBooksCommand { get; }
        public ICommand OpenImportListCommand { get; }
        public ICommand ManageStockCommand { get; }
        public ICommand OpenTransactionListCommand { get; }
        public ICommand OpenBestSellerCommand { get; }
        public ICommand ManageUserCommand { get; }
        public ICommand OpenReportCommand { get; }
        public ICommand ManageRegulationCommand { get; }

        public ICommand OpenSettingsCommand { get; }

        // Role flags
        public bool IsManager => _session.CurrentUser?.Role?.RoleName == "Manager";
        public bool IsStaff => _session.CurrentUser?.Role?.RoleName == "Staff";
        public bool IsStocker => _session.CurrentUser?.Role?.RoleName == "Stocker";

        // Event for navigation (raised to the View)
        public event Action<Page> NavigateRequested;

        public HomeScreenViewModel(SessionManager session, IDbContextFactory<AppDbContext> factory)
        {
            _session = session;
            _factory = factory;

            FullName = _session.FullName;
            RoleName = _session.RoleName;

            using var context = _factory.CreateDbContext();
            Promotions = new ObservableCollection<Promotion>(context.Promotions.ToList());

            LoadData();

            ManageBooksCommand = new RelayCommand(_ => OpenBookManagement(), _ => IsManager);
            ManagePromotionCommand = new RelayCommand(_ => OpenPromotionManagement(), _ => IsManager);
            ViewPromotionCommand = new RelayCommand(_ => OpenViewPromotion(), _ => IsStaff);
            CreateInvoiceCommand = new RelayCommand(_ => OpenCreateInvoice(), _ => IsStaff);
            OpenInvoiceListCommand = new RelayCommand(_ => OpenInvoiceList(), _ => IsStaff || IsManager);
            ImportBooksCommand = new RelayCommand(_ => OpenImportBooks(), _ => IsStocker);
            OpenImportListCommand = new RelayCommand(_ => OpenImportList(), _ => IsStocker || IsManager);
            ManageStockCommand = new RelayCommand(_ => OpenStockManagement(), _ => IsStocker);
            OpenTransactionListCommand = new RelayCommand(_ => OpenTransactionList(), _ => IsManager);
            OpenBestSellerCommand = new RelayCommand(_ => OpenBestSeller(), _ => IsManager);
            ManageUserCommand = new RelayCommand(_ => OpenUserManagement(), _ => IsManager);
            OpenReportCommand = new RelayCommand(_ => OpenReport(), _ => IsManager);
            ManageRegulationCommand = new RelayCommand(_ => OpenRegulationManagement(), _ => IsManager);

            OpenSettingsCommand = new RelayCommand(_ => OpenSettings());
        }

        public void LoadData()
        {
            using var context = _factory.CreateDbContext();

            // Fetch everything from the DB
            var bookList = context.Books
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Include(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
                .Include(b => b.Publisher)
                .Include(b => b.BookStatus)
                .ToList();

            var threshold = context.Regulations.Select(r => r.StockThresholdForImport).FirstOrDefault();
            if (threshold == 0) threshold = 5;

            TotalUsersCount = context.Users.Count();
            LowStockCount = bookList.Count(b => b.Stock <= threshold);

            Books = new ObservableCollection<Book>(bookList);

            var latestBooks = context.Books
                .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
                .OrderByDescending(b => b.BookId)
                .Take(6)
                .ToList();

            NewBooks.Clear();
            foreach (var book in latestBooks)
            {
                NewBooks.Add(book);
            }

            var startOfMonth = new DateTime(Date.Year, Date.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);

            var topBooks = context.InvoiceDetails
                .Include(d => d.Book)
                .Include(d => d.Invoice)
                .Where(d => d.Invoice.InvoiceDate >= startOfMonth && d.Invoice.InvoiceDate < endOfMonth)
                .GroupBy(d => d.Book)
                .Select(g => new {
                    Book = g.Key,
                    TotalSold = g.Sum(d => d.Quantity)
                })
                .OrderByDescending(g => g.TotalSold)
                .Take(3)
                .ToList();

            HomeBestSellers.Clear();
            foreach (var item in topBooks)
            {
                if (item.Book == null) continue;
                HomeBestSellers.Add(new BestSeller
                {
                    Title = item.Book.Title,
                    Quantity = item.TotalSold,
                    ISBN = item.Book.ISBN
                });
            }

            context.ChangeTracker.Clear();

            // Notify UI
            OnPropertyChanged(nameof(Books));
            OnPropertyChanged(nameof(NewBooks));
            OnPropertyChanged(nameof(TotalBooksCount));
            OnPropertyChanged(nameof(UniqueTitlesCount));
            OnPropertyChanged(nameof(TotalUsersCount));
            OnPropertyChanged(nameof(LowStockCount));
            OnPropertyChanged(nameof(HomeBestSellers));
        }

        private void OpenBookManagement()
        {
            // navigate to BookManagementPage
            var vm = new BookViewModel(_session, _factory);
            NavigateRequested?.Invoke(new BookManagementPage(vm));
        }

        private void OpenPromotionManagement()
        {
            // navigate to PromotionManagementPage
            var vm = new PromotionViewModel(_factory);
            NavigateRequested?.Invoke(new PromotionListPage(vm));
        }

        private void OpenViewPromotion()
        {
            var vm = new PromotionViewModel(_factory);
            NavigateRequested?.Invoke(new ViewPromotionPage(vm));
        }

        private void OpenCreateInvoice()
        {
            var vm = App.AppHost!.Services.GetRequiredService<InvoiceViewModel>();
            NavigateRequested?.Invoke(new InvoicePage(vm));
        }

        private void OpenInvoiceList()
        {
            var vm = new InvoiceListViewModel(_session, _factory);
            NavigateRequested?.Invoke(new InvoiceListPage(vm));
        }

        private void OpenImportBooks()
        {
            var vm = new ImportViewModel(_session, _factory);
            NavigateRequested?.Invoke(new ImportPage(vm));
        }

        private void OpenImportList()
        {
            var vm = new ImportListViewModel(_session, _factory);
            NavigateRequested?.Invoke(new ImportListPage(vm));
        }

        private void OpenStockManagement()
        {
            var vm = new StockViewModel(_factory);
            NavigateRequested?.Invoke(new StockPage(vm));
        }

        private void OpenTransactionList()
        {
            var vm = new TransactionListViewModel(_session, _factory);
            NavigateRequested?.Invoke(new TransactionListPage(vm));
        }

        private void OpenBestSeller()
        {
            var vm = new BestSellerReportViewModel(_factory);
            NavigateRequested?.Invoke(new BestSellerReportPage(vm));
        }

        private void OpenUserManagement()
        {
            var vm = new UserManagementViewModel(_factory);
            NavigateRequested?.Invoke(new UserManagementPage(vm));
        }

        private void OpenReport()
        {
            var vm = new ReportViewModel(_factory);
            NavigateRequested?.Invoke(new ReportPage(vm));
        }

        private void OpenRegulationManagement()
        {
            var vm = new RegulationViewModel(_factory);
            NavigateRequested?.Invoke(new RegulationPage(vm));
        }

        private void OpenSettings()
        {
            //var vm = new SettingsViewModel(_session, _factory);
            //NavigateRequested?.Invoke(new SettingsPage(vm));
        }
    }
}