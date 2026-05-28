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
    public class PromotionViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly SessionManager _session;

        public ObservableCollection<Promotion> Promotions { get; }

        private Promotion _selectedPromotion;
        public Promotion SelectedPromotion
        {
            get => _selectedPromotion;
            set { _selectedPromotion = value; OnPropertyChanged(); }
        }

        public bool IsManager => _session.CurrentUser?.Role?.RoleName == "Manager";

        public event Action<Page> NavigateRequested;

        public ICommand AddPromotionCommand { get; }
        public ICommand EditPromotionCommand { get; }
        public ICommand DeletePromotionCommand { get; }

        public PromotionViewModel(SessionManager session, IDbContextFactory<AppDbContext> factory)
        {
            _session = session;
            _factory = factory;

            using var context = _factory.CreateDbContext();
            Promotions = new ObservableCollection<Promotion>(context.Promotions.ToList());

            AddPromotionCommand = new RelayCommand(_ => AddPromotion());
            EditPromotionCommand = new RelayCommand(_ => EditPromotion(), _ => SelectedPromotion != null);
            DeletePromotionCommand = new RelayCommand(_ => DeletePromotion(), _ => SelectedPromotion != null);
        }

        private void AddPromotion()
        {
            var vm = new AddPromotionViewModel(_factory);
            AddPromotionWindow window = new AddPromotionWindow(vm);
            if (Application.Current.MainWindow != null)
            {
                window.Owner = Application.Current.MainWindow;
            }
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();
        }

        private void EditPromotion()
        {
            // Navigate to EditPromotionPage with SelectedPromotion
            var vm = new EditPromotionViewModel(_factory, SelectedPromotion.PromotionId);
            EditPromotionWindow window = new EditPromotionWindow(vm);
            if (Application.Current.MainWindow != null)
            {
                window.Owner = Application.Current.MainWindow;
            }
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();
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
