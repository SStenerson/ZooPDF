using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZooPDF.Data_Classes
{
	public class Trailer
	{
		public int Size { get; private set; } // Total number of entries in the xref table
		public int RootObjectNumber { get; private set; } // Object number of the root object
		public int RootGenerationNumber { get; private set; } // Generation number of the root object
		public int PrevXrefOffset { get; private set; } // Byte offset of the previous xref table (if any)

		// Additional properties as per the PDF specification
		public Dictionary<string, object> AdditionalInfo { get; private set; }

		// Constructor for initializing the Trailer object
		public Trailer(int size, int rootObjectNumber, int rootGenerationNumber, int prevXrefOffset)
		{
			Size = size;
			RootObjectNumber = rootObjectNumber;
			RootGenerationNumber = rootGenerationNumber;
			PrevXrefOffset = prevXrefOffset;
			AdditionalInfo = new Dictionary<string, object>();
		}

		// Method to update the essential trailer properties
		public void UpdateTrailer(int size, int rootObjectNumber, int rootGenerationNumber, int prevXrefOffset)
		{
			Size = size;
			RootObjectNumber = rootObjectNumber;
			RootGenerationNumber = rootGenerationNumber;
			PrevXrefOffset = prevXrefOffset;
		}

		// Method to add or update additional trailer information
		public void AddOrUpdateInfo(string key, object value)
		{
			if (string.IsNullOrWhiteSpace(key))
			{
				throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));
			}

			AdditionalInfo[key] = value;
		}

		// Method to get additional trailer information
		public object GetInfo(string key)
		{
			if (AdditionalInfo.TryGetValue(key, out object value))
			{
				return value;
			}

			return null;
		}
	}
}
