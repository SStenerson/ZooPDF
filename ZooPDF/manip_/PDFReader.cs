using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZooPDF
{
	public class PDFReader
	{
		public byte[] ReadPdfFile(string filePath)
		{
			try
			{
				byte[] pdfBytes = File.ReadAllBytes(filePath);
				return pdfBytes;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error reading PDF file: " + ex.Message);
				return null;
			}
		}
	}
}
