using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QL_Nha_sach.Data;
using QL_Nha_sach.Models;
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
    /// Interaction logic for BookManagementPage.xaml
    /// </summary>
    public partial class BookManagementPage : Page
    {
        public BookManagementPage(BookViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;

            vm.NavigateRequested += page =>
            {
                NavigationService.Navigate(page);
            };
        }
        
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = App.AppHost.Services.GetRequiredService<ManagerHomePage>();
            NavigationService?.Navigate(vm);
        }
    }
}
