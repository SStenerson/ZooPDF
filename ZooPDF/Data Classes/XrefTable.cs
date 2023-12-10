using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZooPDF.Data_Classes
{
	public class XrefTable
	{
		private readonly Dictionary<int, XrefEntry> _entries;

		public XrefTable()
		{
			_entries = new Dictionary<int, XrefEntry>();
		}

		public void AddEntry(int objectNumber, int byteOffset, int generationNumber, bool inUse)
		{
			var entry = new XrefEntry(objectNumber, byteOffset, generationNumber, inUse);
			_entries[objectNumber] = entry;
		}

		public XrefEntry GetEntry(int objectNumber)
		{
			if (_entries.TryGetValue(objectNumber, out XrefEntry entry))
			{
				return entry;
			}
			return null;
		}

		public IEnumerable<XrefEntry> GetAllEntries()
		{
			return _entries.Values;
		}
	}
}
