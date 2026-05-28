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
    public class AddPromotionViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;

        public string Name { get; set; }
        public double DiscountPercentage { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);

        public bool ApplyToAllBooks { get; set; } = true;
        public ObservableCollection<Book> AvailableBooks { get; }
        public ObservableCollection<Book> SelectedBooks { get; } = new();
        
        public ICommand SaveCommand { get; }

        public AddPromotionViewModel(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;

            using var context = _factory.CreateDbContext();
            AvailableBooks = new ObservableCollection<Book>(context.Books.ToList());

            SaveCommand = new RelayCommand(_ => SavePromotion());
        }

        private void SavePromotion()
        {
            using var context = _factory.CreateDbContext();
            var regulation = context.Regulations.FirstOrDefault();
            if (regulation == null)
            {
                ShowMessage("System regulations are not configured.", true);
                return;
            }

            var promotion = new Promotion
            {
                PromotionName = Name,
                Discount = DiscountPercentage,
                StartDate = StartDate,
                EndDate = EndDate,
            };

            if (StartDate > EndDate)
            {
                ShowMessage("Promotion start date must be before end date.", true);
                return;
            }
            if (DiscountPercentage <= 0 || DiscountPercentage > regulation.Discount)
            {
                ShowMessage($"Discount percentage must be between 0 and {regulation.Discount}.", true);
                return;
            }

            context.Promotions.Add(promotion);
            context.SaveChanges();

            if (ApplyToAllBooks)
            {
                var allBooks = context.Books.ToList();
                foreach (var book in allBooks)
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

            ShowMessage("Promotion added successfully!", false);
        }
    }
}
