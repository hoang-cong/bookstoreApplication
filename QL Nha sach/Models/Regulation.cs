using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QL_Nha_sach.Models
{
    public class Regulation
    {
        [Key]
        public int Id { get; set; } = 1;
        // QĐ2 & QĐ3
        public int MaxStock { get; set; } = 500;   // Toi đa 500
        public int MinStock { get; set; } = 5; // Toi thieu 5
        public int StockThresholdForImport { get; set; } = 300; // Nhap khi ton < 300
        public int MinImport { get; set; } = 150; // Nhap it nhat 150

        // QĐ6
        public int MinStockAfterSale { get; set; } = 5;   // Ton sau ban it nhat 5

        // QĐ5
        public double Discount { get; set; } = 40.0;
    }
}
