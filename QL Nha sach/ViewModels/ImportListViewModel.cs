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
    public class ImportListViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly SessionManager _session;

        private Import? _selectedImport;

        public ObservableCollection<Import> _imports { get; set; }
        public ObservableCollection<Import> Imports
        {
            get => _imports;
            set { _imports = value; OnPropertyChanged(); }
        }
        public Import SelectedImport
        {
            get => _selectedImport;
            set
            {
                _selectedImport = value;
                OnPropertyChanged();
            }
        }

        public event Action<Page> NavigateRequested;

        public ICommand ViewImportDetailCommand { get; }

        public ImportListViewModel(SessionManager session, IDbContextFactory<AppDbContext> factory)
        {
            _session = session;
            _factory = factory;

            using var context = factory.CreateDbContext();
            Imports = new ObservableCollection<Import>(
                context.Imports.Include(i => i.ImportDetails).ToList()
            );

            ViewImportDetailCommand = new RelayCommand(ExecuteViewImportDetail);
            LoadImports();
        }

        private void LoadImports()
        {
            using var context = _factory.CreateDbContext();
            Imports = new ObservableCollection<Import>(
                context.Imports.Include(i => i.ImportDetails).ToList()
            );
        }

        private void ExecuteViewImportDetail(object parameter)
        {
            if (SelectedImport == null)
            {
                MessageBox.Show("Please select an Import to see detail.");
                return;
            }

            var vm = new ImportDetailViewModel(_session, _factory, SelectedImport.ImportId);
            NavigateRequested?.Invoke(new ImportDetailPage(vm));
        }
    }
}
