using Microsoft.EntityFrameworkCore;
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

namespace QL_Nha_sach.ViewModels
{
    public class InvoiceListViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly SessionManager _session;

        private Invoice? _selectedInvoice;
        private string _searchText;
        private decimal _totalAmount;
        private ObservableCollection<Invoice> _invoices;
        private ObservableCollection<InvoiceDetail> _details;

        public ObservableCollection<Invoice> Invoices
        {
            get => _invoices;
            set { _invoices = value; OnPropertyChanged(); }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }
        public Invoice? SelectedInvoice
        {
            get => _selectedInvoice;
            set
            {
                _selectedInvoice = value;
                OnPropertyChanged();

                LoadInvoiceDetails();
            }
        }
        public ObservableCollection<InvoiceDetail> Details
        {
            get => _details;
            private set { _details = value; OnPropertyChanged(); }
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            private set { _totalAmount = value; OnPropertyChanged(); }
        }

        public string StaffName => SelectedInvoice?.User?.FullName ?? string.Empty;
        public bool IsVoided => SelectedInvoice?.IsVoided ?? false;

        public ICommand VoidInvoiceCommand { get; }

        public InvoiceListViewModel(SessionManager session, IDbContextFactory<AppDbContext> factory)
        {
            _session = session;
            _factory = factory;

            VoidInvoiceCommand = new RelayCommand(ExecuteVoidInvoice, CanVoidInvoice);

            LoadInvoices();
        }

        private void LoadInvoices()
        {
            using var context = _factory.CreateDbContext();
            Invoices = new ObservableCollection<Invoice>(
                context.Invoices
                    .Include(i => i.User)
                    .Include(i => i.VoidedByUser)
                    .OrderByDescending(i => i.InvoiceDate)
                    .ToList()
            );
        }

        private void ApplyFilter()
        {
            using var context = _factory.CreateDbContext();
            var query = context.Invoices
                .Include(i => i.User)
                .Include(i => i.VoidedByUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(i =>
                    i.InvoiceId.ToString().Contains(SearchText) ||
                    i.User.FullName.Contains(SearchText)
                );
            }
            Invoices = new ObservableCollection<Invoice>(query.OrderByDescending(i => i.InvoiceDate).ToList());
        }

        private void LoadInvoiceDetails()
        {
            if (SelectedInvoice == null)
            {
                Details = new ObservableCollection<InvoiceDetail>();
                TotalAmount = 0;
                OnPropertyChanged(nameof(StaffName));
                OnPropertyChanged(nameof(IsVoided));
                return;
            }

            using var context = _factory.CreateDbContext();

            var completeInvoice = context.Invoices
                .Include(i => i.User)
                .Include(i => i.InvoiceDetails)
                    .ThenInclude(d => d.Book)
                .FirstOrDefault(i => i.InvoiceId == SelectedInvoice.InvoiceId);

            if (completeInvoice != null)
            {
                Details = new ObservableCollection<InvoiceDetail>(completeInvoice.InvoiceDetails);
                TotalAmount = Details.Sum(d => d.SubTotal);

                SelectedInvoice.User = completeInvoice.User;
            }

            OnPropertyChanged(nameof(StaffName));
            OnPropertyChanged(nameof(IsVoided));
        }

        private bool CanVoidInvoice(object parameter) => SelectedInvoice != null && !SelectedInvoice.IsVoided;

        private void ExecuteVoidInvoice(object parameter)
        {
            if (SelectedInvoice == null) return;

            using var context = _factory.CreateDbContext();
            var dbInvoice = context.Invoices
                .Include(i => i.InvoiceDetails)
                .FirstOrDefault(i => i.InvoiceId == SelectedInvoice.InvoiceId);

            if (dbInvoice != null && !dbInvoice.IsVoided)
            {
                dbInvoice.IsVoided = true;
                dbInvoice.VoidedByUserId = _session.CurrentUser.UserId;

                var bookIds = dbInvoice.InvoiceDetails.Select(d => d.BookId).ToList();
                var books = context.Books
                    .Where(b => bookIds.Contains(b.BookId))
                    .ToDictionary(b => b.BookId);

                foreach (var detail in dbInvoice.InvoiceDetails)
                {
                    if (books.TryGetValue(detail.BookId, out var book))
                    {
                        book.Stock -= detail.Quantity;
                    }
                }
                context.SaveChanges();

                SelectedInvoice.IsVoided = true;
                SelectedInvoice.VoidedByUserId = _session.CurrentUser.UserId;
                SelectedInvoice.VoidedByUser = context.Find<User>(_session.CurrentUser.UserId);

                OnPropertyChanged(nameof(IsVoided));

                ShowMessage("Invoice voided successfully!", false);
            }
        }
    }
}
