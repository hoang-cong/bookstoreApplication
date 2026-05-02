using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QL_Nha_sach.Models
{
    public interface ILookup
    {
        public int Id { get; }
        public string Name { get; set; }
    }
}
