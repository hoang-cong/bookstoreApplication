using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QL_Nha_sach.Models
{
    public class Promotion
    {
        [Key]
        public int PromotionId { get; set; }
        public string PromotionName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double Discount { get; set; }

        public virtual ICollection<PromotionTarget> PromotionTargets { get; set; } = new List<PromotionTarget>();
    }
}
