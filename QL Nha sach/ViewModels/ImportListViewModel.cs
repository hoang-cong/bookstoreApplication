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
        private string _searchText;
        private decimal _totalAmount;
        private ObservableCollection<Import> _imports;
        private ObservableCollection<ImportDetail> _details;

        public ObservableCollection<Import> Imports
        {
            get => _imports;
            set { _imports = value; OnPropertyChanged(); }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }
        public Import? SelectedImport
        {
            get => _selectedImport;
            set
            {
                _selectedImport = value;
                OnPropertyChanged();

                LoadImportDetails();
            }
        }
        public ObservableCollection<ImportDetail> Details
        {
            get => _details;
            private set { _details = value; OnPropertyChanged(); }
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            private set { _totalAmount = value; OnPropertyChanged(); }
        }

        public string StaffName => SelectedImport?.User?.FullName ?? string.Empty;
        public bool IsVoided => SelectedImport?.IsVoided ?? false;

        public ICommand VoidImportCommand { get; }

        public ImportListViewModel(SessionManager session, IDbContextFactory<AppDbContext> factory)
        {
            _session = session;
            _factory = factory;

            VoidImportCommand = new RelayCommand(ExecuteVoidImport, CanVoidImport);

            LoadImports();
        }

        private void LoadImports()
        {
            using var context = _factory.CreateDbContext();
            Imports = new ObservableCollection<Import>(
                context.Imports
                    .Include(i => i.User)
                    .Include(i => i.VoidedByUser)
                    .OrderByDescending(i => i.ImportDate)
                    .ToList()
            );
        }

        private void ApplyFilter()
        {
            using var context = _factory.CreateDbContext();
            var query = context.Imports
                .Include(i => i.User)
                .Include(i => i.VoidedByUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(i =>
                    i.ImportId.ToString().Contains(SearchText) ||
                    i.User.FullName.Contains(SearchText)
                );
            }
            Imports = new ObservableCollection<Import>(query.OrderByDescending(i => i.ImportDate).ToList());
        }

        private void LoadImportDetails()
        {
            if (SelectedImport == null)
            {
                Details = new ObservableCollection<ImportDetail>();
                TotalAmount = 0;
                OnPropertyChanged(nameof(StaffName));
                OnPropertyChanged(nameof(IsVoided));
                return;
            }

            using var context = _factory.CreateDbContext();

            var completeImport = context.Imports
                .Include(i => i.User)
                .Include(i => i.ImportDetails)
                    .ThenInclude(d => d.Book)
                .FirstOrDefault(i => i.ImportId == SelectedImport.ImportId);

            if (completeImport != null)
            {
                Details = new ObservableCollection<ImportDetail>(completeImport.ImportDetails);
                TotalAmount = Details.Sum(d => d.SubTotal);

                SelectedImport.User = completeImport.User;
            }

            OnPropertyChanged(nameof(StaffName));
            OnPropertyChanged(nameof(IsVoided));
        }

        private bool CanVoidImport(object parameter) => SelectedImport != null && !SelectedImport.IsVoided;

        private void ExecuteVoidImport(object parameter)
        {
            if (SelectedImport == null) return;

            using var context = _factory.CreateDbContext();
            var dbImport = context.Imports
                .Include(i => i.ImportDetails)
                .FirstOrDefault(i => i.ImportId == SelectedImport.ImportId);

            if (dbImport != null && !dbImport.IsVoided)
            {
                dbImport.IsVoided = true;
                dbImport.VoidedByUserId = _session.CurrentUser.UserId;

                var bookIds = dbImport.ImportDetails.Select(d => d.BookId).ToList();
                var books = context.Books
                    .Where(b => bookIds.Contains(b.BookId))
                    .ToDictionary(b => b.BookId);

                foreach (var detail in dbImport.ImportDetails)
                {
                    if (books.TryGetValue(detail.BookId, out var book))
                    {
                        book.Stock -= detail.Quantity;
                    }
                }
                context.SaveChanges();

                SelectedImport.IsVoided = true;
                SelectedImport.VoidedByUserId = _session.CurrentUser.UserId;
                SelectedImport.VoidedByUser = context.Find<User>(_session.CurrentUser.UserId);

                OnPropertyChanged(nameof(IsVoided));

                ShowMessage("Import voided successfully!", false);
            }
        }
    }
}
