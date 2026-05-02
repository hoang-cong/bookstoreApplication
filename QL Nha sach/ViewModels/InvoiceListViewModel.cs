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
    public class InvoiceListViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly SessionManager _session;

        private Invoice? _selectedInvoice;

        public ObservableCollection<Invoice> _invoices { get; set; }
        public ObservableCollection<Invoice> Invoices
        {
            get => _invoices;
            set { _invoices = value; OnPropertyChanged(); }
        }
        public Invoice SelectedInvoice
        {
            get => _selectedInvoice;
            set
            {
                _selectedInvoice = value;
                OnPropertyChanged();
            }
        }

        public event Action<Page> NavigateRequested;

        public ICommand ViewInvoiceDetailCommand { get; }

        public InvoiceListViewModel(SessionManager session, IDbContextFactory<AppDbContext> factory)
        {
            _session = session;
            _factory = factory;

            using var context = factory.CreateDbContext();
            Invoices = new ObservableCollection<Invoice>(
                context.Invoices.Include(i => i.InvoiceDetails).ToList()
            );

            ViewInvoiceDetailCommand = new RelayCommand(ExecuteViewInvoiceDetail);
            LoadInvoices();
        }

        private void LoadInvoices()
        {
            using var context = _factory.CreateDbContext();
            Invoices = new ObservableCollection<Invoice>(
                context.Invoices.Include(i => i.InvoiceDetails).ToList()
            );
        }

        private void ExecuteViewInvoiceDetail(object parameter)
        {
            if (SelectedInvoice == null)
            {
                MessageBox.Show("Please select an invoice to see detail.");
                return;
            }

            var vm = new InvoiceDetailViewModel(_session, _factory, SelectedInvoice.InvoiceId);
            NavigateRequested?.Invoke(new InvoiceDetailPage(vm));
        }
    }
}
