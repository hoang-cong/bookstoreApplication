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
using System.Windows.Shapes;

namespace QL_Nha_sach.Pages
{
    /// <summary>
    /// Interaction logic for EditPromotionWindow.xaml
    /// </summary>
    public partial class EditPromotionWindow : Window
    {
        public EditPromotionWindow(EditPromotionViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
