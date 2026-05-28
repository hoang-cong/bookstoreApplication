using QL_Nha_sach.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QL_Nha_sach.Models
{
    // MASTER: The overall receipt info
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal Total { get; set; }
        public bool IsVoided { get; set; } = false;
        
        // The Audit Link
        public int UserId { get; set; }
        public virtual User? User { get; set; }
        public int? VoidedByUserId { get; set; }
        public virtual User? VoidedByUser { get; set; }

        public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();
    }

    public class InvoiceDetail : BaseViewModel, IFormDetail
    {
        public int InvoiceId { get; set; } // Links back to Master
        public virtual Invoice? Invoice { get; set; }

        public int BookId { get; set; } // Which book?
        public virtual Book? Book { get; set; }
        
        public string Title { get; set; } = string.Empty; // snapshot
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } // take a snap shot of the price
        private double _discount;
        public double Discount
        {
            get => _discount;
            set
            {
                if (_discount != value)
                {
                    _discount = value;
                    OnPropertyChanged(nameof(Discount));
                    OnPropertyChanged(nameof(SubTotal)); // Alert the UI that SubTotal changed too!
                }
            }
        }
        public decimal SubTotal => (decimal)Quantity * UnitPrice * (decimal)(1 - (Discount / 100));

        [NotMapped]
        public string ISBN { get; set; } = string.Empty;
        [NotMapped]
        public string ErrorMessage { get; set; }
        [NotMapped]
        public bool IsError { get; set; }
    }
}