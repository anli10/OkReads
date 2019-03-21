using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OkReads.Models
{
    public class Book
    {
        public int BookId { get; set; }
        [Required]
        public string Title { get; set; }
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        public string Isbn { get; set; }
        public float Score { get; set; }
        public string CoverPath { get; set; }

        public virtual Genre Genre { get; set; }
        public virtual ICollection<Author> Authors { get; set; }

        public Book() { }
        public Book(string title, string isbn, string coverPath, Genre genre, ICollection<Author> author)
        {
            this.Title = title;
            this.Isbn = isbn;
            this.CoverPath = coverPath;
            this.Genre = genre;
            this.Authors = author;
        }
    }
}