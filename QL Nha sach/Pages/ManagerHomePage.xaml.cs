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
    /// Interaction logic for ManagerHomePage.xaml
    /// </summary>
    public partial class ManagerHomePage : Page
    {
        private readonly SessionManager _session;
        public ManagerHomePage(HomeScreenViewModel vm, SessionManager session)
        {
            InitializeComponent();
            DataContext = vm;
            _session = session;
            vm.NavigateRequested += OnNavigateRequested;
        }

        private void OnNavigateRequested(Page page)
        {
            NavigationService?.Navigate(page);
        }
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            _session.Logout(NavigationService);
        }
    }


}
