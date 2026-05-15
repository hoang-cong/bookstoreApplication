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
    public class PromotionViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;

        public ObservableCollection<Promotion> Promotions { get; }

        private Promotion _selectedPromotion;
        public Promotion SelectedPromotion
        {
            get => _selectedPromotion;
            set { _selectedPromotion = value; OnPropertyChanged(); }
        }

        public event Action<Page> NavigateRequested;

        public ICommand AddPromotionCommand { get; }
        public ICommand EditPromotionCommand { get; }
        public ICommand DeletePromotionCommand { get; }

        public PromotionViewModel(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;

            using var context = _factory.CreateDbContext();
            Promotions = new ObservableCollection<Promotion>(context.Promotions.ToList());

            AddPromotionCommand = new RelayCommand(_ => AddPromotion());
            EditPromotionCommand = new RelayCommand(_ => EditPromotion(), _ => SelectedPromotion != null);
            DeletePromotionCommand = new RelayCommand(_ => DeletePromotion(), _ => SelectedPromotion != null);
        }

        private void AddPromotion()
        {
            // Navigate to AddPromotionPage or open dialog
            var vm = new AddPromotionViewModel(_factory);
            NavigateRequested?.Invoke(new AddPromotionPage(vm));
        }

        private void EditPromotion()
        {
            // Navigate to EditPromotionPage with SelectedPromotion
            var vm = new EditPromotionViewModel(_factory, SelectedPromotion.PromotionId);
            NavigateRequested?.Invoke(new EditPromotionPage(vm));
        }

        private void DeletePromotion()
        {
            using var context = _factory.CreateDbContext();
            context.Promotions.Remove(SelectedPromotion);
            context.SaveChanges();
            Promotions.Remove(SelectedPromotion);
        }
    }
}
