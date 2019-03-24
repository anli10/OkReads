using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OkReads.Models
{
    public class Genre : IEquatable<Genre>
    {
        public int GenreId { get; set; }
        public string Name { get; set; }

        virtual public ICollection<Book> Books { get; set; }

        public Genre() { }
        
        public Genre(string name)
        {
            this.Name = name;
        }

        public bool Equals(Genre other)
        {
            return this.Name.Equals(other.Name);
        }
    }
}