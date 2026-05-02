using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QL_Nha_sach.Data;
using QL_Nha_sach.Models;
using QL_Nha_sach.Pages;
using QL_Nha_sach.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace QL_Nha_sach.ViewModels
{
    public class HomeScreenViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly SessionManager _session;

        public string FullName { get; set; }
        public string RoleName { get; set; }

        // Commands
        public ICommand ManageBooksCommand { get; }
        public ICommand AddBooksCommand { get; }
        public ICommand CreateInvoiceCommand { get; }
        public ICommand OpenInvoiceListCommand { get; }
        public ICommand ImportBooksCommand { get; }
        public ICommand OpenImportListCommand { get; }
        public ICommand OpenTransactionListCommand { get; }

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

            ManageBooksCommand = new RelayCommand(_ => OpenBookManagement(), _ => IsManager);
            AddBooksCommand = new RelayCommand(_ => OpenAddBook(), _ => IsManager);
            CreateInvoiceCommand = new RelayCommand(_ => OpenCreateInvoice(), _ => IsStaff);
            OpenInvoiceListCommand = new RelayCommand(_ => OpenInvoiceList(), _ => IsStaff || IsManager);
            ImportBooksCommand = new RelayCommand(_ => OpenImportBooks(), _ => IsStocker);
            OpenImportListCommand = new RelayCommand(_ => OpenImportList(), _ => IsStocker || IsManager);
            OpenTransactionListCommand = new RelayCommand(_ => OpenTransactionList(), _ => IsManager);
        }

        private void OpenBookManagement() 
        {
            // navigate to BookManagementPage
            var vm = new BookViewModel(_session, _factory);
            NavigateRequested?.Invoke(new BookManagementPage(vm));
        }

        private void OpenAddBook()
        {
            // navigate to AddBookPage
            var vm = new AddBookViewModel(_session, _factory);
            NavigateRequested?.Invoke(new AddBookPage(vm));
        }

        private void OpenCreateInvoice()
        {
            var vm = new InvoiceViewModel(_session, _factory);
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

        private void OpenTransactionList()
        {
            var vm = new TransactionListViewModel(_session, _factory);
            NavigateRequested?.Invoke(new TransactionListPage(vm));
        }
    }
}
