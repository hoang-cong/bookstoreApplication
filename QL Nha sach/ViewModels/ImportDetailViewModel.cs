using Microsoft.EntityFrameworkCore;
using QL_Nha_sach.Data;
using QL_Nha_sach.Models;
using QL_Nha_sach.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QL_Nha_sach.ViewModels
{
    public class ImportDetailViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly SessionManager _session;

        public Import? CurrentImport { get; private set; }

        public int ImportId => CurrentImport.ImportId;
        public DateTime Date => CurrentImport.ImportDate;
        public string StaffName => CurrentImport.User?.FullName ?? string.Empty;

        public ObservableCollection<ImportDetail> Details { get; private set; }

        public ICommand VoidImportCommand { get; }

        public ImportDetailViewModel(SessionManager session, IDbContextFactory<AppDbContext> factory, int ImportId)
        {
            _session = session;
            _factory = factory;

            using var context = _factory.CreateDbContext();
            CurrentImport = context.Imports
                .Include(i => i.User)
                .Include(i => i.ImportDetails)
                    .ThenInclude(d => d.Book)
                .FirstOrDefault(i => i.ImportId == ImportId);

            Details = new ObservableCollection<ImportDetail>(CurrentImport.ImportDetails);

            VoidImportCommand = new RelayCommand(ExecuteVoidImport, CanVoidImport);
        }

        private bool CanVoidImport(object parameter) => CurrentImport != null && !CurrentImport.IsVoided;
        private void ExecuteVoidImport(object parameter)
        {
            using var context = _factory.CreateDbContext();
            var dbImport = context.Imports
                .Include(i => i.ImportDetails)
                .FirstOrDefault(i => i.ImportId == CurrentImport.ImportId);

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
                        book.Stock += detail.Quantity; // reverse the import
                    }
                }
                context.SaveChanges();

                CurrentImport.IsVoided = true;
                CurrentImport.VoidedByUserId = _session.CurrentUser.UserId;
                CurrentImport.VoidedByUser = context.Find<User>(_session.CurrentUser.UserId);

                OnPropertyChanged(nameof(CurrentImport));
                OnPropertyChanged(nameof(IsVoided));
                ShowMessage("Import voided successfully!", false);
            }
        }
        public bool IsVoided => CurrentImport?.IsVoided ?? false;
    }
}
