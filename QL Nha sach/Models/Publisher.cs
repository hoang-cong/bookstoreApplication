using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QL_Nha_sach.Models
{
    public class Publisher : ILookup
    {
        [Key]
        public int PublisherId { get; set; }
        public string PublisherName { get; set; } = string.Empty;
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();

        [NotMapped]
        public int Id => PublisherId;
        [NotMapped]
        public string Name
        {
            get => PublisherName;
            set => PublisherName = value;
        }
    }
}
