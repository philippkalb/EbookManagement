using Nest;
using System;
using System.Collections.Generic;

namespace EbookLibrary.Models
{
    public class BookResult
    {
        public long MaxCount { get; set; }

        public long Displayed { get; set; }

        public List<Book> Books { get; set; }
    }
    

    public class Book
    {
        public Guid? Id { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public List<string> Tags { get; set; }

        public string Filename { get; set; }

        public string Image { get; set; }
    }
}
