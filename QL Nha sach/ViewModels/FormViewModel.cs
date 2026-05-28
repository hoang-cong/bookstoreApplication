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
using System.Windows.Input;

namespace QL_Nha_sach.ViewModels
{
    public class FormViewModel<TDetail> : BaseViewModel where TDetail : IFormDetail, new()
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly SessionManager _session;
        private TDetail _selectedDetail;
        public TDetail SelectedDetail
        {
            get => _selectedDetail;
            set
            {
                _selectedDetail = value;
                OnPropertyChanged(nameof(SelectedDetail));
            }
        }
        public string ScannedISBN { get; set; }

        // Header fields
        public int FormId { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public int StaffId { get; set; }
        public string StaffName { get; set; }

        // Child collection
        public ObservableCollection<TDetail> Details { get; set; }
            = new ObservableCollection<TDetail>();

        // Totals
        public decimal Total => Details.Sum(d => d.SubTotal);

        // Commands
        public ICommand AddScannedBookCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand SaveFormCommand { get; }

        public FormViewModel(SessionManager session, IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
            _session = session;

            StaffId = _session.CurrentUser?.UserId ?? 0;
            StaffName = _session.CurrentUser?.FullName ?? string.Empty;

            AddScannedBookCommand = new RelayCommand(AddScannedBook);
            RemoveItemCommand = new RelayCommand(RemoveItem);
            SaveFormCommand = new RelayCommand(SaveForm);
        }

        private void AddScannedBook(object parameter)
        {
            using var context = _factory.CreateDbContext();
            var book = context.Books.FirstOrDefault(b => b.ISBN == ScannedISBN);
            if (book != null)
            {
                if (book.BookStatusId == 3)
                {
                    ShowMessage("This book is not for sale", true);
                    return;
                }
                var existingDetail = Details.FirstOrDefault(d => d.BookId == book.BookId);
                if (existingDetail != null)
                {
                    existingDetail.Quantity += 1;
                }
                else
                {
                    Details.Add(new TDetail
                    {
                        BookId = book.BookId,
                        ISBN = book.ISBN,
                        Title = book.Title,
                        Quantity = 1,
                        UnitPrice = book.Price
                    });
                }
                OnPropertyChanged(nameof(Total));
            }
            ScannedISBN = string.Empty;
            OnPropertyChanged(nameof(ScannedISBN));
        }

        private void RemoveItem(object parameter)
        {
            if (parameter is TDetail item && Details.Contains(item))
            {
                Details.Remove(item);
                OnPropertyChanged(nameof(Total));
            }
        }

        protected virtual void SaveForm(object parameter)
        { }
    }
}
