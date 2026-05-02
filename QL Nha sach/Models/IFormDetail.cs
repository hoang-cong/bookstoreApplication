using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QL_Nha_sach.Models
{
    public interface IFormDetail
    {
        public int BookId { get; set; }       // Which book
        public string Title { get; set; }     // Snapshot of title
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } // ImportPrice or Invoice UnitPrice
        public decimal SubTotal { get; }      // Calculated line total
    }
}
