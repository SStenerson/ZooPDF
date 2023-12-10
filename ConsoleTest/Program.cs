using ZooPDF;
using ZooPDF.Aggregate_Classes;

internal class Program
{
	private static void Main(string[] args)
	{

		string testPdfPath = @"path";

		PDFParser pdfParser = new PDFParser();
		PDFReader pdfReader = new PDFReader();

		try
		{

			byte[] pdfContent = pdfReader.ReadPdfFile(testPdfPath);

	
			PdfParsingResult parsingResult = pdfParser.ParseXrefAndTrailer(pdfContent);

		
			var acroFormDictionary = pdfParser.LocateAcroFormDictionary(pdfContent, parsingResult.Trailer, parsingResult.Xref);

		
			var formFields = pdfParser.ExtractFormFields(pdfContent, acroFormDictionary, parsingResult.Xref);

		
			Console.WriteLine("Form Fields:");
			foreach (var formField in formFields)
			{
				Console.WriteLine($"Field Name: {formField.Name}");
				Console.WriteLine($"Field Type: {formField.Type}");
				Console.WriteLine($"Location (Rect): {formField.Location}");
				Console.WriteLine();
			}

			Console.WriteLine("Press any key to exit...");
			Console.ReadKey();
		}
		catch (Exception ex)
		{
			Console.WriteLine($"An error occurred: {ex.Message}");
		}

	}
}