using Microsoft.EntityFrameworkCore;
using QL_Nha_sach.Data;
using QL_Nha_sach.Models;
using QL_Nha_sach.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
//9780743273565
namespace QL_Nha_sach.ViewModels
{
    public class InvoiceViewModel : FormViewModel<InvoiceDetail>
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly SessionManager _session;
        private readonly InvoiceService _invoiceService;

        public IDbContextFactory<AppDbContext> Factory => _factory;

        public InvoiceViewModel(SessionManager session, IDbContextFactory<AppDbContext> factory, InvoiceService invoiceService) : base(session, factory)
        {
            _session = session;
            _factory = factory;
            _invoiceService = invoiceService;

            // 🚀 LIVE UPDATES: Run the query automatically as books are scanned!
            Details.CollectionChanged += (s, e) =>
            {
                RecalculateLivePromotions();

                if (e.NewItems != null)
                {
                    foreach (InvoiceDetail item in e.NewItems)
                    {
                        // If user manually edits a Quantity column in the DataGrid, recalculate!
                        item.PropertyChanged += (sender, args) =>
                        {
                            if (args.PropertyName == nameof(InvoiceDetail.Quantity))
                            {
                                RecalculateLivePromotions();
                            }
                        };
                    }
                }
            };
        }

        public void RecalculateLivePromotions()
        {
            // Wrap live screen collection directly into a wrapper Invoice object
            var tempInvoice = new Invoice { InvoiceDetails = Details };

            // Pass it to your service. It will modify 'Details' properties live
            _invoiceService.ApplyPromotions(tempInvoice);

            // Notify the UI that the global bottom-line total needs to refresh
            OnPropertyChanged(nameof(Total));
        }

        protected override void SaveForm(object parameter)
        {
            using var context = _factory.CreateDbContext();
            var regulation = context.Regulations.FirstOrDefault();
            if (regulation == null)
            {
                ShowMessage("System regulations are not configured.", true);
                return;
            }

            // 1. Validate Stock first using the live data on screen
            foreach (var detail in Details)
            {
                var book = context.Books.FirstOrDefault(b => b.BookId == detail.BookId);
                if (book != null)
                {
                    if (book.Stock - detail.Quantity < regulation.MinStockAfterSale)
                    {
                        ShowMessage($"Cannot sell {detail.Quantity} of '{book.Title}'. Stock would fall below minimum allowed ({regulation.MinStockAfterSale}).", true);
                        return;
                    }
                    book.Stock -= detail.Quantity;
                }
            }

            // 2. Build the database invoice using your LIVE Details collection directly.
            // No clones! No broken data paths!
            var invoice = new Invoice
            {
                InvoiceDate = Date,
                UserId = StaffId,
                Total = Total,
                InvoiceDetails = Details // Direct reference linkage
            };

            context.Invoices.Add(invoice);
            context.SaveChanges();

            ShowMessage("Invoice created successfully.", false);
        }
    }
}
