using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OkReads.Models
{
    public enum BookStatus { ToRead, Reading, Read }

    public class LibraryEntry
    {
        public int LibraryEntryId { get; set; }
        public BookStatus BookStatus { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual Book Book { get; set; }

        public LibraryEntry() { }

        public LibraryEntry(ApplicationUser user, Book book, BookStatus status)
        {
            User = user;
            Book = book;
            BookStatus = status;
        }
    }
}