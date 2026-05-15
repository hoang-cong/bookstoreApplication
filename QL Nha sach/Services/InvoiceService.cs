using Microsoft.EntityFrameworkCore;
using QL_Nha_sach.Data;
using QL_Nha_sach.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QL_Nha_sach.Services
{
    public class InvoiceService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;

        public InvoiceService(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
        }

        public void ApplyPromotions(Invoice invoice)
        {
            using var context = _factory.CreateDbContext();

            foreach (var detail in invoice.InvoiceDetails)
            {
                var activePromotions = context.PromotionTargets
                    .Include(pt => pt.Promotion)
                    .Where(pt => pt.BookId == detail.BookId &&
                                 pt.Promotion.StartDate <= DateTime.Now &&
                                 pt.Promotion.EndDate >= DateTime.Now)
                    .Select(pt => pt.Promotion)
                    .ToList();

                detail.Discount = activePromotions.Any()
                    ? activePromotions.Max(p => p.Discount)
                    : 0;
            }

            invoice.Total = invoice.InvoiceDetails.Sum(d => d.SubTotal);
        }
    }

}
