using Microsoft.Extensions.DependencyInjection;
using QL_Nha_sach.Services;
using QL_Nha_sach.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QL_Nha_sach.Pages
{
    /// <summary>
    /// Interaction logic for InvoicePage.xaml
    /// </summary>
    public partial class InvoicePage : Page
    {
        public InvoicePage(InvoiceViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            var staffHome = App.AppHost.Services.GetRequiredService<StaffHomePage>();
            NavigationService?.Navigate(staffHome);
        }
    }
}
