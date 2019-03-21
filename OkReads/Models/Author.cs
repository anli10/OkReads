using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OkReads.Models
{
    public class Author : IEquatable<Author>
    {
        public int AuthorId { get; set; }
        public string Name { get; set; }

        virtual public ICollection<Book> Books { get; set; }

        public Author() { }
        public Author(string name)
        {
            this.Name = name;
        }

        public bool Equals(Author other)
        {
            return this.Name.Equals(other.Name);
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}