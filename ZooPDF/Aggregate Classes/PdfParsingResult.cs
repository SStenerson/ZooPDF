using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZooPDF.Data_Classes;

namespace ZooPDF.Aggregate_Classes
{
	public class PdfParsingResult
	{
		public XrefTable Xref { get; set; }
		public Trailer Trailer { get; set; }
	}
}
