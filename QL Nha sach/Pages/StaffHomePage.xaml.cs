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
    /// Interaction logic for StaffHomePage.xaml
    /// </summary>
    public partial class StaffHomePage : Page
    {
        private readonly SessionManager _session;
        public StaffHomePage(HomeScreenViewModel vm, SessionManager session)
        {
            InitializeComponent();
            this.DataContext = vm;
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

        private void LanguageMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void LanguageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                string cultureCode = menuItem.Tag.ToString();
                SwitchLanguage(cultureCode);
            }
        }
        public void SwitchLanguage(string cultureCode)
        {
            ResourceDictionary dict = new ResourceDictionary();

            switch (cultureCode)
            {
                case "vi":
                    dict.Source = new Uri("../Controls/vi.xaml", UriKind.Relative);
                    break;
                case "en":
                default:
                    dict.Source = new Uri("../Controls/en.xaml", UriKind.Relative);
                    break;
            }

            var currentMergedDictionaries = Application.Current.Resources.MergedDictionaries;
            foreach (var d in currentMergedDictionaries.ToList())
            {
                if (d.Source != null && (d.Source.OriginalString.Contains("en") || d.Source.OriginalString.Contains("vi")))
                {
                    currentMergedDictionaries.Remove(d);
                }
            }
            currentMergedDictionaries.Add(dict);
        }
    }
}
