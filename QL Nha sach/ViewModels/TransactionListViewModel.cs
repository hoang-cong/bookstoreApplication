using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
    public class TransactionListViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly SessionManager _session;

        private Invoice _selectedInvoice;
        public Invoice SelectedInvoice
        {
            get => _selectedInvoice;
            set { _selectedInvoice = value; OnPropertyChanged(); }
        }

        private Import _selectedImport;
        public Import SelectedImport
        {
            get => _selectedImport;
            set { _selectedImport = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Invoice> Invoices { get; }
        public ObservableCollection<Import> Imports { get; }

        public ICommand OpenInvoiceDetailCommand { get; }
        public ICommand OpenImportDetailCommand { get; }

        public event Action<Page> NavigateRequested;

        public TransactionListViewModel(SessionManager session, IDbContextFactory<AppDbContext> factory)
        {
            _session = session;
            _factory = factory;

            using var context = _factory.CreateDbContext();

            Invoices = new ObservableCollection<Invoice>(
                context.Invoices
                       .Include(i => i.User)
                       .Include(i => i.InvoiceDetails)
                       .ThenInclude(d => d.Book)
                       .ToList()
            );

            Imports = new ObservableCollection<Import>(
                context.Imports
                       .Include(im => im.User)
                       .Include(im => im.ImportDetails)
                       .ThenInclude(d => d.Book)
                       .ToList()
            );

            OpenInvoiceDetailCommand = new RelayCommand(ExecuteViewInvoiceDetail);

            OpenImportDetailCommand = new RelayCommand(ExecuteViewImportDetail);
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

        private void ExecuteViewImportDetail(object parameter)
        {
            if (SelectedInvoice == null)
            {
                MessageBox.Show("Please select an import to see detail.");
                return;
            }

            var vm = new ImportDetailViewModel(_session, _factory, SelectedInvoice.InvoiceId);
            NavigateRequested?.Invoke(new ImportDetailPage(vm));
        }
    }
}
