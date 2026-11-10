using System;
using System.Collections.Generic;
using System.Text;

namespace HtmlTemplater
{
    internal class Pen
    {
        string Name { get; set; }

        string Color { get; set; }

        int Width { get; set; }

        int Price { get; set; }

        public Pen(string name, string color, int width, int price)
        {
            Name = name;
            Color = color;
            Width = width;
            Price = price;
        }
    }
}
