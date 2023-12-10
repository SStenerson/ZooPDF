using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZooPDF.Data_Classes
{
	public class FormField
	{
		public string Name { get; set; }
		public string Type { get; set; }
		public Rectangle Location { get; set; } // The rectangle specifying the field's position on the page
		public string Value { get; set; }
		public bool IsReadOnly { get; set; } 
		public bool IsRequired { get; set; }
		public bool IsVisible { get; set; }

		//font, font size, color, etc...?
	}
}
