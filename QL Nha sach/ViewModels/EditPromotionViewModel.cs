using Microsoft.EntityFrameworkCore;
using QL_Nha_sach.Data;
using QL_Nha_sach.Models;
using QL_Nha_sach.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace QL_Nha_sach.ViewModels
{
    public class EditPromotionViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly int _promotionId;

        public string Name { get; set; }
        public double DiscountPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool ApplyToAllBooks { get; set; }
        public ObservableCollection<Book> AvailableBooks { get; }
        public ObservableCollection<Book> SelectedBooks { get; } = new();

        public ICommand SaveCommand { get; }

        public EditPromotionViewModel(IDbContextFactory<AppDbContext> factory, int promotionId)
        {
            _factory = factory;
            _promotionId = promotionId;

            using var context = _factory.CreateDbContext();
            var promotion = context.Promotions
                                           .Include(p => p.PromotionTargets)
                                           .ThenInclude(pt => pt.Book)
                                           .FirstOrDefault(p => p.PromotionId == _promotionId);
            if (promotion != null)
            {
                Name = promotion.PromotionName;
                DiscountPercentage = promotion.Discount;
                StartDate = promotion.StartDate;
                EndDate = promotion.EndDate;

                AvailableBooks = new ObservableCollection<Book>(context.Books.ToList());

                foreach (var target in promotion.PromotionTargets)
                {
                    var book = AvailableBooks.FirstOrDefault(b => b.BookId == target.BookId);
                    if (book != null)
                        SelectedBooks.Add(book);
                }

                ApplyToAllBooks = SelectedBooks.Count == AvailableBooks.Count;
            }

            SaveCommand = new RelayCommand(_ => SavePromotion());
        }

        private void SavePromotion()
        {
            using var context = _factory.CreateDbContext();
            var promotion = context.Promotions
                .Include(p => p.PromotionTargets)
                .FirstOrDefault(p => p.PromotionId == _promotionId);
            if (promotion != null)
            {
                promotion.PromotionName = Name;
                promotion.Discount = DiscountPercentage;
                promotion.StartDate = StartDate;
                promotion.EndDate = EndDate;

                context.PromotionTargets.RemoveRange(promotion.PromotionTargets);

                if (ApplyToAllBooks)
                {
                    foreach (var book in context.Books.ToList())
                    {
                        context.PromotionTargets.Add(new PromotionTarget
                        {
                            PromotionId = promotion.PromotionId,
                            BookId = book.BookId
                        });
                    }
                }
                else
                {
                    foreach (var book in SelectedBooks)
                    {
                        context.PromotionTargets.Add(new PromotionTarget
                        {
                            PromotionId = promotion.PromotionId,
                            BookId = book.BookId
                        });
                    }
                }

                context.SaveChanges();
                ShowMessage("Promotion updated successfully!", isError: false);
            }
        }
    }

}
