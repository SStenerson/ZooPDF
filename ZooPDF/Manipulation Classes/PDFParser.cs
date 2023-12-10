using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZooPDF.Aggregate_Classes;
using ZooPDF.Data_Classes;

namespace ZooPDF
{
	public class PDFParser
	{
		public PdfParsingResult ParseXrefAndTrailer(byte[] pdfContent)
		{
			// Find 'startxref' and extract the offset value
			int startxrefOffset = FindStartxrefOffset(pdfContent);

			if (startxrefOffset == -1)
			{
				throw new Exception("startxref not found.");
			}

			// Read and parse the xref table
			XrefTable xref = ReadXrefTable(pdfContent, startxrefOffset);

			// Parse the trailer dictionary
			Trailer trailer = ParseTrailer(pdfContent, startxrefOffset);

			// Return a structure containing parsed xref and trailer
			return new PdfParsingResult { Xref = xref, Trailer = trailer };
		}

		public int FindStartxrefOffset(byte[] pdfContent)
		{
			// Find 'startxref' and extract the offset value
			byte[] startxrefBytes = Encoding.ASCII.GetBytes("startxref");
			int startxrefIndex = LastIndexOf(pdfContent, startxrefBytes);

			if (startxrefIndex == -1)
			{
				return -1; // 'startxref' keyword not found
			}

			// Find the offset value after 'startxref'
			int offsetStartIndex = startxrefIndex + startxrefBytes.Length;
			int endIndex = pdfContent.Length; // Default to the end of the content if newline not found

			for (int i = offsetStartIndex; i < pdfContent.Length; i++)
			{
				if (pdfContent[i] >= '0' && pdfContent[i] <= '9')
				{
					offsetStartIndex = i; // Set the offsetStartIndex to the first numeric digit
					break;
				}
			}

			for (int i = offsetStartIndex; i < pdfContent.Length; i++)
			{
				if (pdfContent[i] == (byte)'\n')
				{
					endIndex = i;
					break; // Found the newline
				}
			}

		
			StringBuilder offsetStringBuilder = new StringBuilder();
			for (int i = offsetStartIndex; i < endIndex; i++)
			{
			
				if (pdfContent[i] >= '0' && pdfContent[i] <= '9')
				{
					offsetStringBuilder.Append((char)pdfContent[i]);
				}
			}

			string offsetString = offsetStringBuilder.ToString().Trim();
			if (int.TryParse(offsetString, out int offset))
			{
				return offset;
			}

			return -1; // Offset not found or invalid
		}


		
		private int LastIndexOf(byte[] source, byte[] target)
		{
			for (int i = source.Length - target.Length; i >= 0; i--)
			{
				bool match = true;
				for (int j = 0; j < target.Length; j++)
				{
					if (source[i + j] != target[j])
					{
						match = false;
						break;
					}
				}
				if (match)
				{
					return i;
				}
			}
			return -1;
		}

		
		private int IndexOf(byte[] source, byte target, int startIndex)
		{
			for (int i = startIndex; i < source.Length; i++)
			{
				if (source[i] == target)
				{
					return i;
				}
			}
			return -1;
		}

		private XrefTable ReadXrefTable(byte[] pdfContent, int offset)
		{
			XrefTable xrefTable = new XrefTable();

			// Process binary data directly starting from the offset
			int length = pdfContent.Length - offset;
			byte[] binaryData = new byte[length];
			Array.Copy(pdfContent, offset, binaryData, 0, length);

			int currentIndex = 0;

			// Process each line
			while (currentIndex < length)
			{
				
				int spaceIndex = Array.IndexOf<byte>(binaryData, (byte)' ', currentIndex);
				if (spaceIndex != -1)
				{
					int startObject;
					int objectCount;

					if (int.TryParse(Encoding.ASCII.GetString(binaryData, currentIndex, spaceIndex - currentIndex), out startObject))
					{
						int nextSpaceIndex = Array.IndexOf<byte>(binaryData, (byte)' ', spaceIndex + 1);
						if (nextSpaceIndex != -1 && int.TryParse(Encoding.ASCII.GetString(binaryData, spaceIndex + 1, nextSpaceIndex - spaceIndex - 1), out objectCount))
						{
							currentIndex = nextSpaceIndex + 1; 

							for (int i = 0; i < objectCount && currentIndex < length; i++)
							{
								int lineEndIndex = Array.IndexOf<byte>(binaryData, (byte)'\n', currentIndex);
								if (lineEndIndex != -1)
								{
									string entry = Encoding.ASCII.GetString(binaryData, currentIndex, lineEndIndex - currentIndex).Trim();
									string[] entryParts = entry.Split(' ');

									if (entryParts.Length >= 3)
									{
										int objectNumber = startObject + i;
										int byteOffset = int.Parse(entryParts[0]);
										int generationNumber = int.Parse(entryParts[1]);
										bool inUse = entryParts[2] == "n";

										xrefTable.AddEntry(objectNumber, byteOffset, generationNumber, inUse);
									}

									currentIndex = lineEndIndex + 1;
								}
								else
								{
									// Handle case where line is not complete
									break;
								}
							}
						}
						else
						{
							// Handle case where objectCount cannot be parsed
							break;
						}
					}
					else
					{
						// Handle case where startObject cannot be parsed
						break;
					}
				}
				else
				{
					// Handle case where spaceIndex is not found
					break;
				}
			}

			return xrefTable;
		}


