using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QL_Nha_sach.Models
{
    public class Import
    {
        [Key]
        public int ImportId { get; set; }
        public DateTime ImportDate { get; set; }
        public decimal Total { get; set; }
        public bool IsVoided { get; set; } = false;

        // The Audit Link
        public int UserId { get; set; }
        public virtual User? User {  get; set; }
        public int? VoidedByUserId { get; set; }
        public virtual User? VoidedByUser { get; set; }

        public virtual ICollection<ImportDetail> ImportDetails { get; set; } = new List<ImportDetail>();
    }

    public class ImportDetail : IFormDetail
    {
        public int ImportId { get; set; } // Link to Parent
        public virtual Import? Import { get; set; }

        public int BookId { get; set; } // REFERENCE: Link to the Book table
        public virtual Book? Book { get; set; }

        public string Title { get; set; } = string.Empty; // snapshot
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } // SNAPSHOT: The cost from the supplier (import price)
        public decimal SubTotal => Quantity * UnitPrice;

        [NotMapped]
        public string ISBN { get; set; } = string.Empty;
    }
}
