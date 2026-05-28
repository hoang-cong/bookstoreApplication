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

            //foreach (var detail in invoice.InvoiceDetails)
            //{
            //    var activePromotions = context.PromotionTargets
            //        .Include(pt => pt.Promotion)
            //        .Where(pt => pt.BookId == detail.BookId &&
            //                     pt.Promotion.StartDate <= DateTime.Now &&
            //                     pt.Promotion.EndDate >= DateTime.Now)
            //        .Select(pt => pt.Promotion)
            //        .ToList();

            //    detail.Discount = activePromotions.Any()
            //        ? activePromotions.Max(p => p.Discount)
            //        : 0;
            //}

            //invoice.Total = invoice.InvoiceDetails.Sum(d => d.SubTotal);

            if (invoice?.InvoiceDetails == null || !invoice.InvoiceDetails.Any()) return;

            // 1. Gather all BookIds from the invoice
            var bookIds = invoice.InvoiceDetails.Select(d => d.BookId).Distinct().ToList();

            // 2. ONE SINGLE HIT: Pull all matching active promotions
            var activePromotionsLookup = context.PromotionTargets
                .Include(pt => pt.Promotion)
                .Where(pt => bookIds.Contains(pt.BookId) &&
                             pt.Promotion.StartDate <= DateTime.Now &&
                             pt.Promotion.EndDate >= DateTime.Now)
                .ToList()
                .GroupBy(pt => pt.BookId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Max(pt => pt.Promotion.Discount) // Find the best discount per book
                );

            // 3. Update the live properties (which triggers WPF UI updates instantly)
            foreach (var detail in invoice.InvoiceDetails)
            {
                if (activePromotionsLookup.TryGetValue(detail.BookId, out var maxDiscount))
                {
                    detail.Discount = maxDiscount;
                }
                else
                {
                    detail.Discount = 0;
                }
            }

            // 4. Calculate final overall total
            invoice.Total = invoice.InvoiceDetails.Sum(d => d.SubTotal);
        }
    }

}
