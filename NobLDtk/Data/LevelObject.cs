using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NMotion.Nobelia.Data {
	public struct LevelObject {
		public byte Type;
		public byte Sprite;
		public byte Frame;
		public byte Description;
		public ushort Action;
		public ushort Data;
		public uint Word;
		public short X;
		public short Y;

		public LevelObject(byte type, byte sprite = 0, byte frame = 0, byte description = 0, ushort action = 0, ushort data = 0, uint word = 0, short x = 0, short y = 0) {
			Type = type;
			Sprite = sprite;
			Frame = frame;
			Description = description;
			Action = action;
			Data = data;
			Word = word;
			X = x;
			Y = y;
		}

		public void WriteObject(Stream stream) {
			stream.WriteByte(Type);
			stream.WriteByte(Sprite);
			stream.WriteByte(Frame);
			stream.WriteByte(Description);

			stream.WriteByte((byte)(Action >> 8 & 0xff));
			stream.WriteByte((byte)(Action >> 0 & 0xff));

			stream.WriteByte((byte)(Data >> 8 & 0xff));
			stream.WriteByte((byte)(Data >> 0 & 0xff));

			stream.WriteByte((byte)(Word >> 24 & 0xff));
			stream.WriteByte((byte)(Word >> 16 & 0xff));
			stream.WriteByte((byte)(Word >>  8 & 0xff));
			stream.WriteByte((byte)(Word >>  0 & 0xff));

			stream.WriteByte((byte)(X >> 8 & 0xff));
			stream.WriteByte((byte)(X >> 0 & 0xff));

			stream.WriteByte((byte)(Y >> 8 & 0xff));
			stream.WriteByte((byte)(Y >> 0 & 0xff));
		}
	}
}
