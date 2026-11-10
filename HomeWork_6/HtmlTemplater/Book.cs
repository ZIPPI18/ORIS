using System;
using System.Collections.Generic;
using System.Text;

namespace HtmlTemplater
{
    internal class Book
    {
        string Name { get ; set; }
        int Year { get; set; }
        string Author { get; set; }

        public Book(string name, int year, string author)
        {
            Name = name;
            Year = year;
            Author = author;
        }
    }
}
