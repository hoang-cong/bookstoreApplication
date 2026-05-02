using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QL_Nha_sach.Data;
using QL_Nha_sach.Models;
using QL_Nha_sach.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for EditBookWindow.xaml
    /// </summary>
    public partial class EditBookWindow : Window
    {
        public EditBookWindow(Book SelectedBook)
        {
            InitializeComponent();

            var factory = App.AppHost.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
            var vm = new EditBookViewModel(factory)
            {
                EditableBook = SelectedBook
            };
            vm.LoadData();

            DataContext = vm;
        }
        private void AuthorsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(DataContext is EditBookViewModel vm)
            {
                vm.SelectedAuthors = new ObservableCollection<Author>(
                    ((ListBox)sender).SelectedItems.Cast<Author>()
                );
            }
        }
        private void GenresListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(DataContext is EditBookViewModel vm)
            {
                vm.SelectedGenres = new ObservableCollection<Genre>(
                    ((ListBox)sender).SelectedItems.Cast<Genre>()
                );
            }
        }

    }
}
