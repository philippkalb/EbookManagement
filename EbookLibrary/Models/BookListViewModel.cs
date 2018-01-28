using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EbookLibrary.Models
{
    public class BookListViewModel
    {
        public BookListViewModel()
        {
            books = new List<string>();
        }

       public List<string> books { get; set; }
    }
}
