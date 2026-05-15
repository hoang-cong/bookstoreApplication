using Microsoft.EntityFrameworkCore;
using QL_Nha_sach.Data;
using QL_Nha_sach.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls;

namespace QL_Nha_sach.ViewModels
{
    public class ReportViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;

        public ICommand OpenSalesReportCommand { get; }
        public ICommand OpenInventoryReportCommand { get; }

        public SalesReportViewModel SalesReport { get; }
        public InventoryReportViewModel InventoryReport { get; }

        public event Action<Page> NavigateRequested;

        public ReportViewModel(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;

            OpenSalesReportCommand = new RelayCommand(_ => OpenSalesReport());
            OpenInventoryReportCommand = new RelayCommand(_ => OpenInventoryReport());

            SalesReport = new SalesReportViewModel(_factory);
            InventoryReport = new InventoryReportViewModel(_factory);
        }

        private void OpenSalesReport()
        {
            var vm = new SalesReportViewModel(_factory);
            NavigateRequested?.Invoke(new SalesReportPage(vm));
        }
        private void OpenInventoryReport()
        {
            var vm = new InventoryReportViewModel(_factory);
            NavigateRequested?.Invoke(new InventoryReportPage(vm));
        }
    }
}
