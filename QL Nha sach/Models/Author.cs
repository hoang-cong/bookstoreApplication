using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QL_Nha_sach.Models
{
    public class Author : ILookup
    {
        [Key]
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;

        // REQUIRED for WithMany(a => a.Books)
        public virtual ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();

        [NotMapped]
        public int Id => AuthorId;

        [NotMapped]
        public string Name
        {
            get => AuthorName;
            set => AuthorName = value;
        }

        [NotMapped]
        public bool IsSelected { get; set; }
    }
}
