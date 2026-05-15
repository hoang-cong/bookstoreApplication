using Microsoft.EntityFrameworkCore;
using QL_Nha_sach.Data;
using QL_Nha_sach.Models;
using QL_Nha_sach.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QL_Nha_sach.ViewModels
{
    public class ImportViewModel : FormViewModel<ImportDetail>
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly SessionManager _session;

        public IDbContextFactory<AppDbContext> Factory => _factory;

        public ImportViewModel(SessionManager session, IDbContextFactory<AppDbContext> factory) : base(session, factory)
        {
            _session = session;
            _factory = factory;
        }

        protected override void SaveForm(object parameter)
        {
            using var context = _factory.CreateDbContext();
            var regulation = context.Regulations.FirstOrDefault();
            if (regulation == null)
            {
                ErrorMessage = "System regulations are not configured.";
                return;
            }

            var import = new Import
            {
                ImportDate = Date,
                UserId = StaffId,
                Total = Total,
                ImportDetails = Details.Select(d => new ImportDetail
                {
                    BookId = d.BookId,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                }).ToList()
            };

            foreach (var detail in Details)
            {
                if (detail.Quantity < regulation.MinImport)
                {
                    ShowMessage($"Import quantity for book {detail.Title} must be at least {regulation.MinImport} units.", true);
                    return;
                }

                var book = context.Books.FirstOrDefault(b => b.BookId == detail.BookId);
                if (book != null)
                {
                    // Check stock threshold
                    if (book.Stock >= regulation.StockThresholdForImport)
                    {
                        ShowMessage($"Book '{book.Title}' already has {book.Stock} units, above the import threshold ({regulation.StockThresholdForImport}).", true);
                        return;
                    }

                    // Check max stock
                    if (book.Stock + detail.Quantity > regulation.MaxStock)
                    {
                        ShowMessage($"Importing {detail.Quantity} units of '{book.Title}' would exceed max stock ({regulation.MaxStock}).", true);
                        return;
                    }

                    book.Stock += detail.Quantity;
                }
            }

            context.Imports.Add(import);
            context.SaveChanges();
            ShowMessage("Invoice saved successfully!", false);
        }
    }

}
