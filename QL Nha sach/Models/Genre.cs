using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QL_Nha_sach.Models
{
    public class Genre : ILookup
    {
        [Key]
        public int GenreId { get; set; }
        public string GenreName { get; set; } = string.Empty;
        public virtual ICollection<BookGenre> BookGenres { get; set; } = new List<BookGenre>();

        [NotMapped]
        public int Id => GenreId;
        
        [NotMapped]
        public string Name
        {
            get => GenreName;
            set => GenreName = value;
        }
        [NotMapped]
        public bool IsSelected { get; set; }
    }
}