		private Trailer ParseTrailer(byte[] pdfContent, int offset)
		{
			// Convert the relevant part of the PDF content to a string
			string content = Encoding.ASCII.GetString(pdfContent, offset, pdfContent.Length - offset);

			// Find the start of the trailer dictionary
			int trailerDictStart = content.IndexOf("trailer") + "trailer".Length;
			int trailerDictEnd = content.IndexOf("startxref", trailerDictStart);
			if (trailerDictStart == -1 || trailerDictEnd == -1)
			{
				throw new Exception("Trailer dictionary not found.");
			}

			string trailerContent = content.Substring(trailerDictStart, trailerDictEnd - trailerDictStart).Trim();

			var trailerDict = ParsePdfDictionary(trailerContent);

			// Extracting necessary information from the dictionary
			int size = ExtractIntFromDict(trailerDict, "Size");
			int rootObjNumber = ExtractObjNumberFromDict(trailerDict, "Root");
			int rootGenNumber = ExtractGenNumberFromDict(trailerDict, "Root");
			int prevXrefOffset = ExtractIntFromDict(trailerDict, "Prev", optional: true);

			return new Trailer(size, rootObjNumber, rootGenNumber, prevXrefOffset);
		}

		private Dictionary<string, string> ParsePdfDictionary(string dictContent)
		{
			var dict = new Dictionary<string, string>();
			int dictStart = dictContent.IndexOf("<<");
			int dictEnd = dictContent.IndexOf(">>", dictStart + 2);
			if (dictStart == -1 || dictEnd == -1)
			{
				throw new Exception("Invalid PDF dictionary format.");
			}

			string dictEntries = dictContent.Substring(dictStart + 2, dictEnd - dictStart - 2).Trim();
			var entries = dictEntries.Split(new[] { "\r\n", "\r", "\n", " " }, StringSplitOptions.RemoveEmptyEntries);

			for (int i = 0; i < entries.Length; i += 2)
			{
				if (i + 1 < entries.Length)
				{
					dict[entries[i].Trim('/')] = entries[i + 1];
				}
			}

			return dict;
		}

		private int ExtractIntFromDict(Dictionary<string, string> dict, string key, bool optional = false)
		{
			if (dict.TryGetValue(key, out string value))
			{
				if (int.TryParse(value, out int intValue))
				{
					return intValue;
				}
				throw new FormatException($"Invalid integer format for key '{key}'.");
			}

			if (!optional)
			{
				throw new KeyNotFoundException($"Required key '{key}' not found in the trailer dictionary.");
			}

			return 0;
		}
		private int ExtractObjNumberFromDict(Dictionary<string, string> dict, string key)
		{
			if (dict.TryGetValue(key, out string value))
			{
				var parts = value.Split(' ');
				if (parts.Length >= 2 && int.TryParse(parts[0], out int objNumber))
				{
					return objNumber;
				}
			}

			throw new Exception($"Object number for key '{key}' not found or invalid.");
		}

		private int ExtractGenNumberFromDict(Dictionary<string, string> dict, string key)
		{
			if (dict.TryGetValue(key, out string value))
			{
				var parts = value.Split(' ');
				if (parts.Length >= 2 && int.TryParse(parts[1], out int genNumber))
				{
					return genNumber;
				}
			}

			throw new Exception($"Generation number for key '{key}' not found or invalid.");
		}

		public Dictionary<string, string> LocateAcroFormDictionary(byte[] pdfContent, Trailer trailer, XrefTable xref)
		{
			// Get the byte offset of the Root object from the trailer and xref table
			int rootObjectOffset = GetObjectOffset(trailer.RootObjectNumber, xref);

			// Extract the content of the Root object
			string rootObjectContent = ExtractObjectContent(pdfContent, rootObjectOffset);

			// Parse the Root object content to find the reference to the AcroForm dictionary
			string acroFormReference = ParseRootForAcroFormReference(rootObjectContent);

			if (string.IsNullOrEmpty(acroFormReference))
			{
				throw new Exception("AcroForm dictionary not found in Root object.");
			}

			// Extract the AcroForm dictionary object using its reference
			int acroFormObjectNumber = ExtractObjectNumber(acroFormReference);
			int acroFormObjectOffset = GetObjectOffset(acroFormObjectNumber, xref);
			string acroFormContent = ExtractObjectContent(pdfContent, acroFormObjectOffset);

			// Parse and return the AcroForm dictionary
			return ParsePdfDictionary(acroFormContent);
		}

