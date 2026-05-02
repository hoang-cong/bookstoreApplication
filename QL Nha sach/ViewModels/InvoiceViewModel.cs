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

namespace QL_Nha_sach.ViewModels
{
    public class InvoiceViewModel : FormViewModel<InvoiceDetail>
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly SessionManager _session;

        public IDbContextFactory<AppDbContext> Factory => _factory;

        public InvoiceViewModel(SessionManager session, IDbContextFactory<AppDbContext> factory) : base(session, factory)
        {
            _session = session;
            _factory = factory;
        }

        protected override void SaveForm(object parameter)
        {
            using var context = _factory.CreateDbContext();

            var invoice = new Invoice
            {
                InvoiceDate = Date,
                UserId = StaffId,
                Total = Total,
                InvoiceDetails = Details.Select(static d => new InvoiceDetail
                {
                    BookId = d.BookId,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    Discount = d.Discount,
                }).ToList()
            };

            foreach (var detail in Details)
            {
                var book = context.Books.FirstOrDefault(b => b.BookId == detail.BookId);
                if (book != null)
                {
                    book.Stock -= detail.Quantity;
                }
            }

            context.Invoices.Add(invoice);
            context.SaveChanges();
        }
    }
}
