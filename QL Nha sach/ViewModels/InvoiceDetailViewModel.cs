using Microsoft.EntityFrameworkCore;
using QL_Nha_sach.Data;
using QL_Nha_sach.Models;
using QL_Nha_sach.Services;
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
    public class InvoiceDetailViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly SessionManager _session;
        private decimal _totalAmount;

        public Invoice? CurrentInvoice { get; private set; }

        public int InvoiceId => CurrentInvoice.InvoiceId;
        public DateTime Date => CurrentInvoice.InvoiceDate;
        public string StaffName => CurrentInvoice.User?.FullName ?? string.Empty;
        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                _totalAmount = value;
                OnPropertyChanged(nameof(TotalAmount));
            }
        }

        public ObservableCollection<InvoiceDetail> Details { get; private set; }

        public ICommand VoidInvoiceCommand { get; }

        public InvoiceDetailViewModel(SessionManager session, IDbContextFactory<AppDbContext> factory, int invoiceId)
        {
            _session = session;
            _factory = factory;

            using var context = _factory.CreateDbContext();
            CurrentInvoice = context.Invoices
                .Include(i => i.User)
                .Include(i => i.InvoiceDetails)
                    .ThenInclude(d => d.Book)
                .FirstOrDefault(i => i.InvoiceId == invoiceId);

            Details = new ObservableCollection<InvoiceDetail>(CurrentInvoice.InvoiceDetails);

            TotalAmount = CurrentInvoice.InvoiceDetails.Sum(d => d.Quantity * d.UnitPrice);

            VoidInvoiceCommand = new RelayCommand(ExecuteVoidInvoice, CanVoidInvoice);
        }

        private bool CanVoidInvoice(object parameter) => CurrentInvoice != null && !CurrentInvoice.IsVoided;
        private void ExecuteVoidInvoice(object parameter)
        {
            using var context = _factory.CreateDbContext();
            var dbInvoice = context.Invoices
                .Include(i => i.InvoiceDetails)
                .FirstOrDefault(i => i.InvoiceId == CurrentInvoice.InvoiceId);

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
                        book.Stock += detail.Quantity; // reverse sale
                    }
                }
                context.SaveChanges();

                CurrentInvoice.IsVoided = true;
                CurrentInvoice.VoidedByUserId = _session.CurrentUser.UserId;
                CurrentInvoice.VoidedByUser = context.Find<User>(_session.CurrentUser.UserId);

                OnPropertyChanged(nameof(CurrentInvoice));
                OnPropertyChanged(nameof(IsVoided)); // if you expose IsVoided separately
                OnPropertyChanged(nameof(TotalAmount));
                ShowMessage("Invoice voided successfully!", false);
            }
        }
        public bool IsVoided => CurrentInvoice?.IsVoided ?? false;
    }
}
