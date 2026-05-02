using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QL_Nha_sach.Models
{
    public class BookGenre
    {
        [Key, Column(Order = 0)]
        public int BookId { get; set; }
        public virtual Book? Book { get; set; }

        [Key, Column(Order = 1)]
        public int GenreId { get; set; }
        public virtual Genre? Genre { get; set; }
    }
}