		private int GetObjectOffset(int objectNumber, XrefTable xref)
		{
			XrefEntry entry = xref.GetEntry(objectNumber);
			if (entry == null || !entry.InUse)
			{
				throw new Exception($"Object number {objectNumber} not found in xref table or not in use.");
			}
			return entry.ByteOffset;
		}

		private string ExtractObjectContent(byte[] pdfContent, int offset)
		{
			// Move to the object start
			int start = Array.IndexOf(pdfContent, (byte)'o', offset) + 1;
			int end = Array.IndexOf(pdfContent, (byte)'e', start) - 1;

			// Ensure indices are within bounds
			if (start < 0 || end < 0 || end < start)
			{
				throw new Exception("Invalid object indices.");
			}

			// Extract object content and convert it to a string
			byte[] objectData = new byte[end - start];
			Array.Copy(pdfContent, start, objectData, 0, end - start);

			// Encryption handling here

			return Encoding.ASCII.GetString(objectData);
		}

		private string ParseRootForAcroFormReference(string rootObjectContent)
		{
			const string acroFormKey = "/AcroForm";
			int acroFormIndex = rootObjectContent.IndexOf(acroFormKey);
			if (acroFormIndex == -1)
			{
				Console.WriteLine("AcroForm Dictionary not found");
				return null; // AcroForm dictionary not found
			}

			// Move past the key to the reference
			int refStart = acroFormIndex + acroFormKey.Length;
			int refEnd = rootObjectContent.IndexOfAny(new[] { ' ', '\r', '\n' }, refStart);
			string acroFormRef = rootObjectContent.Substring(refStart, refEnd - refStart).Trim();

			return acroFormRef;
		}

		private int ExtractObjectNumber(string reference)
		{
			var parts = reference.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length >= 1 && int.TryParse(parts[0], out int objectNumber))
			{
				return objectNumber;
			}

			throw new Exception($"Invalid object reference: '{reference}'.");
		}

		public List<FormField> ExtractFormFields(byte[] pdfContent, Dictionary<string, string> acroFormDict, XrefTable xref)
		{
			List<FormField> formFields = new List<FormField>();

			if (!acroFormDict.TryGetValue("/Fields", out string fieldsArrayRef))
			{
				throw new Exception("Form fields array not found in AcroForm dictionary.");
			}

			int fieldsArrayObjectNumber = ExtractObjectNumber(fieldsArrayRef);
			int fieldsArrayOffset = GetObjectOffset(fieldsArrayObjectNumber, xref);
			string fieldsArrayContent = ExtractObjectContent(pdfContent, fieldsArrayOffset);

			foreach (var fieldRef in ExtractFieldReferences(fieldsArrayContent, xref))
			{
				string fieldContent = ExtractObjectContent(pdfContent, fieldRef);
				FormField formField = ParseFormField(fieldContent);
				formFields.Add(formField);
			}

			return formFields;
		}

		private IEnumerable<int> ExtractFieldReferences(string fieldsArrayContent, XrefTable xref)
		{
			var fieldRefs = new List<int>();

			int startIdx = fieldsArrayContent.IndexOf('[') + 1;
			int endIdx = fieldsArrayContent.IndexOf(']', startIdx);
			string arrayContent = fieldsArrayContent.Substring(startIdx, endIdx - startIdx);

			var refs = arrayContent.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var r in refs)
			{
				if (r.EndsWith("R") && int.TryParse(r.Split(' ')[0], out int objNum))
				{
					int objOffset = GetObjectOffset(objNum, xref);
					fieldRefs.Add(objOffset);
				}
			}

			return fieldRefs;
		}

		private FormField ParseFormField(string fieldContent)
		{
			var fieldDict = ParsePdfDictionary(fieldContent);
			var formField = new FormField();

			if (fieldDict.TryGetValue("/T", out string name))
				formField.Name = name;
			if (fieldDict.TryGetValue("/FT", out string type))
				formField.Type = type;

			if (fieldDict.TryGetValue("/Rect", out string rectStr))
				formField.Location = ParseRectangle(rectStr);

			return formField;
		}

		private Data_Classes.Rectangle ParseRectangle(string rectStr)
		{
			var rectParts = rectStr.Trim('[', ']').Split(' ');
			if (rectParts.Length == 4 &&
				float.TryParse(rectParts[0], out float llx) &&
				float.TryParse(rectParts[1], out float lly) &&
				float.TryParse(rectParts[2], out float urx) &&
				float.TryParse(rectParts[3], out float ury))
			{
				return new Data_Classes.Rectangle(llx, lly, urx - llx, ury - lly);
			}
				throw new Exception("Invalid Rect format.");
		}


	}
}
