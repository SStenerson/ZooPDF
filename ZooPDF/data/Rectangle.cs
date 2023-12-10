using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZooPDF.Data_Classes
{
	public class Rectangle
	{
		public float X { get; set; } // X coordinate of the lower-left corner of the rectangle
		public float Y { get; set; } // Y coordinate of the lower-left corner of the rectangle
		public float Width { get; set; } // Width of the rectangle
		public float Height { get; set; } // Height of the rectangle

		public Rectangle(float x, float y, float width, float height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

	}
}
