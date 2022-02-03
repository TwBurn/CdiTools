using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMotion.Cdi.Graphics {
	public class RleEntry {
		public int Count;
		public byte Color;

		public RleEntry(byte color) {
			Count = 1;
			Color = color;
		}
	}
}
