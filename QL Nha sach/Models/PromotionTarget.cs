using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QL_Nha_sach.Models
{
    public class PromotionTarget
    {
        public int PromotionId { get; set; }
        public virtual Promotion? Promotion { get; set; }

        public int BookId { get; set; }
        public virtual Book? Book { get; set; }
    }
}
