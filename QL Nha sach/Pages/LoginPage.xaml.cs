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
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        private readonly LoginViewModel _vm;
        private readonly SessionManager _session;

        public LoginPage(LoginViewModel vm, SessionManager session)
        {
            InitializeComponent();
            _vm = vm;
            _session = session;
            this.DataContext = _vm;

            _vm.LoginSucceeded += OnLoginSucceeded;
            vm.NavigateRequested += page =>
            {
                NavigationService.Navigate(page);
            };
        }

        private void OnLoginSucceeded(User user)
        {
            _session.SetUser(user);
            
            var factory = App.AppHost.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();

            switch (user.Role.RoleName)
            {
                case "Manager":
                    NavigationService.Navigate(new ManagerHomePage(new HomeScreenViewModel(_session, factory), _session));
                    break;
                case "Staff":
                    NavigationService.Navigate(new StaffHomePage(new HomeScreenViewModel(_session, factory), _session));
                    break;
                case "Stocker":
                    NavigationService.Navigate(new StockerHomePage(new HomeScreenViewModel(_session, factory), _session));
                    break;
            }
        }

    }
}
