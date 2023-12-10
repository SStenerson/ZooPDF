using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZooPDF.Data_Classes
{
	public class XrefEntry
	{
		public int ObjectNumber { get; set; }
		public int ByteOffset { get; set; }
		public int GenerationNumber { get; set; }
		public bool InUse { get; set; } // 'n' for in-use, 'f' for free

		public XrefEntry(int objectNumber, int byteOffset, int generationNumber, bool inUse)
		{
			ObjectNumber = objectNumber;
			ByteOffset = byteOffset;
			GenerationNumber = generationNumber;
			InUse = inUse;
		}
	}
}
